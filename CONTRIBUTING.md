# Contributing

Information on contributing to this project can be found in the [Castle Project Contributing Guide](https://github.com/castleproject/Home/blob/master/README.md) located in our Home repository.

## Criteria for Patches

Before committing any change make sure all the following items are met:

1. The change makes sense and brings value
1. The code complies with [Castle coding standards](https://github.com/castleproject/Home/blob/master/coding-standards.md)
1. The code compiles in all versions (that currently means .NET 3.5 SP1, .NET 4 (full and client profile), Silverlight 3 and 4, possibly Mono in the (nearish) future)
1. The change has tests, with reasonably good code coverage
1. All tests pass in all versions we build
1. If needed (changes something more significant than fixing a typo or formatting update) `Changes.txt` is updated
1. If the change is breaking, `breakingchanges.txt` is updated
1. The change works with child containers, `NoTrackingReleasePolicy` or other non-standard configuration that may affect it
1. Documentation is accordingly updated
1. Related issues in issue tracker are accordingly updated

## Easy PR's

Easy PR's [pull requests](https://help.github.com/articles/about-pull-requests/) on GitHub. 

## Things You Need To Know

There are a few aspects to the build that requires a little setup and further reading. 

 - Visual Studio needs to be run in `admin` mode.
 - Running `build.cmd` in cmd.exe also requires `admin` mode.
 - Setup your environment using the `build install` utility.

### Visual Studio needs to be run in `admin` mode

Here you have to launch Visual Studio with elevated privelages. If you dont then some tests will fail. Mostly to do with our WcfIntegration facility
(WcfClientFixture, WcfServerFixture). Admin privelages are required for creating a WCF Service Host. We need these tests to pass before accepting PR's 
but are thinking about making this easier going forward. 

###	Running `build.cmd` in cmd.exe also requires `admin` mode.

This is similar to the requirements above for Visual Studio but only from the command line.

### Setup your environment using the `build install` utility

At the moment this supports enabling port sharing on the host OS. It will prompt you for the current `logged in` users password and that user must be 
an administrator. 

