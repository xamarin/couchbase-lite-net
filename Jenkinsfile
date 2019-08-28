pipeline {
    agent any
    stages {
        stage("Entry") {
            parallel {
                stage("Windows Node") {
                    agent { label 's61114win10_(litecore)' }
                    environment {
                        NEXUS_REPO="http://nexus.build.couchbase.com:8081/nexus/content/repositories/releases/com/couchbase/litecore"
                    }
                    stages {
                        stage("Checkout") {
                            steps {
                                powershell '''
                                New-Item -Type Directory tmp
                                Get-ChildItem -Path $pwd -Exclude "tmp" -Force | Move-Item -Destination "tmp"
                                & 'C:\\Program Files\\Git\\bin\\git.exe' clone git@github.com:couchbaselabs/couchbase-lite-net-ee --branch $env:CHANGE_TARGET --depth 1
                                Get-ChildItem couchbase-lite-net-ee\\* -Force | Move-Item -Destination .
                                Get-ChildItem -Force tmp\\* | Move-Item -Destination couchbase-lite-net
                                Remove-Item tmp

                                Push-Location couchbase-lite-net
                                & 'C:\\Program Files\\Git\\bin\\git.exe' submodule update --init
                                Pop-Location

                                Push-Location jenkins
                                & 'C:\\Program Files\\Git\\bin\\git.exe' clone https://github.com/couchbaselabs/couchbase-lite-net-validation --depth 1 proj
                                Pop-Location
                                '''
                            }
                        }
                        stage(".NET Core Windows") {
                            steps {
                                powershell 'jenkins\\run_win_tests.ps1'
                            }
                        }
                        // Note: Jenkins is a PITA for this, so I will remove UWP from PR
                        // validation, but it will get checked in post-commit
                        // stage("UWP") {
                        //     steps {
                        //         powershell 'jenkins\\run_uwp_tests.ps1'
                        //     }
                        // }
                    }
                }
                stage("Linux Node") {
                    agent { label 's61113u16 (litecore)' }
                    environment {
                        NEXUS_REPO="http://nexus.build.couchbase.com:8081/nexus/content/repositories/releases/com/couchbase/litecore"
                    }
                    stages {
                        stage("Checkout") {
                            steps {
                                sh '''#!/bin/bash
                                set -e
                                shopt -s extglob dotglob
                                mkdir tmp
                                mv !(tmp) tmp
                                git clone git@github.com:couchbaselabs/couchbase-lite-net-ee --branch $CHANGE_TARGET --depth 1
                                mv couchbase-lite-net-ee/* .
                                mv tmp/* couchbase-lite-net
                                rmdir tmp

                                pushd couchbase-lite-net
                                git submodule update --init
                                popd

                                pushd jenkins
                                git clone https://github.com/couchbaselabs/couchbase-lite-net-validation --depth 1 proj
                                popd
                                '''
                            }
                        }
                        stage(".NET Core Linux") {
                            steps {
                                sh 'jenkins/run_unix_tests.sh'
                            }
                        }
                    }
                }
                stage("Mac Node") {
                    agent { label 'mobile-mac-mini'  }
                    environment {
                        NEXUS_REPO="http://nexus.build.couchbase.com:8081/nexus/content/repositories/releases/com/couchbase/litecore"
                    }
                    stages {
                        stage("Checkout") {
                            steps {
                                sh '''#!/bin/bash
                                set -e
                                shopt -s extglob dotglob
                                mkdir tmp
                                mv !(tmp) tmp
                                git clone git@github.com:couchbaselabs/couchbase-lite-net-ee --branch $CHANGE_TARGET --depth 1
                                mv couchbase-lite-net-ee/* .
                                mv tmp/* couchbase-lite-net
                                rmdir tmp

                                pushd couchbase-lite-net
                                git submodule update --init
                                popd

                                pushd jenkins
                                git clone https://github.com/couchbaselabs/couchbase-lite-net-validation --depth 1 proj
                                popd
                                '''
                            }
                        }
                        stage(".NET Core Mac") {
                            steps {
                                sh 'jenkins/run_unix_tests.sh'
                            }
                        }
                        stage("Xamarin Android") {
                            steps {
                                sh 'jenkins/run_android_tests.sh'
                            }
                        }
                        stage("Xamarin iOS") {
                            steps {
                                sh 'jenkins/run_ios_tests.sh'
                            }
                        }
                    }
                }
            }
        }
    }   
}