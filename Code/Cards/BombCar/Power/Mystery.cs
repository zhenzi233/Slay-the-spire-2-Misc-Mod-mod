using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;
using Test.Code.Powers;
using Test.Code.Powers.BombCar;

namespace Test.Code.Cards.BombCar.Skill;
// 神秘
// 每回合获得2点能量，3点灾厄
// 每回合获得3点能量，1点灾厄


[Pool(typeof(ColorlessCardPool))]
public sealed class Mystery() : CustomCardModel(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DoomPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2),
        new PowerVar<DoomPower>(3)
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<MysteryUpgradePower>(Owner.Creature, 1, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<MysteryPower>(Owner.Creature, 1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
        DynamicVars["DoomPower"].UpgradeValueBy(-2m);
    }
}
