// Sets up assembly level security settings
#if ! CLIENTPROFILE // this could technically be removed if we decide to do something similar in Castle.Core.dll
#if ! SILVERLIGHT
[assembly: System.Security.AllowPartiallyTrustedCallers]
#if !DOTNET35 && FEATURE_SECURITY_PERMISSIONS
[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level2)]
#endif
// [assembly: System.Security.SecurityTransparent]
#endif
#endif