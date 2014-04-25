[xml] $settings = Get-Content 'Settings.proj'
$version_group = $settings.Project.PropertyGroup[1]

$package_major = $version_group.Project_Major
$package_minor = $version_group.Project_Minor
$package_build = $version_group.Project_Build

Write-Host "##teamcity[setParameter name='ReleaseVersion' value='$package_major.$package_minor.$package_build']"

$core_version_components = (Get-ChildItem .\lib\NET40\Castle.Core.dll).VersionInfo.ProductVersion.Split('.')

$core_major = $core_version_components[0]
$core_minor = $core_version_components[1]
$core_build = $core_version_components[2]

Write-Host "##teamcity[setParameter name='CastleCoreVersion' value='$core_major.$core_minor.$core_build']"

$year = (Get-Date).Year

Write-Host "##teamcity[setParameter name='CurrentYear' value='$year']"
