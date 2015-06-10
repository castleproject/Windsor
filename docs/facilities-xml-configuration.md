# Facilities XML Configuration

Facilities are defined by tag `<facility />` within `<facilities />` section.

## Default facility configuration

By default only two attributes must be defined to register facility via XML:

```xml
<facilities>
  <facility
    id="event.wiring"
    type="Castle.Facilities.EventWiring.EventWiringFacility, Castle.Windsor" />
</facilities>
```

The `id` attribute is the unique identified for the facility, while `type` is the type implementing it.

:warning: **Facilities must have public default constructor:** When facilities are instantiated by Windsor, they must have public default constructor. Otherwise an exception will be thrown.

:information_source: **Additional configuration:** Facilities often provide additional configuration over the default shown here. They often also extend the configuration of components. For details see the documentation of specific facilities.