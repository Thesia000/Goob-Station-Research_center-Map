// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Marcus F <marcus2008stoke@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <marcus2008stoke@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.MartialArts;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedMartialArtsSystem
{
    private void InitializeKravMaga()
    {
        SubscribeLocalEvent<KravMagaComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<KravMagaComponent, KravMagaActionEvent>(OnKravMagaAction);
        SubscribeLocalEvent<KravMagaComponent, MeleeHitEvent>(OnMeleeHitEvent);
        SubscribeLocalEvent<KravMagaComponent, ComponentShutdown>(OnKravMagaShutdown);
    }

    private void OnMeleeHitEvent(Entity<KravMagaComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count <= 0)
            return;

        foreach (var hitEntity in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hitEntity))
                continue;
            if (!TryComp<RequireProjectileTargetComponent>(hitEntity, out var isDowned))
                continue;

            DoKravMaga(ent, hitEntity, isDowned);
        }
    }

    private void DoKravMaga(Entity<KravMagaComponent> ent, EntityUid hitEntity, RequireProjectileTargetComponent reguireProjectileTargetComponent)
    {
        if (ent.Comp.SelectedMoveComp == null)
            return;
        var moveComp = ent.Comp.SelectedMoveComp;

        switch (ent.Comp.SelectedMove)
        {
            case KravMagaMoves.LegSweep:
                if(_netManager.IsClient)
                    return;
                _stun.TryKnockdown(hitEntity, TimeSpan.FromSeconds(4), true); // okay buddy
                _stamina.TakeStaminaDamage(hitEntity, 25f, applyResistances: true);
                break;
            case KravMagaMoves.NeckChop:
                var comp = EnsureComp<KravMagaSilencedComponent>(hitEntity);
                comp.SilencedTime = _timing.CurTime + TimeSpan.FromSeconds(moveComp.EffectTime);
                break;
            case KravMagaMoves.LungPunch:
                _stamina.TakeStaminaDamage(hitEntity, moveComp.StaminaDamage, applyResistances: true);
                var blockedBreathingComponent = EnsureComp<KravMagaBlockedBreathingComponent>(hitEntity);
                blockedBreathingComponent.BlockedTime = _timing.CurTime + TimeSpan.FromSeconds(moveComp.EffectTime);
                break;
            case null:
                var damage = ent.Comp.BaseDamage;
                if (reguireProjectileTargetComponent.Active)
                    damage *= ent.Comp.DownedDamageModifier;

                DoDamage(ent.Owner, hitEntity, "Blunt", damage, out _);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ent.Comp.SelectedMove = null;
        ent.Comp.SelectedMoveComp = null;
    }

    private void OnKravMagaAction(Entity<KravMagaComponent> ent, ref KravMagaActionEvent args)
    {
        var actionEnt = args.Action.Owner;
        if (!TryComp<KravMagaActionComponent>(actionEnt, out var kravActionComp))
            return;

        _popupSystem.PopupClient(Loc.GetString("krav-maga-ready", ("action", kravActionComp.Name)), ent, ent);
        ent.Comp.SelectedMove = kravActionComp.Configuration;
        ent.Comp.SelectedMoveComp = kravActionComp;
    }

    private void OnMapInit(Entity<KravMagaComponent> ent, ref MapInitEvent args)
    {
        if (HasComp<MartialArtsKnowledgeComponent>(ent) || HasComp<ChangelingIdentityComponent>(ent))
            return;

        foreach (var actionId in ent.Comp.BaseKravMagaMoves)
        {
            var actions = _actions.AddAction(ent, actionId);
            if (actions != null)
                ent.Comp.KravMagaMoveEntities.Add(actions.Value);
        }
    }

    private void OnKravMagaShutdown(Entity<KravMagaComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<KravMagaComponent>(ent, out var kravMaga))
            return;

        foreach (var action in ent.Comp.KravMagaMoveEntities)
        {
            _actions.RemoveAction(action);
        }
    }
}
