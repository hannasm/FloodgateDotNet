* Update Nuget Package Link in README.md
* Update source tag link in README.md
* Update Version Number in Floodgate.VersionNumber.md
* update release notes in Floodgate.ReleaseNotes.md
* create tag for release on github with name as version Number
* 'dotnet pack -c Release' to generate the nuget PublishNugetPackages
* 'dotnet nuget push <package.nupkg> -s  https://www.nuget.org/api/v2/package -k <api key here>' to push to nuget.org 
