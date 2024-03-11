# Build NuGet package

- Update Castle.Windsor to the desired version.

- Open a terminal.

- Set the APPVEYOR_BUILD_VERSION environment variable to the version you want to publish:

```bash
set APPVEYOR_BUILD_VERSION=X.X.X-beta000X
```

- Launch the build script command:

```bash
build.cmd
```

- The NuGet package is generated in the `build` folder.

- Remove the environment variable, it can cause issues when building in Visual Studio.