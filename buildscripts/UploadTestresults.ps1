
# upload results to AppVeyor
$wc = New-Object 'System.Net.WebClient'

$testBinPath = "bin\Release\net45"
$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)", (Resolve-Path "src\Castle.Windsor.Tests\$testBinPath\TestResult.xml"))
$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)", (Resolve-Path "src\Castle.Facilities.WcfIntegration.Tests\$testBinPath\TestResult.xml"))

