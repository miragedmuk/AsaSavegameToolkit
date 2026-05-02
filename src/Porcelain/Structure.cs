using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;
using System.Security.Principal;

namespace AsaSavegameToolkit.Porcelain;

public class Structure
{
    public Guid Id { get; set; }
    public required string ClassName { get; set; }
    public string? StructureName { get; set; }
    public bool IsActivated { get; set; } = false;
    public double LastActivatedTime { get; set; } = 0;
    public double LastDeactivatedTime { get; set; } = 0;
    public Inventory? Inventory { get; set; }
    public FVector? Location { get;  set; }
    public FQuat? Rotation { get;  set; }

    internal static Structure Create(GameObjectRecord r, ActorTransform? transform)
    {
        var className = r.GetClassName();

        var properties = r.Properties;

        var targetingTeam = properties.Get<int>("TargetingTeam");
        if (TeamInfo.IsTamed(targetingTeam))
        {
            return PlayerStructure.Create(r,transform);
        }
                
        var structureId = properties.Get<uint>("StructureID");
        var containerActivated = properties.Get<bool>("bContainerActivated");
        var lastActivatedTime = properties.Get<double>("LastActivatedTime");
        var lastDeactivatedTime = properties.Get<double>("LastDeactivatedTime");
        var displayName = properties.Get<string>("BoxName") ?? "";

        return new Structure
        {
            Id = r.Uuid,
            ClassName = className,
            StructureName = displayName,
            IsActivated = containerActivated,
            LastActivatedTime = lastActivatedTime,
            LastDeactivatedTime = lastDeactivatedTime,
            Location = transform?.Location,
            Rotation = transform?.Rotation
        };
    }

    public virtual void IngestInventory(Inventory inventory)
    {
        Inventory = inventory;
    }

    public override string ToString()
    {
        var name = StructureName ?? ClassName;
        return $"{name}";
    }
}
