using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Test.Code.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Commands.Builders;

namespace Test.Code.Powers;



public sealed class CoveredAmbushPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => CustomPackedIconPath;

    public override async Task AfterBlockGained(
        Creature creature, 
        decimal amount, 
        ValueProp props, 
        CardModel? cardSource)
    {
        if (creature.IsPlayer && creature != Owner)
        {
            await DamageCmd.Attack(Amount)
            .TargetingRandomOpponents(CombatState)
            .WithHitCount(1)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(new ThrowingPlayerChoiceContext());
            Flash();
        }
    }
}