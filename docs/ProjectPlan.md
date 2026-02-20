# AsaSavegameToolkit Project Plan

## Milestones

- [ ] Milestone 1: Plumbing Layer - Unreal Engine Compliance
  - [x] 1.1 Property System Compliance
  - [ ] 1.2 Object Indexing and References
- [ ] Milestone 2: Porcelain Layer - Domain Models
  - [ ] 2.1 Core Domain Classes
  - [ ] 2.2 Property Interpretation

---

## Architecture Overview

### Two-Layer Design: Plumbing and Porcelain

The toolkit is designed with two distinct layers:

1. **Plumbing Layer** (Low-level serialization)
   - Direct binary parsing from ARK save files
   - Matches Unreal Engine's FArchive serialization exactly
   - Provides raw property bags and object references
   - Version-aware parsing for different save format versions
   - **Purpose**: Enable low-level save file reading, debugging, and forensics

2. **Porcelain Layer** (High-level domain models)
   - User-friendly domain classes (Creature, Player, Tribe, etc.)
   - Property interpretation and semantic understanding
   - Strongly-typed accessors for common properties
   - **Purpose**: Enable game-specific features like tamed dino extraction, map state viewers, inventory reading, etc.

### Layer Separation Benefits

- **Plumbing users** can work directly with raw properties for unknown/new game objects
- **Porcelain users** get a clean API without worrying about binary formats
- **Maintainability**: Version changes stay in plumbing, game updates affect porcelain
- **Testability**: Each layer can be tested independently
---

## Milestone 1: Plumbing Layer - Unreal Engine Compliance

### 1.1 Property System Compliance

**Goal**: Match Unreal Engine 5.2/5.5 property serialization exactly

**Tasks:**
- [x] Update `AsaPropertyHeader` to match `FPropertyTag` structure (now `PropertyTag`)
  - [x] Add flags byte handling (version 14+)
  - [x] Add PropertyGuid support (HasPropertyGuid flag 0x02)
  - [x] Add PropertyExtensions support (HasPropertyExtensions flag 0x04)
  - [x] Implement ArrayIndex conditional read (HasArrayIndex flag 0x01)
  - [x] Implement version-layered Read()/ReadPre14() pattern

- [x] Fix `AsaBoolProperty` (now `BoolProperty`)
  - [x] Version 14+: Read value from flags byte (bit 0x10)
  - [x] Version < 14: Read value from BoolVal int16 in tag
  - [x] Add unit tests for both formats

- [x] Complete `AsaMapProperty` implementation (now `MapProperty`)
  - [x] Match UE5 FMapProperty serialization
  - [x] Handle nested property keys and values
  - [ ] Add validation for key uniqueness

- [x] Complete `AsaSetProperty` implementation (now `SetProperty`)
  - [x] Match UE5 FSetProperty serialization
  - [x] Handle nested property values
  - [ ] Add validation for value uniqueness

- [x] Property Extensions System
  - [x] Implement PropertyExtensions structure
  - [x] Handle OverrideOperation metadata
  - [ ] Support nested struct extensions

**Verification:**
- Unit tests comparing against UE 5.2 source reference files
- Compatibility tests with ark-sa-save-tools Java implementation

### 1.2 Object Indexing and References

**Goal**: Support object references and array indexing properly

**Tasks:**
- [x] Implement sparse array support using ArrayIndex
  - [ ] Example: ColorRegions[2], [4], [5] (not sequential) - not yet verified end-to-end
  - [ ] Handle missing indices gracefully

- [x] Object reference resolution (`ObjectReference` class: GUID or path)
  - [ ] Map integer indices to object UUIDs
  - [ ] Support forward references (reference before definition)
  - [ ] Handle null/invalid references

- [ ] Cross-database references
  - [ ] Support references between .arkprofile, .arktribe files
  - [ ] Lazy loading of referenced objects

**Verification:**
- Load complex save with many object references
- Verify ColorRegions array parsing
- Test creature tame/tribe owner references

---

## Milestone 2: Porcelain Layer - Domain Models

### 2.1 Core Domain Classes

**Goal**: Provide clean, type-safe APIs for common game objects

**Classes to Implement:**
- [ ] `Creature` - Tamed dinosaurs and creatures
  - Properties: Name, Level, Stats, ColorRegions, Imprint, Mutations
  - Methods: CalculateStatValue(), GetExportText()
  
- [ ] `Player` - Player characters
  - Properties: Name, Level, Tribe, Inventory, Engrams, Ascensions
  
- [ ] `Tribe` - Player tribes
  - Properties: Name, Members, TameCount, Structures, Logs
  - Methods: GetPermissions()
  
- [ ] `Inventory` - Item containers
  - Properties: Items, MaxSlots, Folder structure

- [ ] `Item` - Individual items
  - Properties: Blueprint, Quantity, Durability, Crafting data

### 2.2 Property Interpretation

**Goal**: Abstract away raw property bags

**Tasks:**
- [x] GameObjectExtensions for common property patterns
  - `GetClassName()`, `GetDisplayName()`, typed getters for all property types
  
- [ ] Stat calculation with multipliers
  - Base, wild, tamed, imprint, mutation multipliers
  - Server settings integration
  
- [ ] ColorRegion mapping
  - Region index → color name lookup

---

## Testing Strategy

### Unit Tests
- [x] Property serialization (each property type, each version)
- [x] Version migration tests (v13 vs v14 format)

### Integration Tests
- [x] Load real ARK save files from different versions
- [ ] Verify object graph resolution
- [ ] Test cross-file references (profile → tribe)

### Test Data
- [x] Store test saves in `tests/assets/` by version (gitignored per-object fragments in `.work/output/`)
- [x] Maintain minimal test saves for specific scenarios
- [ ] Reference files from UE 5.2 for format validation

---

## Version Support Matrix

| Save Version | Format Changes                           | Status                              |
| ------------ | ---------------------------------------- | ----------------------------------- |
| 11-13        | Legacy format: explicit array index      | [x] Current                         |
| 14           | Added flags byte, BoolProperty in header | [x] Current                         |
| 15+          | Future versions                          | Auto-compatible until format change |

---

## Documentation Roadmap

- [x] Architecture overview (this document)
- [x] Version-layered parsing strategy (this document)
- [ ] API reference (auto-generated from XML docs)
- [ ] Cookbook (common recipes for reading saves)
- [ ] Property type reference (complete property catalog)

---

## Contributing

When adding new features:

1. **Plumbing changes**: Update this plan, implement with version layers, add unit tests
2. **Porcelain changes**: Design API first, get feedback, then implement
3. **Update docs**: Keep this plan in sync with code
4. **Test with real saves**: Verify with actual ARK save files before merging

