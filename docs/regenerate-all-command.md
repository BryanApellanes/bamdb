# bamdb: `regenerateAll` Command

## Summary

Added a `regenerateAll` command that recursively scans for `*.dao-repo-gen.yaml` files, discovers the owning `.csproj` by walking up the directory tree, builds the project, and regenerates DAO code for each config.

## Usage

```bash
# Regenerate all DAO code under a specific directory
bamdb /regenerateAll:C:\src\repos\bamtk\submodules\bam.protocol

# Regenerate everything from current directory
bamdb /regenerateAll

# Short form
bamdb /ra
```

## Files Modified

| File | Change |
|------|--------|
| `bam.data/bam.data/IDaoRepoGenerationConfig.cs` | Added `string? Project { get; set; }` |
| `bam.generators/bam.generators/DaoRepoGenerationConfig.cs` | Added `Project` property |
| `bamdb/bamdb/DaoRepoGenerationService.cs` | New service class with regeneration pipeline |
| `bamdb/bamdb/BamDbMenuContainer.cs` | Added `RegenerateAll()` command method |

## Design

### Project Discovery
Walks up from each yaml file's directory looking for a `.csproj`. Once found, writes the `Project` property back into the yaml so subsequent runs skip discovery. Re-discovers if the stored path doesn't exist.

### Build Optimization
Tracks already-built project paths in a `HashSet` to avoid rebuilding the same `.csproj` multiple times (e.g., when one project has multiple yaml configs).

### WriteSourceTo Resolution
- `./` paths resolved relative to config file directory
- `~/` paths resolved via `HomePath`
- Absolute paths used as-is
