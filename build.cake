//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var outDir = Directory("./bin") + Directory(configuration);
var testDir = Directory("./tests/x2net/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(outDir);
    CleanDirectory(testDir);
	
	// Intermediate
	CleanDirectory(Directory("./src/x2net/obj"));
	CleanDirectory(Directory("./src/xpiler/obj"));
	CleanDirectory(Directory("./tests/x2net/obj"));
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./x2net35.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./x2net35.sln", settings =>
        settings.SetConfiguration(configuration));
      MSBuild("./x2net40.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./x2net35.sln", settings =>
        settings.SetConfiguration(configuration));
      XBuild("./x2net40.sln", settings =>
        settings.SetConfiguration(configuration));
    }

    /*DotNetCoreRestore("./x2netcore.sln");

    var coreFramework = "netcoreapp2.0";
    var standardFramework = "netstandard2.0";

    DotNetCoreBuild("./src/xpiler/x2netcore.xpiler.csproj", new DotNetCoreBuildSettings {
        Framework = coreFramework,
        Configuration = configuration,
        OutputDirectory = outDir + Directory(coreFramework)
    });
    DotNetCoreBuild("./src/x2net/x2netcore.csproj", new DotNetCoreBuildSettings {
        Framework = standardFramework,
        Configuration = configuration,
        OutputDirectory = outDir + Directory(standardFramework)
    });*/
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2("./tests/**/bin/" + configuration + "/net35/*.tests.dll");
    XUnit2("./tests/**/bin/" + configuration + "/net40/*.tests.dll");

    /*DotNetCoreTest("./tests/x2net/x2netcore.tests.csproj", new DotNetCoreTestSettings {
        Configuration = configuration
    });*/
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
