﻿// 
//  LogTest.cs
// 
//  Copyright (c) 2018 Couchbase, Inc All rights reserved.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Couchbase.Lite;
using Couchbase.Lite.Internal.Logging;
using Couchbase.Lite.Logging;

using FluentAssertions;

using JetBrains.Annotations;
#if !WINDOWS_UWP
using Xunit;
using Xunit.Abstractions;
#else
using Fact = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif

namespace Test
{
#if WINDOWS_UWP
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
#endif
    public sealed class LogTest
    {
#if NETCOREAPP2_0
        static LogTest()
        {
            Couchbase.Lite.Support.NetDesktop.Activate();
        }
#endif

        [Fact]
        public void TestDefaultLogLocation()
        {
            var logDirectory = Database.Log.File.Directory;
            WriteLog.To.Database.I("TEST", "MESSAGE");
            Directory.EnumerateFiles(logDirectory, "*.cbllog").Count().Should()
                .BeGreaterOrEqualTo(5, "because there should be at least 5 log entries in the folder");
        }

        [Fact]
        public void TestDefaultLogFormat()
        {
            // Can't test all files because there might be some plaintext ones leftover from previous runs
            // and/or tests
            var logDirectory = Database.Log.File.Directory;
            WriteLog.To.Database.I("TEST", "MESSAGE");
            var logFilePath = Directory.EnumerateFiles(logDirectory, "cbl_info_*").LastOrDefault();
            logFilePath.Should().NotBeNullOrEmpty();
            var logContent = ReadAllBytes(logFilePath);
            logContent.Should().StartWith(new byte[] { 0xcf, 0xb2, 0xab, 0x1b },
                "because the log should be in binary format");
        }

        [Fact]
        public void TestPlaintext()
        {
            try {
                // Can't test all files because there might be some plaintext ones leftover from previous runs
                // and/or tests
                var logDirectory = Database.Log.File.Directory;
                Database.Log.File.UsePlaintext = true;
                WriteLog.To.Database.I("TEST", "MESSAGE");
                var logFilePath = Directory.EnumerateFiles(logDirectory, "cbl_info_*").LastOrDefault();
                logFilePath.Should().NotBeNullOrEmpty();
                var logContent = ReadAllLines(logFilePath);
                logContent.Any(x => x.Contains("MESSAGE") && x.Contains("TEST"))
                    .Should().BeTrue("because the message should show up in plaintext");
            } finally {
                Database.Log.File.UsePlaintext = false;
            }
        }

        [Fact]
        public void TestConsoleLoggingLevels()
        {
            WriteLog.To.Database.I("IGNORE", "IGNORE"); // Skip initial message
            Database.Log.Console.Level = LogLevel.None;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            WriteLog.To.Database.E("TEST", "TEST ERROR");
            stringWriter.Flush();
            stringWriter.ToString().Should().BeEmpty("because logging is disabled");

            Database.Log.Console.Level = LogLevel.Verbose;
            stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            WriteLog.To.Database.V("TEST", "TEST VERBOSE");
            WriteLog.To.Database.I("TEST", "TEST INFO");
            WriteLog.To.Database.W("TEST", "TEST WARNING");
            WriteLog.To.Database.E("TEST", "TEST ERROR");
            stringWriter.Flush();
            stringWriter.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Should()
                .HaveCount(4, "because all levels should be logged");

            var currentCount = 1;
            foreach (var level in new[] { LogLevel.Error, LogLevel.Warning, 
                LogLevel.Info}) {
                Database.Log.Console.Level = level;
                stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                WriteLog.To.Database.V("TEST", "TEST VERBOSE");
                WriteLog.To.Database.I("TEST", "TEST INFO");
                WriteLog.To.Database.W("TEST", "TEST WARNING");
                WriteLog.To.Database.E("TEST", "TEST ERROR");
                stringWriter.Flush();
                stringWriter.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Should()
                    .HaveCount(currentCount, "because {0} levels should be logged for {1}", currentCount, level);
                currentCount++;
            }

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
        }

        [Fact]
        public void TestConsoleLoggingDomains()
        {
            WriteLog.To.Database.I("IGNORE", "IGNORE"); // Skip initial message
            Database.Log.Console.Domains = LogDomain.None;
            Database.Log.Console.Level = LogLevel.Info;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            foreach (var domain in WriteLog.To.All) {
                domain.I("TEST", "TEST MESSAGE");
            }

            stringWriter.Flush();
            stringWriter.ToString().Should().BeEmpty("because all domains are disabled");
            foreach (var domain in WriteLog.To.All) {
                Database.Log.Console.Domains = domain.Domain;
                stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                foreach (var d in WriteLog.To.All) {
                    d.I("TEST", "TEST MESSAGE");
                }

                stringWriter.Flush();
                stringWriter.ToString().Should().Match(x => x.Contains(domain.Domain.ToString()));
            }

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
        }

        [Fact]
        public void TestCustomLoggingLevels()
        {
            var customLogger = new LogTestLogger();
            WriteLog.To.Database.I("IGNORE", "IGNORE"); // Skip initial message
            Database.Log.Custom = customLogger;
            customLogger.Level = LogLevel.None;
            WriteLog.To.Database.E("TEST", "TEST ERROR");
            customLogger.Lines.Should().BeEmpty("because logging level is set to None");
            

            customLogger.Level = LogLevel.Verbose;
            WriteLog.To.Database.V("TEST", "TEST VERBOSE");
            WriteLog.To.Database.I("TEST", "TEST INFO");
            WriteLog.To.Database.W("TEST", "TEST WARNING");
            WriteLog.To.Database.E("TEST", "TEST ERROR");
            customLogger.Lines.Should().HaveCount(4, "because all levels should be logged");
            customLogger.Reset();;

            var currentCount = 1;
            foreach (var level in new[] { LogLevel.Error, LogLevel.Warning, 
                LogLevel.Info}) {
                customLogger.Level = level;
                WriteLog.To.Database.V("TEST", "TEST VERBOSE");
                WriteLog.To.Database.I("TEST", "TEST INFO");
                WriteLog.To.Database.W("TEST", "TEST WARNING");
                WriteLog.To.Database.E("TEST", "TEST ERROR");
                customLogger.Lines.Should()
                    .HaveCount(currentCount, "because {0} levels should be logged for {1}", currentCount, level);
                currentCount++;
                customLogger.Reset();
            }
        }

        [Fact]
        public void TestPlaintextLoggingLevels()
        {
            WriteLog.To.Database.I("IGNORE", "IGNORE"); // Skip initial message
            var logPath = Path.Combine(Path.GetTempPath(), "LogTestLogs");
            Directory.CreateDirectory(logPath);
            Database.Log.File.UsePlaintext = true;
            Database.Log.File.Directory = logPath;
            Database.Log.File.MaxRotateCount = 0;

            foreach (var level in new[]
            { LogLevel.None, LogLevel.Error, LogLevel.Warning, LogLevel.Info, LogLevel.Verbose }) {
                Database.Log.File.Level = level;
                WriteLog.To.Database.V("TEST", "TEST VERBOSE");
                WriteLog.To.Database.I("TEST", "TEST INFO");
                WriteLog.To.Database.W("TEST", "TEST WARNING");
                WriteLog.To.Database.E("TEST", "TEST ERROR");
            }

            try {
                foreach (var file in Directory.EnumerateFiles(logPath)) {
                    if (file.Contains(LogLevel.Verbose.ToString().ToLowerInvariant())) {
                        ReadAllLines(file).Should()
                            .HaveCount(2, "because there should be 1 log line and 1 meta line");
                    } else if (file.Contains(LogLevel.Info.ToString().ToLowerInvariant())) {
                        ReadAllLines(file).Should()
                            .HaveCount(3, "because there should be 2 log lines and 1 meta line");
                    } else if (file.Contains(LogLevel.Warning.ToString().ToLowerInvariant())) {
                        ReadAllLines(file).Should()
                            .HaveCount(4, "because there should be 3 log lines and 1 meta line");
                    } else if (file.Contains(LogLevel.Error.ToString().ToLowerInvariant())) {
                        ReadAllLines(file).Should()
                            .HaveCount(5, "because there should be 4 log lines and 1 meta line");
                    }
                }
            } finally {
                Database.Log.File.UsePlaintext = false;
                Database.Log.File.Directory = null;
                Directory.Delete(logPath, true);
            }
        }

        private static string[] ReadAllLines(string path)
        {
            var lines = new List<string>();
            using(var fin = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fin)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }

        private static byte[] ReadAllBytes(string path)
        {
            using(var fin = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new BinaryReader(fin)) {
                var retVal = new byte[fin.Length];
                reader.Read(retVal, 0, retVal.Length);
                return retVal;
            }
        }

        private class LogTestLogger : ILogger
        {
            [NotNull]
            private readonly List<string> _lines = new List<string>();

            public IReadOnlyList<string> Lines => _lines;
            public LogLevel Level { get; set; }

            public void Reset()
            {
                _lines.Clear();
            }

            public void Log(LogLevel level, LogDomain domain, string message)
            {
                if (level >= Level) {
                    _lines.Add(message);
                }
            }
        }
    }
}
