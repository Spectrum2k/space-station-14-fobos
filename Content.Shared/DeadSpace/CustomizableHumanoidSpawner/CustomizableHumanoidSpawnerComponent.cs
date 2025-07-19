// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Dataset;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Tag;
using Robust.Shared.Enums;

namespace Content.Shared.DeadSpace.CustomizableHumanoidSpawner;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class CustomizableHumanoidSpawnerComponent : Component
{
    // Отключает настройку персонажа и сразу спавнит рандомного.
    [DataField]
    public bool ForceRandom;

    // Прототип должности которая будет заспавнена
    [DataField(required: true)]
    public ProtoId<JobPrototype> JobPrototype;

    // Разрешённые расы
    [DataField(serverOnly: true)]
    public List<ProtoId<SpeciesPrototype>> AllowedSpecies = [];

    // Датасет со списком имен для первой части имени
    [DataField(serverOnly: true)]
    public ProtoId<DatasetPrototype>? RandomNameFirstDataset;

    // Датасет со списком имен для второй части имени (для имени из одного слова - укажите только датасет первой части)
    [DataField(serverOnly: true)]
    public ProtoId<DatasetPrototype>? RandomNameSecondDataset;

    // Локализованный датасет имен для первой части имени. Игнорируется если указан обычный датасет.
    [DataField(serverOnly: true)]
    public ProtoId<LocalizedDatasetPrototype>? RandomNameFirstLocalized;

    // Локализованный датасет имен для второй части имени. Игнорируется если указан обычный датасет.
    [DataField(serverOnly: true)]
    public ProtoId<LocalizedDatasetPrototype>? RandomNameSecondLocalized;

    // Список тегов которые будут добавлены к персонажу
    [DataField(serverOnly: true)]
    public List<ProtoId<TagPrototype>>? Tags;

    // Список фракций которые будут добавлены к персонажу
    [DataField(serverOnly: true)]
    public HashSet<ProtoId<NpcFactionPrototype>>? Factions;
}

[Serializable, NetSerializable]
public sealed class CustomizableHumanoidSpawnerCharacterInfo(
    string name,
    string description,
    ProtoId<SpeciesPrototype> species,
    Gender gender,
    int index)
{
    public string Name = name;
    public string Description = description;
    public ProtoId<SpeciesPrototype> Species = species;
    public Gender Gender = gender;
    public int Index = index;
}

[Serializable, NetSerializable]
public sealed class CustomizableHumanoidSpawnerBuiState(
    List<CustomizableHumanoidSpawnerCharacterInfo> characters,
    bool canChangeNameAndDescription,
    string? randomizedName) : BoundUserInterfaceState
{
    public readonly List<CustomizableHumanoidSpawnerCharacterInfo> Characters = characters;
    public readonly bool CanChangeNameAndDescription = canChangeNameAndDescription;
    public readonly string? RandomizedName = randomizedName;
}

[Serializable, NetSerializable]
public sealed class CustomizableHumanoidSpawnerMessage(
    bool useRandom,
    int characterIndex,
    bool useCustomName,
    string customName,
    bool useCustomDescription,
    string customDescription) : BoundUserInterfaceMessage
{
    public bool UseRandom = useRandom;
    public int CharacterIndex = characterIndex;
    public bool UseCustomName = useCustomName;
    public string CustomName = customName;
    public bool UseCustomDescription = useCustomDescription;
    public string CustomDescription = customDescription;
}

[Serializable, NetSerializable]
public enum CustomizableHumanoidSpawnerUiKey : byte
{
    Key,
}
