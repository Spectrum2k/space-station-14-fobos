// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Diagnostics.CodeAnalysis;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Shared.DeadSpace.JustSpawner;
using Content.Shared.Mind.Components;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Content.Shared.Speech;

namespace Content.Server.DeadSpace.JustSpawner;

public sealed class JustSpawnerSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StationSystem _stations = default!;
    [Dependency] private readonly StationSpawningSystem _spawning = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;
    [Dependency] private readonly SpeechSystem _speech = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JustSpawnerComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<JustSpawnerComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<JustSpawnerComponent, JustSpawnerMessage>(OnUiMessage);
        SubscribeLocalEvent<JustSpawnerComponent, TakeGhostRoleEvent>(OnTakeGhostRole);
    }

    private void OnMindAdded(EntityUid uid, JustSpawnerComponent comp, MindAddedMessage args)
    {
        if (TryComp(uid, out GhostRoleComponent? ghost))
            _ghostRole.UnregisterGhostRole((uid, ghost));

        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        _speech.SetSpeech(uid, false);
        if (comp.ForceRandom || GetAvailableCharacters(actor, comp).Count == 0)
        {
            Spawn(
                uid,
                comp,
                actor,
                true,
                0,
                false,
                string.Empty,
                false,
                string.Empty);

            return;
        }

        _ui.OpenUi(uid, JustSpawnerUiKey.Key, actor.PlayerSession);
    }

    private void OnUiOpened(EntityUid uid, JustSpawnerComponent comp, BoundUIOpenedEvent args)
    {
        if (!TryComp<ActorComponent>(args.Actor, out var actor))
            return;

        var state = new JustSpawnerBuiState(GetAvailableCharacters(actor, comp), !TryGetRandomName(comp, out _));
        _ui.SetUiState(uid, JustSpawnerUiKey.Key, state);
    }

    private void OnUiMessage(EntityUid uid, JustSpawnerComponent comp, JustSpawnerMessage msg)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        Spawn(uid,
            comp,
            actor,
            msg.UseRandom,
            msg.CharacterIndex,
            msg.UseCustomName,
            msg.CustomName,
            msg.UseCustomDescription,
            msg.CustomDescription);
    }

    private void Spawn(EntityUid uid,
        JustSpawnerComponent comp,
        ActorComponent actor,
        bool isRandomCharacter,
        int characterIndex,
        bool useCustomName,
        string customName,
        bool useCustomDescription,
        string customDescription)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mindComp))
            return;

        if (!_transform.TryGetMapOrGridCoordinates(uid, out var coords))
            return;

        HumanoidCharacterProfile profile;
        if (isRandomCharacter)
        {
            if (comp.AllowedSpecies.Count == 0)
            {
                profile = HumanoidCharacterProfile.Random();
            }
            else
            {
                var species = _random.Pick(comp.AllowedSpecies);
                profile = HumanoidCharacterProfile.RandomWithSpecies(species);
            }
        }
        else
        {
            var prefs = _prefs.GetPreferences(actor.PlayerSession.UserId);
            profile = (HumanoidCharacterProfile)prefs.GetProfile(characterIndex);

            if (useCustomName)
                profile = profile.WithName(customName);

            if (useCustomDescription)
                profile = profile.WithFlavorText(customDescription);
        }

        if (TryGetRandomName(comp, out var randomName))
            profile = profile.WithName(randomName);

        var station = _stations.GetOwningStation(uid);
        var newEntity = _spawning.SpawnPlayerMob(coords.Value, comp.JobPrototype, profile, station);
        _mind.TransferTo(mindId, newEntity, true, mind: mindComp);

        QueueDel(uid);
    }

    private void OnTakeGhostRole(EntityUid uid, JustSpawnerComponent comp, ref TakeGhostRoleEvent args)
    {
        if (args.TookRole)
            return;

        args.TookRole = true;

        if (TryComp(uid, out GhostRoleComponent? ghost))
            _ghostRole.UnregisterGhostRole((uid, ghost));

        _mind.ControlMob(args.Player.UserId, uid);
    }

    private List<JustSpawnerCharacterInfo> GetAvailableCharacters(ActorComponent actor, JustSpawnerComponent comp)
    {
        var userId = actor.PlayerSession.UserId;
        var prefs = _prefs.GetPreferences(userId);
        var characters = new List<JustSpawnerCharacterInfo>();
        foreach (var (index, profile) in prefs.Characters)
        {
            if (profile is not HumanoidCharacterProfile humanoid)
                continue;

            if (!comp.AllowedSpecies.Contains(humanoid.Species))
                continue;

            var flavor = humanoid.FlavorText;
            characters.Add(new JustSpawnerCharacterInfo(
                humanoid.Name,
                flavor,
                humanoid.Species,
                humanoid.Gender,
                index));
        }

        return characters;
    }

    private bool TryGetRandomName(JustSpawnerComponent comp, [NotNullWhen(true)] out string? name)
    {
        var firstPart = TryGetRandomNameFirst(comp);
        var secondsPart = TryGetRandomNameSecond(comp);

        if (firstPart is null)
        {
            name = null;
            return false;
        }

        name = secondsPart is null ? firstPart : $"{firstPart} {secondsPart}";
        return true;
    }

    private string? TryGetRandomNameFirst(JustSpawnerComponent comp)
    {
        if (comp.RandomNameFirstParts.Count > 0)
            return comp.RandomNameFirstParts[_random.Next(0, comp.RandomNameFirstParts.Count)];

        if (comp.RandomNameFirstDataset is not null)
            return _random.Pick(_prototypeManager.Index(comp.RandomNameFirstDataset.Value).Values);

        if (comp.RandomNameFirstLocalized is not null)
            return Loc.GetString(_random.Pick(_prototypeManager.Index(comp.RandomNameFirstLocalized.Value).Values));

        return null;
    }

    private string? TryGetRandomNameSecond(JustSpawnerComponent comp)
    {
        if (comp.RandomNameSecondParts.Count > 0)
            return comp.RandomNameSecondParts[_random.Next(0, comp.RandomNameSecondParts.Count)];

        if (comp.RandomNameSecondDataset is not null)
            return _random.Pick(_prototypeManager.Index(comp.RandomNameSecondDataset.Value).Values);

        if (comp.RandomNameSecondLocalized is not null)
            return Loc.GetString(_random.Pick(_prototypeManager.Index(comp.RandomNameSecondLocalized.Value).Values));

        return null;
    }
}
