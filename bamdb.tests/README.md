# bamdb.tests

Unit tests for the `bamdb` CLI tool, validating database type enumerations, port constants, and container configuration.

## Overview

bamdb.tests is the test project for the `bamdb` DAO code generation CLI tool. It uses the Bam Framework's menu-driven test runner (`BamConsoleContext.StaticMain`) with the `[UnitTestMenu]` and `[UnitTest]` attributes, rather than xUnit or NUnit. Tests are executed via `dotnet run --project bamdb.tests.csproj -- --ut`.

The test suite currently focuses on verifying the correctness of bamdb's configuration constants: the `DefaultPorts` enum values (MySql 3306, Postgres 5432, MsSql 1433, Oracle 1521) and the `ContainerDatabaseTypes` enum members (MySql, Postgres, MsSql, Oracle). It also includes a `TestDataClasses` directory with a `LeftData` class that appears to be scaffolding for future cross-reference or relationship tests.

The project is intentionally lightweight, with the core generation logic tested more thoroughly in `bam.generators.tests`. bamdb.tests validates the constants and enumerations that are specific to the `bamdb` project itself.

## Key Classes

| Class | Description |
|---|---|
| `BamDbShould` | Test container with selector `bds`. Verifies `DefaultPorts` enum values and `ContainerDatabaseTypes` enum members. |
| `LeftData` | Test data class with `Id`, `Name`, and `Description` properties. Placeholder for future relationship/xref tests. |
| `Program` | Entry point; delegates to `BamConsoleContext.StaticMain(args)`. |

## Tests

| Test Method | Description |
|---|---|
| `HaveExpectedDefaultPorts` | Asserts that `DefaultPorts.MySql == 3306`, `Postgres == 5432`, `MsSql == 1433`, and `Oracle == 1521`. |
| `DefineContainerDatabaseTypes` | Asserts that `ContainerDatabaseTypes` defines `MySql`, `Postgres`, `MsSql`, and `Oracle`. |

## Dependencies

### Project References

- `bam.base`
- `bam.console`
- `bam.test`
- `bamdb`

### Package References

None.

### Target Framework

- `net10.0` (Exe)

## Usage Examples

### Running All Unit Tests

```bash
dotnet run --project bamdb.tests.csproj -- --ut
```

**Important:** Use `--ut` (double dash), not `/ut`. Git Bash on Windows rewrites `/ut` to a file path.

### Running by Selector

```bash
# Run only BamDbShould tests
dotnet run --project bamdb.tests.csproj -- --ut bds
```

## Known Gaps / Not Yet Implemented

- **`TestDataClasses/LeftData.cs`**: This class exists but is not referenced by any test. It appears to be scaffolding for future tests covering cross-reference or relationship generation scenarios.
- No tests currently exist for the core generation commands (`initConfig`, `generateSchemaRepository`, `GenerateDataAccessCodeFromAssemblyNamespace`) exposed by `BamDbMenuContainer`. Those code paths are exercised by `bam.generators.tests` at the library level.
