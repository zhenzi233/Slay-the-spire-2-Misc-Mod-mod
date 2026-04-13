using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.CardPools;
using Test.Code.Cards.Extension;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Wriggler;

// 蠕动
// （保留）结束本回合，获得等同其他玩家最高血量数值的护甲。

[Pool(typeof(ColorlessCardPool))]
public sealed class Squirm() : CustomCardModel(1, CardType.Skill, CardRarity.Common, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public override HashSet<CardKeyword> CanonicalKeywords =>
    [
    ];

    public override string PortraitPath => StringExtensions.WrigglerCard();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);


        var allyP = cardPlay.Target.Player;

        await CreatureCmd.GainBlock(base.Owner.Creature, new BlockVar(cardPlay.Target.CurrentHp, ValueProp.Move), cardPlay, false);
        await CreatureCmd.GainBlock(cardPlay.Target, new BlockVar(cardPlay.Target.CurrentHp, ValueProp.Move), cardPlay, false);

        PlayerCmd.EndTurn(base.Owner, false);
    }

    // 将三张锻造材料+放入所有玩家的抽牌堆。
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}