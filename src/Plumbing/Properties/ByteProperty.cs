using AsaSavegameToolkit.Plumbing.Primitives;
using System;
using System.Drawing;
using System.Xml.Linq;

namespace AsaSavegameToolkit.Plumbing.Properties;

/// <summary>
/// 8-bit unsigned integer property, can be either numeric or enum.
/// Check tag.Type.Parameters to determine if it's an enum (ByteProperty(EnumType)) or simple byte.
/// </summary>
public class ByteProperty : Property<byte>
{
    public static Property Read(Readers.AsaArchive archive, PropertyTag tag)
    {
        if (archive.IsArkFile)
        {
            if (tag.Size == 1)
            {
                var currentPos = archive.Position;
                archive.Position -= 1;
                var byteType = archive.ReadByte();
                if (byteType == 0)
                {
                    var byteValue = archive.ReadByte();
                    return new ByteProperty
                    {
                        Tag = tag,
                        Value = byteValue
                    };
                }
                else
                {
                    var intEnumValue = archive.ReadInt32();
                    _ = archive.ReadByte();

                    return new ByteEnumProperty
                    {
                        Tag = tag,
                        Value =  new FName(0,0,intEnumValue.ToString())
                    };
                }

            

            }

            archive.Position -= 5;

            var enumType = archive.ReadString();


            var enumSize = archive.ReadInt32();
            var enumPath = archive.ReadString();

            var a1 = archive.ReadInt32();
            var a2 = archive.ReadInt32();
            _ = archive.ReadByte();

            var enumValue = archive.ReadString();

            return new ByteEnumProperty
            {
                Tag = tag,
                Value = new FName(0, 0, enumValue)
            };
        }

        if (tag.Type.Parameters.Count > 0)
        {
            // Enum: read as FName
            return new ByteEnumProperty
            {
                Tag = tag,
                Value = archive.ReadFName()
            };
        }
        else
        {
            return new ByteProperty
            {
                Tag = tag,
                Value = archive.ReadByte()
            };
        }
    }
        
    /// <summary>
    /// Reads just the value without a tag (for array elements, etc.).
    /// Assumes numeric byte since we don't have tag context.
    /// </summary>
    public static byte ReadValue(Readers.AsaArchive archive)
    {
        return archive.ReadByte();
    }
}

public class ByteEnumProperty : Property<FName>
{
    public static ByteEnumProperty Read(Readers.AsaArchive archive, PropertyTag tag)
    {
        return new ByteEnumProperty
        {
            Tag = tag,
            Value = archive.ReadFName()
        };
    }
}
