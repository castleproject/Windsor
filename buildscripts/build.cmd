@ECHO OFF
REM ****************************************************************************
REM Copyright 2004-2013 Castle Project - http://www.castleproject.org/
REM Licensed under the Apache License, Version 2.0 (the "License");
REM you may not use this file except in compliance with the License.
REM You may obtain a copy of the License at
REM 
REM     http://www.apache.org/licenses/LICENSE-2.0
REM 
REM Unless required by applicable law or agreed to in writing, software
REM distributed under the License is distributed on an "AS IS" BASIS,
REM WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
REM See the License for the specific language governing permissions and
REM limitations under the License.
REM ****************************************************************************

if "%1" == "" goto no_config 
if "%1" NEQ "" goto set_config 

:set_config
SET Configuration=%1
GOTO restore_packages

:no_config
SET Configuration=Release
GOTO restore_packages

:restore_packages
dotnet restore ./src/Castle.Windsor/Castle.Windsor.csproj
dotnet restore ./src/Castle.Facilities.EventWiring/Castle.Facilities.EventWiring.csproj
dotnet restore ./src/Castle.Facilities.FactorySupport/Castle.Facilities.FactorySupport.csproj
dotnet restore ./src/Castle.Facilities.Logging/Castle.Facilities.Logging.csproj
dotnet restore ./src/Castle.Facilities.Synchronize/Castle.Facilities.Synchronize.csproj
dotnet restore ./src/Castle.Facilities.WcfIntegration/Castle.Facilities.WcfIntegration.csproj
dotnet restore ./src/Castle.Facilities.WcfIntegration.Demo/Castle.Facilities.WcfIntegration.Demo.csproj
dotnet restore ./src/Castle.Facilities.WcfIntegration.Tests/Castle.Facilities.WcfIntegration.Tests.csproj
dotnet restore ./src/Castle.Windsor.Tests/Castle.Windsor.Tests.csproj

GOTO build

:build
dotnet build Castle.Windsor.sln -c %Configuration%
GOTO test

:test
SET nunitConsole=%UserProfile%\.nuget\packages\nunit.consolerunner\3.6.1\tools\nunit3-console.exe
SET testBin=bin\%Configuration%\net45
%nunitConsole% src\Castle.Windsor.Tests\%testBin%\Castle.Windsor.Tests.dll --result=src\Castle.Windsor.Tests\%testBin%\TestResult_Windsor.xml
SET ERRORCOUNT=%errorlevel%
%nunitConsole% src\Castle.Facilities.WcfIntegration.Tests\%testBin%\Castle.Facilities.WcfIntegration.Tests.dll --result=src\Castle.Facilities.WcfIntegration.Tests\%testBin%\TestResult_WcfIntegration.xml
SET /A ERRORCOUNT=ERRORCOUNT+%errorlevel%

EXIT /B %ERRORCOUNT%


