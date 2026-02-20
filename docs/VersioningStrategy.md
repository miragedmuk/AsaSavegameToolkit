## Version-Layered Parsing Strategy

### Architecture Principles

1. **Forward Compatibility by Default**
   - Individual property readers should work with newer versions unless specifically broken
   - Only check for "too old" versions in property readers
   - Only check "too new" versions at the outermost entry point

2. **Single Version Boundary Check**
   - `AsaSaveDatabase.Load()` is the **only** place that checks version bounds
   - It validates: `MinSupportedVersion <= SaveVersion <= MaxSupportedVersion`
   - Individual property readers assume the version has already been validated

3. **Clean Delegation Pattern**
   - Each version reader is **complete and independent**
   - Modern reader (`Read()`) handles current version format
   - Legacy readers (`ReadPre14()`, `ReadPre11()`) each read their format from scratch
   - No "patches" or "deltas" between versions

### Implementation Pattern

#### Property Reader Structure

```csharp
public class AsaPropertyHeader
{
    /// <summary>
    /// Read property header for save version 14+ (current format).
    /// Automatically delegates to older readers for legacy formats.
    /// </summary>
    public static AsaPropertyHeader? Read(AsaArchive archive)
    {
        // Early delegation for older versions
        if (archive.SaveVersion < 14)
        {
            return ReadPre14(archive);
        }
        
        // Complete version 14+ implementation
        // Read everything from scratch - this is NOT a patch
        var name = archive.ReadFName();
        if (name == "None")
            return null;
            
        var type = archive.ReadFName();
        var size = archive.ReadInt64();
        var flags = archive.ReadByte();
        
        // Conditional fields based on flags
        int arrayIndex = 0;
        if ((flags & 0x01) != 0) // HasArrayIndex
        {
            arrayIndex = archive.ReadInt32();
        }
        
        Guid? propertyGuid = null;
        if ((flags & 0x02) != 0) // HasPropertyGuid
        {
            propertyGuid = archive.ReadGuid();
        }
        
        PropertyExtensions? extensions = null;
        if ((flags & 0x04) != 0) // HasPropertyExtensions
        {
            extensions = PropertyExtensions.Read(archive);
        }
        
        return new AsaPropertyHeader(name, type, size, arrayIndex, flags, propertyGuid, extensions);
    }
    
    /// <summary>
    /// Read property header for save versions 11-13 (legacy format).
    /// This is a complete independent implementation.
    /// </summary>
    private static AsaPropertyHeader? ReadPre14(AsaArchive archive)
    {
        // Complete version 11-13 implementation
        // Read everything from scratch - independent of Read()
        var name = archive.ReadFName();
        if (name == "None")
            return null;
            
        var type = archive.ReadFName();
        var size = archive.ReadInt64();
        var arrayIndex = archive.ReadInt32(); // Always present in v11-13
        
        // No flags, guid, or extensions in legacy format
        return new AsaPropertyHeader(name, type, size, arrayIndex, flags: 0, null, null);
    }
}
```

#### Version-Specific Property Handling

```csharp
public class AsaBoolProperty
{
    /// <summary>
    /// Read bool property value for save version 14+ (current format).
    /// Value is encoded in the header flags byte (bit 0x10).
    /// </summary>
    public static bool Read(AsaArchive archive, AsaPropertyHeader header)
    {
        // Early delegation for older versions
        if (archive.SaveVersion < 14)
        {
            return ReadPre14(archive, header);
        }
        
        // Version 14+ stores bool value in header flags
        // No data section to read
        return (header.Flags & 0x10) != 0; // BoolValue bit
    }
    
    /// <summary>
    /// Read bool property value for save versions 11-13 (legacy format).
    /// Value is stored as int16 in the data section.
    /// </summary>
    private static bool ReadPre14(AsaArchive archive, AsaPropertyHeader header)
    {
        // Version 11-13 stores bool value as int16 in data section
        return archive.ReadInt16() != 0;
    }
}
```

### Version Evolution Process

When a new save format version is released (e.g., version 15):

#### If Property Format is Unchanged

```csharp
// No code changes needed!
// Read() automatically handles v14-15 because format is identical
public static AsaPropertyHeader? Read(AsaArchive archive)
{
    if (archive.SaveVersion < 14)
    {
        return ReadPre14(archive);
    }
    
    // Works for v14 AND v15
    // ... existing implementation ...
}
```

#### If Property Format Changed

```csharp
// Rename existing Read() → ReadPre15()
// Write new Read() for v15+
public static AsaPropertyHeader? Read(AsaArchive archive)
{
    if (archive.SaveVersion < 15)
    {
        return ReadPre15(archive); // Handles v14 and older
    }
    
    // New complete implementation for v15+
    // ... read v15 format from scratch ...
}

private static AsaPropertyHeader? ReadPre15(AsaArchive archive)
{
    if (archive.SaveVersion < 14)
    {
        return ReadPre14(archive);
    }
    
    // Complete v14 implementation (unchanged from before)
    // ... existing v14 code ...
}

private static AsaPropertyHeader? ReadPre14(AsaArchive archive)
{
    // Complete v11-13 implementation (unchanged)
    // ... existing v11-13 code ...
}
```

### Why No "Too New" Checks?

```csharp
// ❌ BAD: Don't do this in property readers
public static AsaPropertyHeader? Read(AsaArchive archive)
{
    if (archive.SaveVersion < 14 || archive.SaveVersion > MaxSupportedVersion)
    {
        throw new Exception("Version out of range");
    }
    // ... implementation ...
}

// ✅ GOOD: Only check at entry point
public class AsaSaveDatabase
{
    private const int MinSupportedVersion = 11;
    private const int MaxSupportedVersion = 14;
    
    public static AsaSaveDatabase Load(string path)
    {
        var header = ReadHeader(path);
        
        // Single version check for entire load operation
        if (header.SaveVersion < MinSupportedVersion || 
            header.SaveVersion > MaxSupportedVersion)
        {
            throw new UnsupportedVersionException(
                $"Save version {header.SaveVersion} is not supported. " +
                $"Supported versions: {MinSupportedVersion}-{MaxSupportedVersion}");
        }
        
        // All property readers inherit the validated version
        return new AsaSaveDatabase(path, header);
    }
}
```

**Benefits:**
- Property readers automatically work with newer versions
- When v15 releases, only update `MaxSupportedVersion` constant
- Property readers only need updates if format actually changed
- Reduces maintenance burden and risk of version check bugs