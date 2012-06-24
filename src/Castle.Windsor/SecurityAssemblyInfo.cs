// Sets up assembly level security settings
#if ! SILVERLIGHT
[assembly: System.Security.AllowPartiallyTrustedCallers]
#if !DOTNET35
[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level2)]
#endif
// [assembly: System.Security.SecurityTransparent]
#endif