# Windsor Tutorial - ASP.NET MVC 3 application (To be Seen)

This is introductory tutorial that will get you up to speed with using Windsor in a simple web application. The application is called *To be seen* and helps user collect information about upcoming movies, CDs, books, events etc, and set up reminders for when they come up.

The tutorial assumes no prior familiarity with Windsor, any other container or related concepts. However a sound knowledge of C# and some experience with ASP.NET (hopefully in its MVC incarnation) is required.

You can find the entire code for finished application [on github](https://github.com/kkozmic/ToBeSeen).

## Tutorial

So far the following parts were published:

### Introduction

* [Part One - Getting Windsor](mvc-tutorial-part-1-getting-windsor.md) discusses downloading Windsor assemblies and adding them to your project
* [Part Two - Plugging Windsor In](mvc-tutorial-part-2-plugging-windsor-in.md) discusses creation of custom controller factory
* [Part Three - Writing Your First Installer](mvc-tutorial-part-3-writing-your-first-installer.md) discusses writing Windsor installer
  * [Part Three (a) - Testing Your First Installer](mvc-tutorial-part-3a-testing-your-first-installer.md) discusses testing Windsor installers (validating conventions)
* [Part Four - Putting It All Together](mvc-tutorial-part-4-putting-it-all-together.md) discusses using all elements from previous parts to get a working application

## Building the app

* [Part Five - Adding Logging Support](mvc-tutorial-part-5-adding-logging-support.md) discusses using logging facility to add logging support to the application
* [Part Six - Persistence Layer](mvc-tutorial-part-6-persistence-layer.md) discusses creating custom facility and registering externally created objects, as well as setting up NHibernate
* [Part Seven - Lifestyles](mvc-tutorial-part-7-lifestyles.md) discusses using different lifestyles, per-web-request in particular
* [Part Eight - Satisfying Dependencies](mvc-tutorial-part-8-satisfying-dependencies.md) discusses how dependencies are resolved and what approaches there is to specify dependencies
* [Part Nine - Diagnosing missing dependency issues](mvc-tutorial-part-9-diagnosing-missing-dependency-issues.md) discusses how to approach exceptions throw from the container