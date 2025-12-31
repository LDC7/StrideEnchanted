# StrideEnchanted – Repository Context

- **Purpose**: Extensions for the Stride 4.3 game engine: a Generic Host wrapper and a browser-based scene explorer, plus a sample game wiring them together.
- **Solution**: `StrideEnchanted.slnx` with projects `StrideEnchanted.Host`, `StrideEnchanted.Explorer`, meta-package `StrideEnchanted`, and sample under `Example/`.
- **Tooling**: .NET `net10.0`, central package management (`Directory.Packages.props`), `UsePackageReferences` toggle for packing vs local project refs, MIT license.

## StrideEnchanted.Host (StrideEnchanted.Host/*)
- `StrideApplication` / `StrideApplicationBuilder<TGame>` wrap Stride `Game` in `GenericHost`; config from `appsettings*.json` + env vars; DI container seeded with Stride systems (audio, scene, graphics, scripting, profiling, VR) via `AddGameService`.
- Logging bridge: `StrideLoggerProvider` + `StrideLogger` adapt `Microsoft.Extensions.Logging` to Stride’s `GlobalLogger`; supports scopes, maps log levels.
- DI helpers: `ServiceRegistryExtensions` enable resolving/ requiring host services from Stride `IServiceRegistry`.
- Entry usage: `var builder = StrideApplication.CreateBuilder<Game>(args); … using var app = builder.Build(); await app.RunAsync();`.

## StrideEnchanted.Explorer (StrideEnchanted.Explorer/*)
- Purpose: hosted as `IHostedService` (`WebHostAdapter`) to expose a MudBlazor Razor Components UI for inspecting/editing live scenes.
- Registration: `StrideApplicationBuilderExtensions.AddStrideExplorer` spins up Kestrel inside the host; uses existing logger providers; warns when `IHostEnvironment.IsProduction()`.
- Options (`StrideExplorerOptions`): timer interval, IP, port, HTTPS toggle (defaults 127.0.0.1:51891, HTTP).
- Runtime tracking: `DataTrackingTimer` polls; `TrackedScene`/`TrackedEntity` monitor Stride scene graph; `TrackedEntityComponent` reflects public fields/properties (skips `DataMemberIgnore` or `Display(Browsable=false)`); `TrackedEntityComponentParameter` drives editable values with deterministic GUIDs.
- UI: Razor components under `Components/**` using MudBlazor; static web assets shipped via `StrideEnchanted.Explorer.targets`.

## Meta-package (StrideEnchanted/StrideEnchanted.csproj)
- Packs `StrideEnchanted.Host` and `StrideEnchanted.Explorer`; intended for package consumption when `UsePackageReferences=true`.

## Example (Example/*)
- Sample Stride game demonstrating integration. Windows entry (`Example/StrideEnchanted.Example.Windows/Program.cs`) builds host, registers a scoped `TestService`, conditionally `AddStrideExplorer()` in DEBUG, then runs the game.
- Gameplay assets & scripts (`Example/StrideEnchanted.Example/*.cs`) provide camera controller and parameterized scripts to exercise the explorer; Stride asset files under `Example/StrideEnchanted.Example/Assets/`.

## Notes
- No tests present.
- Build: `dotnet build StrideEnchanted.slnx` (requires .NET 10 SDK and Stride packages). Use `UsePackageReferences=true` for NuGet packaging; default uses project references.
