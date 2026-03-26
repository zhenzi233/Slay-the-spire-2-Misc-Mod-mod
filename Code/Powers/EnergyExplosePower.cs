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
using Test.Code.Cards.Extension;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Test.Code.Cards.Rare;
using Test.Code.Cards.Rare.Power;

namespace Test.Code.Powers;


// 其他玩家每消耗一点能量，本回合就获得等量的集中。

public sealed class EnergyExplosePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => CustomPackedIconPath;

    public override async Task AfterEnergySpent(CardModel card, int amount)
	{
        if (card.Owner.Creature == base.Owner || amount == 0)
        {
            return;
        }
        await PowerCmd.Apply<EnergyExploseFocusPower>(base.Owner, amount, base.Owner, ModelDb.Card<EnergyExplose>());
        Flash();
	}
}