using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;

namespace AsaSavegameToolkit.Porcelain;

public class Structure
{
    public Guid Id { get; set; }
    public required string ClassName { get; set; }
    public string? StructureName { get; set; }
    public long TribeId { get; set; }   
    public string? TribeName { get; set; }
    public bool IsPlayerBuilt { get; set; }
    public FVector? Location { get; private set; }
    public FQuat? Rotation { get; private set; }
    public GameObjectRecord? Record { get; set; }

    internal static Structure Create(GameObjectRecord r, ActorTransform? transform)
    {
        return new Structure
        {
            Id = r.Uuid,
            ClassName = r.GetClassName(),
            StructureName = r.Properties.Get<string>("StructureName"),
            TribeName = r.Properties.Get<string>("OwnerName"),
            TribeId = r.Properties.Get<int>("TargetingTeam"),
            IsPlayerBuilt = (r.Properties.Get<bool>("bHasResetDecayTime") || GameObjectExtensions.NonCreatureDisambiguatedClasses.Contains(r.GetClassName())) && r.Properties.Get<int>("TargetingTeam") !=0,
            Location = transform?.Location,
            Rotation = transform?.Rotation,
            Record = r
        };
    }

    public override string ToString()
    {
        var name = StructureName ?? ClassName;
        var tribe = TribeName != null ? $" [{TribeName}]" : "";
        return $"{name}{tribe}";
    }
}
