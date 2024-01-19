# Destructurama.ByIgnoring

![License](https://img.shields.io/github/license/destructurama/by-ignoring)

[![codecov](https://codecov.io/gh/destructurama/by-ignoring/branch/master/graph/badge.svg?token=0ZRHIUEQM4)](https://codecov.io/gh/destructurama/by-ignoring)
[![Nuget](https://img.shields.io/nuget/dt/Destructurama.ByIgnoring)](https://www.nuget.org/packages/Destructurama.ByIgnoring)
[![Nuget](https://img.shields.io/nuget/v/Destructurama.ByIgnoring)](https://www.nuget.org/packages/Destructurama.ByIgnoring)

[![GitHub Release Date](https://img.shields.io/github/release-date/destructurama/by-ignoring?label=released)](https://github.com/destructurama/by-ignoring/releases)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/destructurama/by-ignoring/latest?label=new+commits)](https://github.com/destructurama/by-ignoring/commits/master)
![Size](https://img.shields.io/github/repo-size/destructurama/by-ignoring)

[![GitHub contributors](https://img.shields.io/github/contributors/destructurama/by-ignoring)](https://github.com/destructurama/by-ignoring/graphs/contributors)
![Activity](https://img.shields.io/github/commit-activity/w/destructurama/by-ignoring)
![Activity](https://img.shields.io/github/commit-activity/m/destructurama/by-ignoring)
![Activity](https://img.shields.io/github/commit-activity/y/destructurama/by-ignoring)

[![Run unit tests](https://github.com/destructurama/by-ignoring/actions/workflows/test.yml/badge.svg)](https://github.com/destructurama/by-ignoring/actions/workflows/test.yml)
[![Publish preview to GitHub registry](https://github.com/destructurama/by-ignoring/actions/workflows/publish-preview.yml/badge.svg)](https://github.com/destructurama/by-ignoring/actions/workflows/publish-preview.yml)
[![Publish release to Nuget registry](https://github.com/destructurama/by-ignoring/actions/workflows/publish-release.yml/badge.svg)](https://github.com/destructurama/by-ignoring/actions/workflows/publish-release.yml)
[![CodeQL analysis](https://github.com/destructurama/by-ignoring/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/destructurama/by-ignoring/actions/workflows/codeql-analysis.yml)

Specify how complex types are logged to Serilog by excluding individual properties.

Install from NuGet:

```powershell
Install-Package Destructurama.ByIgnoring
```

Mark properties to ignore on target types:

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByIgnoringProperties<User>(u => u.Password)
    // Other logger configuration
    .CreateLogger()
```

When these types are destructured, all instance (that is not static) properties except the specified ones will be passed through:

```csharp
Log.Information("Logged on {@User}", new User { Username = "nick", Password = "This is ignored" });

// Prints `Logged on User { Username: "nick"  }`
```

