# Performance Counters

Windsor 3 introduces support for Windows performance counters.

Currently Windsor publishes just one counter - "Objects tracked by release policy", which shows you the total number of objects tracked by release policy of given container.

:information_source: **Hunting memory leaks:** This is a very useful feature, that will help you quickly validate if you have problems with non-releasing tracked component instances.

## Using Counters

This feature is not enabled by default. The following code shows how you can enable it:

```csharp
var container = new WindsorContainer();
var diagnostic = LifecycledComponentsReleasePolicy.GetTrackedComponentsDiagnostic(container.Kernel);
var counter = LifecycledComponentsReleasePolicy.GetTrackedComponentsPerformanceCounter(new PerformanceMetricsFactory());
container.Kernel.ReleasePolicy = new LifecycledComponentsReleasePolicy(diagnostic, counter);
```

Then Windsor will inspect if it has all required permissions, and if it does, it will ensure the right category and counters are created and will update the counter as the application(s) run.

In order to see the data open Performance Monitor (part of Computer Management console accessible from Administrative Tools section of your Windows Control Panel). Then click Add (Ctrl+N) and find "Castle Windsor" section. As noted above it will contain just one counter - "Objects tracked by release policy", and list of its instances.

List of instances of Windsor performance counter:

![](images/perf-counter-setup.png)

For example on the image above you can see there are two instances of the counter. Each of them comes from separate instance of the same application. After you select them you will be able to track, live, total number of all tracked component instances in each of the containers.

List of tracked instances in each container:

![](images/perf-counter-instances.png)