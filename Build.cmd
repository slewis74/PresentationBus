if not exist NuGetOutput md NuGetOutput

Nuget\nuget pack Slew.PresentationBus.nuspec -OutputDirectory NuGetOutput -Version %1