// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.Dataset;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Enums;

namespace Content.Shared.DeadSpace.JustSpawner;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class JustSpawnerComponent : Component
{
    [DataField]
    public bool ForceRandom;

    [DataField(required: true)]
    public ProtoId<JobPrototype> JobPrototype;

    [DataField(serverOnly: true)]
    public List<ProtoId<SpeciesPrototype>> AllowedSpecies = [];

    [DataField(serverOnly: true)]
    public List<string> RandomNameFirstParts = [];

    [DataField(serverOnly: true)]
    public List<string> RandomNameSecondParts = [];

    [DataField(serverOnly: true)]
    public ProtoId<LocalizedDatasetPrototype>? RandomNameFirstLocalized;

    [DataField(serverOnly: true)]
    public ProtoId<LocalizedDatasetPrototype>? RandomNameSecondLocalized;

    [DataField(serverOnly: true)]
    public ProtoId<DatasetPrototype>? RandomNameFirstDataset;

    [DataField(serverOnly: true)]
    public ProtoId<DatasetPrototype>? RandomNameSecondDataset;
}

[Serializable, NetSerializable]
public sealed class JustSpawnerCharacterInfo(
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
public sealed class JustSpawnerBuiState(List<JustSpawnerCharacterInfo> characters, bool canChangeNameAndDescription)
    : BoundUserInterfaceState
{
    public readonly List<JustSpawnerCharacterInfo> Characters = characters;
    public readonly bool CanChangeNameAndDescription = canChangeNameAndDescription;
}

[Serializable, NetSerializable]
public sealed class JustSpawnerMessage(
    bool useRandom,
    int characterIndex,
    bool useCustomName,
    string customName,
    bool useCustomDescription,
    string customDescription)
    : BoundUserInterfaceMessage
{
    public bool UseRandom = useRandom;
    public int CharacterIndex = characterIndex;
    public bool UseCustomName = useCustomName;
    public string CustomName = customName;
    public bool UseCustomDescription = useCustomDescription;
    public string CustomDescription = customDescription;
}

[Serializable, NetSerializable]
public enum JustSpawnerUiKey : byte
{
    Key,
}
