# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

**Prerequisites:** All git submodules must be initialized before building:
```bash
git submodule update --init --recursive
```

```bash
dotnet build                              # Build entire solution
dotnet run --project bamdb                # Run the CLI app
dotnet test                               # Run tests
dotnet publish -c Release -r linux-x64    # Publish (also: win-x64, osx-x64)
```

Build output goes to `~/.bam/build/{Configuration}/bamdb/`.

### Running DAO Generation

Generate schema repository from a YAML config:
```bash
dotnet run --project bamdb -- --gsr --config=./DaoRepoGenerationConfig/<config>.yaml --o=<output-path>
```

VS launch profiles in `bamdb/Properties/launchSettings.json` provide preset regen commands (e.g., `regen-protocol-server`).

## Architecture

**bamdb** is a C# .NET 8.0 console application for DAO code generation. It generates C# data access objects from multiple sources (databases, C# files, assemblies, OpenAPI specs) targeting multiple RDBMS systems (MySQL, PostgreSQL, MS SQL Server, Oracle, Firebird).

### Entry Point Flow

1. `bamdb/Program.cs` (namespace `Bam.Application`) — Registers CLI arguments (`--config`, `--output`/`-o`, `--schemaName`) and delegates to `BamConsoleContext.Current.Main(args)`
2. `BamConsoleContext` (from `bam.console` submodule) — Discovers classes with `[ConsoleMenu]` attribute and routes commands
3. `bamdb/BamDbMenuContainer.cs` (namespace `BamDb`) — Contains all console commands and DI configuration

### DI Configuration

`BamDbMenuContainer.Configure()` registers core services via `ServiceRegistry` (Bam Framework's DI container):
- `IDaoCodeWriter` → `HandlebarsCSharpDaoCodeWriter` (Handlebars templates for C# output)
- `ISchemaProvider` → `SchemaProvider`
- `IDaoGenerator` → `DaoGenerator`
- `IWrapperGenerator` → `HandlebarsWrapperGenerator`
- `IDaoRepository` → `DaoRepository`

### Console Commands

Defined in `BamDbMenuContainer.cs` via `[ConsoleCommand]` and `[MenuItem]` attributes:
- **`initConfig`** — Creates a default `dao-repo-gen.yaml` config file
- **`generateSchemaRepository`** (`--gsr`) — Generates schema repository from a YAML config using `HandlebarsSchemaRepositoryGenerator`
- **`GenerateDataAccessCodeFromAssemblyNamespace`** — Interactive DAO generation from assembly + namespace using `TypeToDaoGenerator`

### YAML Config Format

DAO generation configs live in `bamdb/DaoRepoGenerationConfig/` (6 files). Format:
```yaml
TypeAssembly: ~/.bam/build/Debug/bam.protocol.data/net8.0/bam.protocol.data.dll
SchemaName: ServerSessionSchema
FromNamespace: Bam.Protocol.Data.Server
WriteSourceTo: ../Generated_Dao
CheckForIds: true
UseInheritanceSchema: false
WarningsAsErrors: true
```

### Submodules

The `submodules/` directory contains 12 Bam Framework git submodules referenced as `ProjectReference` entries via relative paths. Key ones: `bam.base`, `bam.console`, `bam.data`, `bam.data.schema`, `bam.data.repositories`, `bam.generators`, `bam.configuration`, `bam.logging`. The `_legacy/` directory is excluded from the build.

## Coding Conventions

- **Namespaces:** `BamDb` for main app classes, `Bam.Application` for Program.cs
- **Id type:** `ulong` for entity Id properties
- **One-to-many:** Parent has `virtual List<Child> Children { get; set; }`, child has `ulong ParentTypeNameId`
- **Many-to-many:** Cross-reference (Xref) tables
- **C# features:** ImplicitUsings enabled, Nullable enabled, .NET 8.0
- **DI pattern:** `ServiceRegistry` fluent API (see `BamDbMenuContainer.Configure()`)
- **NuGet dependency:** YamlDotNet 16.3.0

## Docker Database Containers

Shell scripts in `bamdb/.bam/tools/docker/` start database containers:

| Database | Port | Image |
|---|---|---|
| MS SQL Server | 1433 | `mcr.microsoft.com/mssql/server:2022-latest` |
| MySQL | 3306 | `mysql:8.2` |
| PostgreSQL | 5432 | `postgres` |
| Oracle | 1521 | — |
