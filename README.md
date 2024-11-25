# ⚠ Nowhere near production ready!
This package is mostly just experimental right now for my own projects. It may or may not ever be completed. Use at your own risk!

# Pure Dependencies
C# source generated dependency injection

## Usage

**Only singletons are supported right now**

### Create a service provider
```cs
using Hertzole.PureDependencies;

[ServiceProvider]
public partial class AppServiceProvider { }
```

### Add a singleton
```cs
using Hertzole.PureDependencies;

[ServiceProvider]
[Singleton(typeof(MyService))]
public partial class AppServiceProvider { }
```

### Create using a factory method
```cs
using Hertzole.PureDependencies;

[ServiceProvider]
[Singleton(typeof(MyService), Factory = nameof(CreateService))]
public partial class AppServiceProvider
{
    private static MyService CreateService()
    {
        return new MyService();
    }
}
```
