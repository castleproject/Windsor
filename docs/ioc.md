# Inversion of Control

Inversion of Control is a principle used by frameworks as a way to allow developers to extend the framework or create applications using it. The basic idea is that the framework is aware of the programmer's objects and makes invocations on them.

This is the opposite of using an API, where the developer's code makes the invocations to the API code. Hence, frameworks invert the control: it is not the developer code that is in charge, instead the framework makes the calls based on some stimulus.

You have probably been in situations where you have developed under the light of this principle, even though you were not aware of it.

## Inversion of Control Container

An Inversion of Control Container uses the principle stated above to (in a nutshell) manage classes. That is, their creation, destruction, lifetime, configuration, and dependencies. This way classes do not need to obtain and configure the classes they depend on. This dramatically reduces coupling in a system and, as a consequence, simplifies reuse and testability.

There is some confusion created by people that think that 'Inversion of Control' is a synonym for 'Inversion of Control Container'. As stated, Inversion of control is a broader principle.

Often people think that it is all about "injection", and broadcast that this is the primary purpose of IoC containers. In fact, "injection" is a consequence, a means to decouple, not the primary purpose.

## External resources

* [Blog post by Stefano Mazzocchi (Jan 22, 2004)](http://www.betaversion.org/~stefano/linotype/news/38/)
* [bliki article by Martin Fowler, which totally misses the point](http://www.martinfowler.com/articles/injection.html)