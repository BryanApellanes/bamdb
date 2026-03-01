# Refactor: Path Correction in `DaoRepoGenerationService`

## Context

The first run of `regenerateAll` corrupted the yaml config files. The service mutated `config.WriteSourceTo` to an absolute path at runtime, then serialized the entire config object back to yaml (to persist the `Project` path). This had three side effects:

1. **WriteSourceTo** changed from `./Generated_Dao` to `C:\src\repos\bamtk\...\Generated_Dao` — breaks if project folder moves
2. **Project** stored as absolute `C:\src\repos\bamtk\...\bam.protocol.data.csproj` — breaks if project folder moves
3. **TypeAssembly** still says `net8.0` but the project targets `net10.0` — not auto-corrected
4. **Default properties leaked** into yaml (`TemplatePath`, `ToNamespace`, `UseAsync`) — noise from serializing the full object

The goal: all three paths (`WriteSourceTo`, `Project`, `TypeAssembly`) should be correct even if the project parent folder is moved on the filesystem.

## Three Approaches

### Approach A: Resolve-Only (never write yaml back)

Treat yaml as read-only. Discover the project and derive TypeAssembly at runtime every time. Never call `ToYamlFile()`.

- **WriteSourceTo**: kept as `./Generated_Dao` in yaml, resolved at runtime relative to config file
- **Project**: not stored in yaml, discovered by walking up from config file every run
- **TypeAssembly**: derived from csproj's `<TargetFramework>` at runtime, yaml value ignored

| Pro | Con |
|-----|-----|
| Simplest, zero yaml corruption risk | Project discovery every run (fast but redundant) |
| No changes to config class needed | Stale TypeAssembly in yaml may confuse users reading it |

### Approach B: Selective Write-Back (persist Project as relative, update TypeAssembly)

Write back only `Project` (as a relative path) and `TypeAssembly` (corrected TFM) using surgical yaml editing — not full object serialization.

- **WriteSourceTo**: never touched in yaml
- **Project**: stored as `../bam.protocol.data.csproj` (relative to config file)
- **TypeAssembly**: TFM segment updated if stale (e.g., `net8.0` → `net10.0`)

| Pro | Con |
|-----|-----|
| Cached project path, portable via relative | Requires surgical yaml text editing or dictionary round-trip |
| TypeAssembly auto-corrected in yaml | More complex write-back logic |

### Approach C: Immutable Config + Runtime Context (chosen)

Separate "what's in the yaml" from "what the runtime needs". A new `DaoRepoGenerationContext` class takes the raw config + config file location, and exposes resolved absolute paths without mutating the config. The yaml is never written back.

- **WriteSourceTo**: kept as-is in yaml, resolved to absolute in context
- **Project**: not stored in yaml, discovered in context constructor
- **TypeAssembly**: derived from csproj in context, overrides whatever yaml says

| Pro | Con |
|-----|-----|
| Clean separation of storage vs runtime | One new class |
| Never corrupts yaml | Project discovery every run |
| Auto-derives TypeAssembly from csproj | |
| No changes to config class or interface | |

## Files Modified

| File | Action |
|------|--------|
| `bamdb/DaoRepoGenerationContext.cs` | **New** — wraps config with resolved paths |
| `bamdb/DaoRepoGenerationService.cs` | **Modify** — use context, remove ResolveWriteSourceTo/ResolveProject |

No changes to `IDaoRepoGenerationConfig`, `DaoRepoGenerationConfig`, or `BamDbMenuContainer`.

## Detailed Changes

### 1. New: `DaoRepoGenerationContext`

**File:** `bamdb/DaoRepoGenerationContext.cs`

Constructed from `(DaoRepoGenerationConfig config, FileInfo configFile)`. Exposes:

- `OriginalConfig` — the raw deserialized config (never mutated)
- `ConfigFile` — the yaml file location
- `ProjectFile` — discovered `.csproj` (`FileInfo?`), found by walking up from config file
- `ResolvedWriteSourceTo` — absolute path: `./` resolved against config dir, `~/` via HomePath, else as-is
- `ResolvedTypeAssembly` — derived from csproj: `~/.bam/build/Debug/{projectName}/{targetFramework}/{projectName}.dll`

**Key method — `ToResolvedConfig()`**: Returns a new `DaoRepoGenerationConfig` with absolute paths set, suitable for passing to `HandlebarsSchemaRepositoryGenerator`. Copies only the properties the generator actually uses (`TypeAssembly`, `SchemaName`, `FromNamespace`, `WriteSourceTo`, `CheckForIds`, `UseAsync`, `UseInheritanceSchema`, `WarningsAsErrors`).

**`DeriveTypeAssemblyPath(FileInfo projectFile)`**: Parses `<TargetFramework>` from csproj XML via `XDocument.Load()`, constructs the home-relative path, then resolves via `HomePath`. Falls back to `net10.0` if element missing.

**`DiscoverProject(FileInfo configFile)`**: Walks up from `configFile.Directory` looking for `*.csproj`. Returns the first found, or null.

### 2. Refactor `DaoRepoGenerationService`

**File:** `bamdb/DaoRepoGenerationService.cs`

**Removed methods:**
- `ResolveWriteSourceTo` — was mutating config.WriteSourceTo in place
- `ResolveProject` — was calling `config.ToYamlFile()` which corrupted yaml

**Refactored `ProcessConfigFile`:**
1. Load config via `DaoRepoGenerationConfig.ReadFrom(configFile)`
2. Create `DaoRepoGenerationContext(config, configFile)`
3. If `context.ProjectFile` is null → skip
4. Build `context.ProjectFile.FullName` (with existing build cache)
5. Validate resolved TypeAssembly DLL exists after build
6. Create resolved config via `context.ToResolvedConfig()`
7. Pass resolved config to `HandlebarsSchemaRepositoryGenerator`
8. Generate + output warnings

The `_builtProjects` HashSet and `BuildProject` method stayed as-is.

### 3. Yaml files restored

The 5 corrupted yaml files in `bam.protocol.data/` were restored via `git checkout` to their original state with relative `WriteSourceTo: ./Generated_Dao` and no leaked properties.

## Edge Cases

- **Multiple `.csproj` in one directory**: Takes the first one. Same as current behavior. Documented limitation.
- **Release vs Debug**: `DeriveTypeAssemblyPath` uses `Debug`. Matches the current yaml convention. Can be parameterized later if needed.
- **No `<TargetFramework>` in csproj**: Falls back to `net10.0`.
- **TypeAssembly DLL doesn't exist after build**: Logs warning with expected path. User can check their build configuration.
- **Yaml has explicit TypeAssembly that differs from derived**: The derived path (from csproj) takes precedence at runtime. The yaml value is preserved but ignored.

## Verification

1. `dotnet build submodules/bamdb/bamdb/bamdb.csproj` — compilation verified (0 warnings, 0 errors)
2. Yaml files restored via `git checkout` in bam.protocol submodule
3. Ran `bamdb /regenerateAll:submodules/bam.protocol` — verified:
   - 5 configs discovered
   - Project auto-discovered for each
   - TypeAssembly derived as `net10.0` (not `net8.0`)
   - Build succeeded, generation succeeded
4. Inspected yaml files after run — confirmed **unchanged** (no absolute paths written, no leaked defaults)
