# Refactor: AsyncDaoRepository Inheritance & ISchemaRepository Interfaces

## Summary
Refactored `AsyncDaoRepository` from composition (wrapping `DaoRepository`) to inheritance (extending `DaoRepository`), and extracted `ISchemaRepository` / `IAsyncSchemaRepository` interfaces for generic type-parameterized schema operations.

## Changes

### New Files
- `bam.data.repositories/ISchemaRepository.cs` — Generic interface: `SetOneWhere<T>`, `OneWhere<T>`, `Where<T>`, `TopWhere<T>`, `Count<T>`, `CountWhere<T>`, `BatchAll<T>`
- `bam.data.repositories/IAsyncSchemaRepository.cs` — Async versions of `ISchemaRepository` methods
- `bamdb.tests/AsyncDaoRepositoryShould.cs` — Type hierarchy and generated repo tests

### Modified Files
- `bam.data.repositories/DaoRepository.cs` — Implements `ISchemaRepository`; added generic methods using reflection against DAO static methods; made `WarningsAsErrors` null-safe for parameterless constructor usage
- `bam.data.repositories/AsyncDaoRepository.cs` — Now extends `DaoRepository` (was `AsyncRepository`); implements `IAsyncRepository` and `IAsyncSchemaRepository`
- `bam.data.repositories/SchemaRepositoryGenerator.cs` — `Configure()` now checks `UseAsync` config flag to set `BaseRepositoryType` to `AsyncDaoRepository`
- `bam.data/IDaoRepoGenerationConfig.cs` — Added `UseAsync` property
- `bam.generators/DaoRepoGenerationConfig.cs` — Added `UseAsync` property (default `false`)
- 5 YAML config files in `bamdb/DaoRepoGenerationConfig/` — Added `UseAsync: true`, updated `net8.0` to `net10.0`
- 5 generated `*SchemaRepository.cs` files — Base class changed from `DaoRepository` to `AsyncDaoRepository`
- `bamdb.tests/bamdb.tests.csproj` — Added project references for test coverage

### Unchanged
- `ProfileSchemaRepository` — Uses `DaoInheritanceRepository` (not affected)
- All Handlebars templates — No changes needed; `{{BaseRepositoryType}}` handles it
- `AsyncRepository` — Unchanged; still available for non-DAO async repos

## Type Hierarchy
```
Repository (abstract)
  -> DaoRepository : ISchemaRepository, IDaoRepository
       -> AsyncDaoRepository : IAsyncRepository, IAsyncSchemaRepository
            -> CommonSchemaRepository (generated)
            -> ServerSessionSchemaRepository (generated)
            -> ClientSessionSchemaRepository (generated)
            -> PrivateSchemaRepository (generated)
            -> DistributedBlobDataRepository (generated)
       -> DaoInheritanceRepository
            -> ProfileSchemaRepository (generated)
  -> AsyncRepository : IAsyncRepository (unchanged, for non-DAO repos)
```

## Key Design Decisions
- `UseInheritanceSchema` takes precedence over `UseAsync` (only `ProfileSchema` uses inheritance)
- Generic `ISchemaRepository` methods use `IQueryFilter` (not `WhereDelegate<TColumns>`) since columns types are generated and type-specific
- `CreateWhereDelegateForFilter()` bridges `IQueryFilter` to `WhereDelegate<TColumns>` via expression trees for reflection calls to DAO static methods
- `BatchAll<T>` uses Id-based pagination via `Top<T>()` to avoid complex delegate reflection
