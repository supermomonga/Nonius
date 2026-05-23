# Repository Guidelines

## Project Structure & Module Organization

Nonius is a .NET 8 Avalonia desktop app for viewing Vernier profiling data from Rails applications. The solution lives at `src/Nonius.sln`, and the main app project is `src/Nonius/Nonius.csproj`. Application code is organized by UI role under `src/Nonius/Views`, `ViewModels`, `Models`, `Controls`, and `Converters`. Static app resources are in `src/Nonius/Assets`; documentation images are in `docs`; the Rack sample app is in `sample`; macOS packaging scripts are in `src/bundle-macos.sh` and `src/Info.plist`.

## Build, Test, and Development Commands

- `dotnet restore src/Nonius.sln`: restore NuGet packages and local tool metadata.
- `dotnet build src/Nonius/Nonius.csproj -p:Platform=osx-arm64`: build on macOS using the ARM64 WebView package. Use `-p:Platform=win-x64` for Windows.
- `dotnet run --project src/Nonius/Nonius.csproj -p:Platform=osx-arm64`: run the desktop app locally.
- `dotnet publish src/Nonius/Nonius.csproj -c Release -r osx-arm64 -p:Platform=osx-arm64 -p:UseAppHost=true`: create the macOS publish output.
- `sh src/bundle-macos.sh`: package the publish output into `dist/Nonius.app`.
- `dotnet tool restore && sh src/generate-license-info.sh`: regenerate `src/Nonius/Assets/licenses.json`.

## Coding Style & Naming Conventions

Follow `.editorconfig`: spaces, 4-space indentation for C# and AXAML, 2-space indentation for XML. C# uses file-scoped namespaces, nullable reference types, explicit built-in types instead of `var`, braces for blocks, and `using` directives outside namespaces. Use PascalCase for types, methods, properties, events, and public fields; use camelCase for locals and parameters; use `s_` camelCase for private static fields.

## Testing Guidelines

There is currently no dedicated test project. Validate changes with `dotnet build` for the affected platform and run the app for UI or profiling-flow changes. When adding tests, place them in a sibling project such as `src/Nonius.Tests`, name test files after the class or feature under test, and wire them into `src/Nonius.sln`.

## Commit & Pull Request Guidelines

Recent history uses short imperative or scoped summaries, for example `readme: build from source`, `trivial fixes`, and `fix LICENSE`. Keep commits focused and avoid mixing packaging, UI, and documentation changes. Pull requests should describe the user-visible change, list build/manual verification commands, link related issues, and include screenshots or screen recordings for UI changes.

## Agent-Specific Instructions

Keep user-facing replies for this workspace in Japanese. Do not commit generated `bin` or `obj` outputs; `dist/Nonius.app` should only change when packaging output is intentionally updated.
