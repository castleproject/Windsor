# Contributing, Continued

Easy PR's [pull requests](https://help.github.com/articles/about-pull-requests/) on GitHub. 

## Things You Need To Know

There are a few aspects to the build that requires a little setup and further reading. 

 - Visual Studio needs to be run in `admin` mode.
 - Setup your environment using the `build install` utility.

### Visual Studio needs to be run in `admin` mode

Here you have to launch Visual Studio with elevated privelages. If you dont then some tests will fail. Mostly to do with our WcfIntegration facility
(WcfClientFixture, WcfServerFixture). Admin privelages are required for creating a WCF Service Host. We need these tests to pass before accepting PR's 
but are thinking about making this easier going forward. 

### Setup your environment using the `build install` utility

At the moment this supports enabling port sharing on the host OS. It will prompt you for the current `logged in` users password and that user must be 
an administrator. 
