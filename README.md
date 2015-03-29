# Destructurama.ByIgnoring

Specify how complex types are logged to Serilog by excluding individual properties.

Install from NuGet:

```powershell
Install-Package Destructurama.ByIgnoring
```

Mark properties to ignore on target types:

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByIgnoringProperties<User>(u => u.Password)
    // Other logger configurationg
    .CreateLogger()
```

When these types are destructured, all properties except the specified ones will be passed through:

```csharp
Log.Information("Logged on {@User}", new User { Username = "nick", Password = "This is ignored" });

// Prints `Logged on User { Username: "nick"  }`
```

