# How-To: Get Started with Hosting

**Uno.Extensions.Hosting** can be used to register services that will be accessible throughout the application via dependency injection (DI). This tutorial will walk you through the critical steps needed to leverage hosting in your application.

NOTE: If you used the `dotnet new unoapp-extensions` template to create your solution, the steps outlined here are unnecessary. You can see whether your application already creates the `IHost` interface using the `CreateDefaultBuilder()` method by looking for the generated `App.xaml.host.cs` file.

## Step-by-steps
1. Install the Uno.Extensions.Hosting library from NuGet, making sure you reference the package that is appropriate for the flavor of Uno in use. Likewise, projects with Uno.WinUI require installation of [Microsoft.Extensions.Hosting.WinUI](https://www.nuget.org/packages/Uno.Extensions.Hosting.WinUI) while those with Uno.UI need [Microsoft.Extensions.Hosting.UWP](https://www.nuget.org/packages/Uno.Extensions.Hosting.UWP) instead.

2. Use the `UnoHost` static class to create the `IHost` for the application when the application instance is created:
    * We need to expose the IHost instance to the rest of the application. Add the following property to your App.xaml.cs class:
        ```cs
        public IHost Host { get; private set; }
        ```
    * Invoke the `CreateDefaultBuilder()` method in `UnoHost` followed by `Build()`
        ```cs
        public App()
        {
            Host = UnoHost
                .CreateDefaultBuilder()
                .Build();
        ...
        ```