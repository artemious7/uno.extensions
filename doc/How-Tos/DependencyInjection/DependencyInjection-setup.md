# How-To: Use Services with Dependency Injection

Dependency injection (DI) is an important design pattern for building loosely-coupled software that allows for maintainability and testing. This tutorial will walk you through how to register services with a service provider for use throughout your application.

> [!Tip] This guide assumes you used the Uno.Extensions `dotnet new unoapp-extensions` template to create the solution. You can verify this by looking for the generated `App.xaml.host.cs` file. Instructions for creating an application from the template can be found [here](../GettingStarted/UsingUnoExtensions.md)

## Step-by-steps

1. Plan the contract for your service
    * Create a new interface which declares the method(s) your service offers:
    ```cs
        public interface IProfilePictureService
        {
	        Task<byte[]> GetAsync(CancellationToken ct);
        }
    ```

2. Add and register a service that encapsulates app functionality
    * Write a new service class which implements the interface you created above, defining its member(s):
    ```cs
        public class ProfilePictureService : IProfilePictureService
        {
            public async Task<byte[]> GetAsync(CancellationToken ct)
            {
                . . .
            }
        }
    ```
    * Register this service implementation with the `IServiceCollection` instance provided by your application's `IHostBuilder`:
    ```cs
        public IHost Host { get; private set; }

        public App()
        {
            Host = UnoHost
                .CreateDefaultBuilder()
                .ConfigureServices(services =>
				{
					// Register your services below
					services.AddSingleton<IProfilePictureService, ProfilePictureService>();
				})
                .Build();
        }
    ```
3. Leverage constructor injection to use an instance of the registered service
    * Create a new view model class that will use the functionality offered by your service. Add a constructor with a parameter of the same type as the service interface you defined earlier:
    ```cs
        public class MainViewModel
        {
            private readonly IProfilePictureService userPhotoService;

            public MainViewModel(IProfilePictureService userPhotoService)
            {
                this.userPhotoService = userPhotoService;
            }
        }
    ```
    * For the dependency injection framework to handle instantiation of the service as a constructor argument, you must also register your view model with the `IServiceCollection`:
    ```cs
        public IHost Host { get; private set; }

        public App()
        {
            Host = UnoHost
                .CreateDefaultBuilder()
                .ConfigureServices(services =>
				{
					// Register your services
					services.AddSingleton<IProfilePictureService, ProfilePictureService>();
                    // Register view model
                    services.AddTransient<MainViewModel>();
				})
                .Build();
        }        
    ```
    * Now, `MainViewModel` has access to the functionality provided by the implementation of your service resolved by `IServiceProvider`:
    ```cs
        byte[] profilePhotoBytes = await userPhotoService.GetAsync(cancellationToken);
    ```
4. Associate the view model with a view from code behind
    * From the code behind of a view, directly reference the application's `IHost` instance to request an instance of the desired view model. Set this as the `DataContext`:
    ```cs
        public MainPage()
        {
            this.InitializeComponent();
            DataContext = (Application.Current as App).Host.Services.GetRequiredService<MainViewModel>();
        }
    ```