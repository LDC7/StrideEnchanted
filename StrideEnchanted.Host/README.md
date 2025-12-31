# StrideEnchanted.Host

Building and setup the Stride game application like a WebApplication.

## 🚀 Getting Started

```csharp
    var builder = StrideApplication.CreateBuilder<Game>(args);
    using var application = builder.Build();
    await application.RunAsync();
```
