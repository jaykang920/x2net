#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
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

    //DotNetCoreRestore("./x2netcore.sln");
    //DotNetCoreBuild("./x2netcore.sln");
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit2("./tests/**/bin/" + configuration + "/net35/*.tests.dll");
    XUnit2("./tests/**/bin/" + configuration + "/net40/*.tests.dll");
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
