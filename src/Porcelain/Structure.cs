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
    public long TribeId { get; set; }   
    public string? TribeName { get; set; }
    public bool IsPlayerBuilt { get; set; }
    public Inventory? Inventory { get; set; }
    public FVector? Location { get; private set; }
    public FQuat? Rotation { get; private set; }

    internal static Structure Create(GameObjectRecord r, ActorTransform? transform)
    {
        var properties = r.Properties;

        var structureId = properties.Get<uint>("StructureID");
        var targetingTeam = properties.Get<int>("TargetingTeam");
        bool isPlayerBuilt = TeamInfo.IsTamed(targetingTeam);
       
        var ownerName = properties.Get<string>("OwnerName");
        var owningPlayerId = properties.Get<int>("OwningPlayerID");
        var originalPlacerId = properties.Get<int>("OriginalPlacerID");
        var displayName = properties.Get<string>("BoxName");

        var originalCreationTime = properties.Get<double>("OriginalCreationTime");
        var originalPlacedTimeStamp = properties.Get<string>("OriginalPlacedTimeStamp");
        var lastInAllyRangeTimeSerialized = properties.Get<double>("LastAllyInRangeTimeSerialized");


        var hasFuel = properties.Get<bool>("bHasFuel"); 
        var lastToggleActivated = properties.Get<bool>("bLastToggleActivated");
        var isPinLocked = properties.Get<bool>("bIsPinLocked"); 
        var isLocked = properties.Get<bool>("bIsLocked");
        var isWatered = properties.Get<bool>("bIsWatered");
        var isPowered = properties.Get<bool>("bIsPinPowered"); 
        var isSeeded = properties.Get<bool>("bIsSeeded");
        var isFertilized = properties.Get<bool>("bIsFertilized");
        var hasFruitItems = properties.Get<bool>("bHasFruitItems");

        var containerActivated = properties.Get<bool>("bContainerActivated");
        var lastActivatedTime = properties.Get<double>("LastActivatedTime");
        var lastDeactivatedTime = properties.Get<double>("LastDeactivatedTime");




        return new Structure
        {
            Id = r.Uuid,
            ClassName = r.GetClassName(),
            StructureName = r.Properties.Get<string>("StructureName"),
            TribeName = r.Properties.Get<string>("OwnerName"),
            TribeId = targetingTeam,
            IsPlayerBuilt = isPlayerBuilt,
            Location = transform?.Location,
            Rotation = transform?.Rotation
        };
    }

    public void IngestInventory(Inventory inventory)
    {
        Inventory = inventory;
    }

    public override string ToString()
    {
        var name = StructureName ?? ClassName;
        var tribe = TribeName != null ? $" [{TribeName}]" : "";
        return $"{name}{tribe}";
    }
}
