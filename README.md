# bamdb

CLI tool for generating Data Access Object (DAO) code from POCO types, with support for multiple database backends and container-based database management.

## Overview

bamdb is the command-line entry point for the Bam Framework's DAO code generation pipeline. It wraps `bam.generators` in a console application that reads `DaoRepoGenerationConfig` YAML files and invokes `HandlebarsSchemaRepositoryGenerator` to produce strongly-typed DAO source code. The tool is packaged as a NuGet tool package (v2.0.0) and can be invoked from the command line or used interactively through the Bam Framework's menu-driven console system.

The primary workflow is: point bamdb at a YAML configuration file that specifies a source assembly, a source namespace containing POCO types, a target namespace for generated DAO types, and an output directory. bamdb then loads the assembly, discovers all types in the specified namespace, builds an in-memory schema (including foreign keys and cross-references), and renders a complete set of C# source files using Handlebars templates. The generated code includes DAO classes, collections, query classes, column metadata, wrapper classes, and a schema repository with typed `OneWhere`/`GetOneWhere` methods.

bamdb also defines constants for container-based database deployments, including `ContainerDatabaseTypes` (MySql, Postgres, MsSql, Oracle), `DefaultPorts` (3306, 5432, 1433, 1521), and `DefaultImageTags` for Docker images. The project includes a `_legacy` directory (excluded from compilation) containing earlier shell-based code generation providers and a `BamDbResponder`/`BamDbServer` for an older HTTP-based API.

## Key Classes

| Class | Description |
|---|---|
| `BamDbMenuContainer` | Main console menu container. Provides `InitConfig`, `GenerateSchemaRepository`, and `GenerateDataAccessCodeFromAssemblyNamespace` commands. Configures the service registry with Handlebars-based code writer, schema provider, and wrapper generator. |
| `Program` | Entry point. Registers `--config` and `--output` CLI arguments, then delegates to `BamConsoleContext.Current.Main(args)`. |
| `ContainerDatabaseTypes` | Enum defining supported containerized database engines: `MySql`, `Postgres`, `MsSql`, `Oracle`. |
| `DefaultPorts` | Enum defining default ports: MySql (3306), Postgres (5432), MsSql (1433), Oracle (1521). |
| `DefaultImageTags` | Constants for default Docker image tags: `mcr.microsoft.com/mssql/server:2022-latest`, `mysql:8.2`, `postgres`. |

## Commands

| Command | Description |
|---|---|
| `initConfig` | Writes a default `DaoRepoGenerationConfig.yaml` to the current directory. |
| `generateSchemaRepository` | Reads a `DaoRepoGenerationConfig` (from `--config` path or default location) and generates DAO source. Supports `--output` / `-o` to override the output directory. |
| `GenerateDataAccessCodeFromAssemblyNamespace` | Interactive prompt-driven generation: asks for assembly path, namespace, schema name, and output path, then generates DAO source from matching types. |

## Relationship Conventions

### Child Collections (One to Many)

Define the enumerable property on the parent as `virtual` and specify a `ulong` property on the child whose name is in the form `<ParentTypeName>Id`:

```csharp
public class Parent
{
    public virtual List<Child> Children { get; set; }
}
public class Child
{
    public ulong ParentId { get; set; }
}
```

### Cross-Reference Collections (Many to Many)

Add a `virtual List<OtherType>` property to each of the related types:

```csharp
public class Student
{
    public virtual List<Course> Courses { get; set; }
}
public class Course
{
    public virtual List<Student> Students { get; set; }
}
```

## Dependencies

### Project References

- `bam.base`
- `bam.configuration`
- `bam.console`
- `bam.data.dynamic`
- `bam.data.objects`
- `bam.data.repositories`
- `bam.data.schema`
- `bam.data.config`
- `bam.data.firebird`
- `bam.data.mssql`
- `bam.data.mysql`
- `bam.data.oracle`
- `bam.data.postgres`
- `bam.data`
- `bam.encryption`
- `bam.generators`
- `bam.logging`
- `bam.presentation`
- `bam.protocol`
- `bam.server`
- `bam.shell`

### Package References

- `YamlDotNet` 16.3.0

### Target Framework

- `net10.0` (Exe, packaged as NuGet tool)

## Usage Examples

### Initialize a Configuration File

```bash
dotnet run --project bamdb.csproj -- /initConfig
```

This writes a default `DaoRepoGenerationConfig.yaml` to the current directory.

### Generate DAO Source from Config

```bash
dotnet run --project bamdb.csproj -- /generateSchemaRepository --config ./my-config.yaml --output ./Generated_Dao
```

### Example DaoRepoGenerationConfig YAML

```yaml
TypeAssembly: ~/.bam/build/Debug/my.project/net10.0/my.project.dll
SchemaName: MySchema
FromNamespace: MyApp.Data.Models
ToNamespace: MyApp.Data.Models.Dao
WriteSourceTo: ./Generated_Dao
CheckForIds: true
UseInheritanceSchema: false
WarningsAsErrors: true
```

### Bundled Generation Configs

The project ships with several pre-built generation configs in `DaoRepoGenerationConfig/`:

| Config File | Schema | Source Namespace |
|---|---|---|
| `bam.protocol.data.server.dao-repo-gen.yaml` | ServerSessionSchema | `Bam.Protocol.Data.Server` |
| `bam.protocol.data.client.dao-repo-gen.yaml` | ClientSchema | `Bam.Protocol.Data.Client` |
| `bam.protocol.data.common.dao-repo-gen.yaml` | CommonSchema | `Bam.Protocol.Data.Common` |
| `bam.protocol.data.private.dao-repo-gen.yaml` | PrivateSchema | `Bam.Protocol.Data.Private` |
| `bam.protocol.data.profile.dao-repo-gen.yaml` | ProfileSchema | `Bam.Protocol.Data.Profile` |
| `bam.blobs.distributed.dao-repo-gen.yaml` | BlobsDistributed | `Bam.Blobs.Distributed` |

### Generate from Existing Database

```bash
bamdb code generate --from:"[database connection string]" --out:./Dao --dbType:mssql
```

Supported database types: `mssql`, `mysql`, `oracle`, `postgres`.

### Generate from Assembly

```bash
bamdb code generate --from:./assembly.dll --out:./Dao --fromNamespace:MyApp.Models
```

### Generate a Compiled Assembly

```bash
bamdb generate --from:./srcDir --out:./path/to/my-assembly.dll
```

## Legacy Code (`_legacy/`)

The `_legacy/` directory is excluded from compilation (`<Compile Remove="_legacy\**" />`). It contains earlier implementations including:

- `BamDbResponder` / `BamDbServer` -- HTTP-based API for remote DAO generation.
- `Shell/CodeGen/` -- Older shell providers for DAO, model, schema extraction, and GraphQL generation.
- `Shell/Data/` -- Data shell providers for interactive data entry and model management.

## Known Gaps / Not Yet Implemented

The following items exist only in the excluded `_legacy/` directory and are **not** compiled:

- **`Shell/Data/EntryProvider.cs`**: Methods `New()`, `Get()`, `Set()`, `Del()`, and `Find()` all throw `NotImplementedException`.
- **`Shell/Data/ModelProvider.cs`**: Methods `New()`, `Get()`, `Set()`, and `Del()` all throw `NotImplementedException`.
- **`BamDbResponder.cs`**: Two methods throw `NotImplementedException`.
- **`Shell/CodeGen/GraphQLProvider.cs`**: Contains a `TODO` comment: "encapsulate GraphQLGenerationConfig provider logic; IGraphQLGenerationConfigProvider".

None of these affect the active (compiled) codebase.
