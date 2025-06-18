// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.JustSpawner;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.JustSpawner;

[UsedImplicitly]
public sealed class JustSpawnerBUI(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private JustSpawnerUI? _ui;

    protected override void Open()
    {
        base.Open();
        _ui = this.CreateWindow<JustSpawnerUI>();
        _ui.OnConfirm += Send;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_ui == null || state is not JustSpawnerBuiState msg)
            return;

        _ui.SetData(msg);
    }

    private void Send(
        bool useRandom,
        int characterIndex,
        bool useCustomName,
        string customName,
        bool useCustomDescription,
        string customDescription)
    {
        SendMessage(new JustSpawnerMessage(
            useRandom,
            characterIndex,
            useCustomName,
            customName,
            useCustomDescription,
            customDescription));
    }
}
