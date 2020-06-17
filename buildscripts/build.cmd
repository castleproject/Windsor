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
dotnet restore ./tools/Explicit.NuGet.Versions/Explicit.NuGet.Versions.csproj
dotnet restore ./buildscripts/BuildScripts.csproj
dotnet restore ./src/Castle.Windsor/Castle.Windsor.csproj
dotnet restore ./src/Castle.Facilities.Logging/Castle.Facilities.Logging.csproj
dotnet restore ./src/Castle.Facilities.AspNet.SystemWeb/Castle.Facilities.AspNet.SystemWeb.csproj
dotnet restore ./src/Castle.Facilities.AspNet.SystemWeb.Tests/Castle.Facilities.AspNet.SystemWeb.Tests.csproj
dotnet restore ./src/Castle.Facilities.AspNet.Mvc/Castle.Facilities.AspNet.Mvc.csproj
dotnet restore ./src/Castle.Facilities.AspNet.Mvc.Tests/Castle.Facilities.AspNet.Mvc.Tests.csproj
dotnet restore ./src/Castle.Facilities.AspNet.WebApi/Castle.Facilities.AspNet.WebApi.csproj
dotnet restore ./src/Castle.Facilities.AspNet.WebApi.Tests/Castle.Facilities.AspNet.WebApi.Tests.csproj
dotnet restore ./src/Castle.Facilities.AspNetCore/Castle.Facilities.AspNetCore.csproj
dotnet restore ./src/Castle.Facilities.AspNetCore.Tests/Castle.Facilities.AspNetCore.Tests.csproj
dotnet restore ./src/Castle.Windsor.Extensions.DependencyInjection.Tests/Castle.Windsor.Extensions.DependencyInjection.Tests.csproj
dotnet restore ./src/Castle.Windsor.Extensions.DependencyInjection/Castle.Windsor.Extensions.DependencyInjection.csproj
dotnet restore ./src/Castle.Facilities.WcfIntegration/Castle.Facilities.WcfIntegration.csproj
dotnet restore ./src/Castle.Facilities.WcfIntegration.Demo/Castle.Facilities.WcfIntegration.Demo.csproj
dotnet restore ./src/Castle.Facilities.WcfIntegration.Tests/Castle.Facilities.WcfIntegration.Tests.csproj
dotnet restore ./src/Castle.Windsor.Tests/Castle.Windsor.Tests.csproj


GOTO build

:build
dotnet build ./tools/Explicit.NuGet.Versions/Explicit.NuGet.Versions.sln
dotnet build Castle.Windsor.sln -c %Configuration%
GOTO test

:test

echo -------------
echo Running Tests
echo -------------

dotnet test src\Castle.Windsor.Tests || exit /b 1
dotnet test src\Castle.Windsor.Extensions.DependencyInjection.Tests || exit /b 1
dotnet test src\Castle.Facilities.AspNetCore.Tests || exit /b 1
dotnet test src\Castle.Facilities.AspNet.SystemWeb.Tests || exit /b 1
dotnet test src\Castle.Facilities.AspNet.Mvc.Tests || exit /b 1
dotnet test src\Castle.Facilities.AspNet.WebApi.Tests || exit /b 1
dotnet test src\Castle.Facilities.WcfIntegration.Tests || exit /b 1

GOTO nuget_explicit_versions

:nuget_explicit_versions

.\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "castle.windsor"
.\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "castle.loggingfacility"
.\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "castle.windsor.extensions.dependencyinjection"
.\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "castle.facilities.aspnetcore"
.\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "castle.facilities.aspnet.mvc"
.\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "castle.facilities.aspnet.webapi"
.\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "castle.facilities.aspnet.systemweb"
.\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "castle.wcfintegrationfacility"
