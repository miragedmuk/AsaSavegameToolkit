using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Readers;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AsaSavegameToolkit.Porcelain;

public class AsaSaveGame
{
    public int SaveVersion { get; set; }

    public required IDictionary<Guid, Player> Players { get; set; }
    public required IDictionary<Guid, Tribe> Tribes { get; set; }
    public required IDictionary<Guid, Creature> WildCreatures { get; set; }
    public required IDictionary<Guid, Creature> TamedCreatures { get; set; }
    public required IDictionary<Guid, Structure> Structures { get; set; }
    public required IDictionary<Guid, Item> DroppedItems { get; set; }

    public static AsaSaveGame ReadFrom(string path, ILogger? logger = null, AsaReaderSettings? settings = null, CancellationToken cancellationToken = default)
    {
        logger ??= NullLogger.Instance;

        using var reader = new AsaSaveReader(path, logger, settings ?? AsaReaderSettings.None);

        var header = reader.ReadSaveHeader(cancellationToken);
        var gameObjects = reader.ReadGameRecords(cancellationToken);
        var transforms = reader.ReadActorTransforms(cancellationToken).Transforms;

        var creatureRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();
        var statusRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();
        var inventoryRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();

        var droppedItemRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();     
        var inventoryItemRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();

        var tribeRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();
        var profileRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();
        var playerComponentRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();
        var structureRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();
        var deathCacheRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();
        var ignoredRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();
        var unknownRecords = new ConcurrentDictionary<Guid, GameObjectRecord>();


        // Place all top-level objects (those with only 1 name) into the recordsByName dictionary so they
        // can be referenced as parents by their components in the second pass.
        Dictionary<string, GameObjectRecord> recordsByName = gameObjects.Values
            .Where(x => x.Names.Count == 1)
            .DistinctBy(x => x.Names[0])
            .ToDictionary(x => x.Names[0]);

        // Second pass: for each game object with multiple names, find its parent object in recordsByName using the
        // last name in the Names list, then crawl down the component stack using the preceding names until we find the
        // correct parent to attach this component to.

        foreach (var gameObject in gameObjects.Values.Where(x => x.Names.Count > 1))
        {
            var parentName = gameObject.Names[^1];
            if (!recordsByName.TryGetValue(parentName, out var parent))
            {
                // only warn about missing parents if the parent name isn't rooted.
                // We don't expect to find `/Game/PrimalEarth/Sound/PlayerVoice/Female_Legacy/DA_Female_Legacy` in the records.
                if (!parentName.StartsWith('/'))
                {
                    logger.LogWarning(
                        "Could not find parent game object with name {ParentName} for game object with name {ChildName}",
                        gameObject.Names[1],
                        gameObject.Names[0]);
                }

                continue;
            }

            // The component names are in deepest-first order, so crawl from the back of the list forward down the component stack
            // This will probably never be be invoked as there are no known objects with more than 2 names
            for (var i = gameObject.Names.Count - 2; i > 0; i--)
            {
                parent = parent.Components[gameObject.Names[i]];
            }

            parent.Components[gameObject.Names[0]] = gameObject;
        }
        recordsByName = null!;


        // Third Pass: now that we've nested all the components, we can categorize the top level game objects by type
        Parallel.ForEach(gameObjects.Values, gameObject =>
        //foreach (var gameObject in gameObjects.Values)
        {
            if (gameObject.IsCreature())
                creatureRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsTribe())
                tribeRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsProfile())
                profileRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsPlayerComponent())
                playerComponentRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsStatusComponent())
                statusRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsStructure())
                structureRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsInventory())
                inventoryRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsInventoryItem())
                inventoryItemRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsDeathItemCache())
                deathCacheRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.IsDroppedItem() || (gameObject.ObjectType == ObjectTypeFlags.Item && transforms.ContainsKey(gameObject.Uuid)))
                droppedItemRecords[gameObject.Uuid] = gameObject;
            else if (gameObject.ClassNameContains("NPCZone") || gameObject.ClassNameContains("NPCCount"))
                ignoredRecords[gameObject.Uuid] = gameObject;
            else
                unknownRecords[gameObject.Uuid] = gameObject;
        });


        // The full record dictionaries are no longer needed now that every object has been
        // placed into a typed bucket. Release them so the GC can collect the raw
        // GameObjectRecord graph before we begin allocating the Porcelain objects.
        gameObjects = null!;

        var structures = structureRecords.ToDictionary(
            r => r.Key,
            r => Structure.Create(r.Value, transforms.TryGetValue(r.Key, out var t) ? t : null));

        var players = profileRecords.ToDictionary(
            r => r.Key,
            r =>
            {
                ulong playerDataId = 0;

                var myData = r.Value.Properties.Get<StructProperty>("MyData");
                if (myData != null)
                {
                    List<Property> properties = (List<Property>)myData.Value;
                    playerDataId = properties.Get<ulong>("PlayerDataID");

                }
                List<GameObjectRecord> playerComponents = playerComponentRecords.Values.Where(v => v.Properties.Get<ulong>("LinkedPlayerDataID") == playerDataId).ToList();
                
                var characterRecord = playerComponents.FirstOrDefault(c => !c.IsStatusComponent());
                GameObjectRecord? statusRecord = null;

                ActorTransform? actorLocation = null;
                if (characterRecord != null)
                {
                    actorLocation = transforms.TryGetValue(characterRecord.Uuid, out var t) ? t : null;

                    var statusRefProperty = (ObjectReference)characterRecord.Properties.Get<ObjectProperty>("MyCharacterStatusComponent")?.Value;

                    var statusRef = statusRefProperty?.ObjectId;
                    if (statusRef != null)
                        statusRecord = statusRecords[statusRef.Value];
                }

                var player = Player.Create(r.Value, actorLocation);

                if (characterRecord != null)
                    player.IngestCharacterRecord(characterRecord);

                if (statusRecord != null)
                    player.IngestStatusRecord(statusRecord);

                return player;
            });


        var tribes = tribeRecords.ToDictionary(
        r => r.Key,
        r =>
        {
            return Tribe.Create(r.Value);
        });

        var creatures = creatureRecords.ToDictionary(
            r => r.Key,
            r => {
                var creature = Creature.Create(r.Value, transforms.TryGetValue(r.Key, out var t) ? t : null);
                var statusComponentRef = (ObjectReference?)r.Value.Properties.Get<ObjectProperty>("MyCharacterStatusComponent")?.Value;
                if (statusComponentRef != null)
                {
                    if (statusRecords.ContainsKey(statusComponentRef.ObjectId))
                    {
                        var statusComponent = statusRecords[statusComponentRef.ObjectId];
                        creature.IngestStatusRecord(statusComponent);
                    }
                }

                var inventoryComponentRef = (ObjectReference?)r.Value.Properties.Get<ObjectProperty>("MyInventoryComponent")?.Value;
                if (inventoryComponentRef != null)
                {


                }

                return creature;
            });


        var droppedItems = droppedItemRecords.ToDictionary(
            r => r.Key,
            r => Item.Create(r.Value, transforms.TryGetValue(r.Key, out var t) ? t : null));

        var inventoryItems = inventoryItemRecords.ToDictionary(
            r => r.Key,
            r => Item.Create(r.Value));



        var tamedCreatures = creatures.Values.Where(c => c.IsTamed).ToDictionary(x => x.Id);
        var wildCreatures = creatures.Values.Where(c => !c.IsTamed).ToDictionary(x => x.Id);


        return new AsaSaveGame
        {
            SaveVersion = header.SaveVersion,
            Players = players,
            Tribes = tribes,
            TamedCreatures = tamedCreatures,
            WildCreatures = wildCreatures,
            Structures = structures,
            DroppedItems = droppedItems,
        }; 
    }
}
