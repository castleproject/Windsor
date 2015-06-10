# XML configuration properties

If you have parameters that are used by more than one component or you have a complex system configuration, you may use a properties node which allows you to centralize the configuration. For example:

```xml
<configuration>

    <properties>
        <port>10</port>
        <host>smtphost</host>
    </properties>

    <components>
        <component id="smtp.sender"
            service="Namespace.IEmailSender, AssemblyName"
            type="Namespace.SmtpMailSender, AssemblyName">

            <parameters>
                <port>#{port}</port>
                <host>#{host}</host>
            </parameters>

        </component>
    </components>

</configuration>
```

You can specify values to attributes using the same syntax.

Notice the `#{propertyName}` notation. This is non-surprisingly called **property reference notation** and it means *use the value of property with given name*.