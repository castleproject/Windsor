# ASPNET Facility Lifestyles

This is an addendum to the ASPNET facilities to give more detail on what you can expect while
working with scoped and transient lifestyles.

## Scoped Lifestyle

This behaviour is effectively a `Per Web Request` lifestyle. A scope is setup for you once the 
request pipeline for ASPNET is initiated and disposed once it completes. Anything using this
lifestyle should hold on to its state for the duration of the web request. 

You can nest scopes via the `WindsorContainer.BeginScope` method within a given web request which
for any component resolved within that child scope will be given a new instance. Once that child 
scope is disposed, resolved services will honour the original outer scope setup by the facility. 

If you accustomed to using an `instance per matching lifetime scope` from AutoFac, then it is
important note that windsor does not currently support this. We are considering implementing
named scopes but this still requires further discussion when it comes to how the overriding behaviours
of lifestyles would work. 

## Transient Lifestyle

When marking controllers with a transient lifestyle, you also effectively gain a `Per Web Request`
lifestyle. For components resolved outside of controllers one can expect normal transient behaviour,
meaning if they get resolved more then once, you can expect to have a new instance.
