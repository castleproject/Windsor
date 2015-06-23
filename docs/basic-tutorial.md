# Basic Windsor Tutorial

The intent of this tutorial is to provide the most basic exposure to Windsor and minimal steps to use. For deeper coverage, see additional tutorials hosted on this site. Aside from Visual Studio, The only external tools necessary to complete this tutorial is the Castle Windsor component. This tutorial uses Castle Windsor version 2.5.3, and .NET Framework 3.5.

It is assumed you have familiarity with Inversion of Control concepts but no prior familiarity with Windsor. Also, it is assumed you have an understanding of C# concepts, such as interfaces.

## Getting Started

Download the Windsor components. Directions how to get Windsor are located [here](mvc-tutorial-part-1-getting-windsor.md), but these instructions are for a different, more detailed tutorial, and do not refer back to this tutorial.

The premise of this tutorial is based on two projects - a windows form project, and a class library, both in the same solution. To follow the tutorial it is probably easiest to use the same names. I have called the new solution "CastleWindsorExample", the windows forms project is also called "CastleWindsorExample", and the class library is called "ClassLibrary1".

## Create Class Library

In the project ClassLibrary1, add two interfaces - IDependency1 and IDependency2 (fictional names I made up - these names could be anything)

The interface IDependency1 has the following definition...

```csharp
namespace ClassLibrary1
{
    public interface IDependency1
    {
        object SomeObject { get; set; }
    }
}
```

The interface IDependency2 has the following definition...

```csharp
namespace ClassLibrary1
{
    public interface IDependency2
    {
        object SomeOtherObject { get; set; }
    }
}
```

Now, create two classes that will be the actual implementation of these classes. My example uses classes Dependency1 and Dependency2.

The definition of dependency1 is ...

```csharp
namespace ClassLibrary1
{
    public class Dependency1 : IDependency1
    {
        public object SomeObject { get; set; }
    }
}
```

The definition for dependency2 is ...

```csharp
namespace ClassLibrary1
{
    public class Dependency2 : IDependency2
    {
        public object SomeOtherObject { get; set; }
    }
}
```

Notice they inherit from the interfaces.

Create a new class called Main. This is the entry point class to the library.

The definition for main is:

```csharp
namespace ClassLibrary1
{
    public class Main
    {
        private IDependency1 object1;
        private IDependency2 object2;

        public Main(IDependency1 dependency1, IDependency2 dependency2)
        {
           object1 = dependency1;
           object2 = dependency2;
        }

        public void DoSomething()
        {
           object1.SomeObject = "Hello World";
           object2.SomeOtherObject = "Hello Mars";
        }
    }
}
```

Notice the constructor requires two parameters, the same as the interfaces. Here we are injecting the dependencies rather than creating an instance of each. We cannot create an object from the main class unless we also provide the two dependencies.

## Use Class Library

Up to this point, we have 3 classes, and two interfaces. Because this example is so simple, there is not much functionality in this program. Our class library ClassLibrary1 exposes 3 object types, and has nothing to do with Windsor - yet. We now shift our attention to the Windows Forms project 'CastleWindsorExample'.

In the project CastleWindsorExample add three references, a reference to the ClassLibrary1 project, to Castle.Core, and Castle.Windsor (Downloaded Castle Windsor components).

My default windows forms project started me with a file Form1.cs. I will add a button to this form called button1. In the button1_Click event we will wire up the class library. This is the heart of Castle Windsor. Everything up to this point is preparation.

The file Form1.cs file contains...

```csharp
using Castle.MicroKernel.Registration;
using Castle.Windsor;

private void button1_Click(object sender, EventArgs e)
{
    // CREATE A WINDSOR CONTAINER OBJECT AND REGISTER THE INTERFACES, AND THEIR CONCRETE IMPLEMENTATIONS.
    var container = new WindsorContainer();
    container.Register(Component.For<Main>());
    container.Register(Component.For<IDependency1>().ImplementedBy<Dependency1>());
    container.Register(Component.For<IDependency2>().ImplementedBy<Dependency2>());

    // CREATE THE MAIN OBJECT AND INVOKE ITS METHOD(S) AS DESIRED.
    var mainThing = container.Resolve<Main>();
    mainThing.DoSomething();
}
```

If you set a breakpoint in the constructor of main, you will see the dependencies are defined. The code we implemented never created instances of dependency1 or dependency2. Windsor did this for us. The AddComponent methods perform the wireup for the application.

## Conclusion

Why do we want this? The main class is completely independent and can be unit tested easily. We have achieved seperation of concerns on each dependency. Each dependent class can be individually unit tested. This tutorial shows both the concept of Inversion of Control, as well as a very basic use of Windsor. How does this tutorial compare to real-world use? Replace dependency1 with a File-Getter and replace dependency2 with a File-Parser. Both the File-Getter and the File-Parser would need to be injected into the main class, and yet all are autonomous and unit testable.