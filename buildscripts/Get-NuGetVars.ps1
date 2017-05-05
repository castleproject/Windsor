[xml] $settings = Get-Content 'Settings.proj'
$version_group = $settings.Project.PropertyGroup[1]

$package_major = $version_group.Project_Major
$package_minor = $version_group.Project_Minor
$package_build = $version_group.Project_Build

if ( $env:release_build -eq "true" )
{
    $version_suffix = ""
}
else
{
    $version_suffix = "-ci{0:00000}" -f [int]$env:build_number
}

Write-Host "##teamcity[setParameter name='ReleaseVersion' value='$package_major.$package_minor.$package_build$version_suffix']"

Write-Host "##teamcity[setParameter name='CastleCoreVersion' value='|[3.3.0,4.0)']"

$year = (Get-Date).Year

Write-Host "##teamcity[setParameter name='CurrentYear' value='$year']"
