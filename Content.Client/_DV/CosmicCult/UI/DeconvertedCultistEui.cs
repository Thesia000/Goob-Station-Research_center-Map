// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;

namespace Content.Client._DV.CosmicCult.UI;

public sealed class DeconvertedCultistEui : BaseEui
{
    private readonly CosmicDeconvertedMenu _menu;

    public DeconvertedCultistEui() => _menu = new CosmicDeconvertedMenu();

    public override void Opened() => _menu.OpenCentered();

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }
}
