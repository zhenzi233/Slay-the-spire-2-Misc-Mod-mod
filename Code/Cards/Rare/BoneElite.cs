using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Cards.Extension;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Rare;

// 骸骨精英
// 消耗，另一名玩家召唤等同你血量一半的数值。
[Pool(typeof(NecrobinderCardPool))]
public sealed class BoneElite() : CustomCardModel(3, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.SummonDynamic, base.DynamicVars.Summon)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var allyC = cardPlay.Target;
        double cal = Owner.Creature.CurrentHp / 2;
        decimal summonValue = (decimal)Math.Floor(cal);

        await OstyCmd.Summon(choiceContext, allyC.Player, summonValue, this);
    }

    // 另一名玩家召唤等同你血量一半的数值。
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        RemoveKeyword(CardKeyword.Exhaust);
    }
}