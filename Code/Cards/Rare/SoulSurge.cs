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

// 灵魂涌动
// 在所有玩家的抽牌堆中加入2张灵魂。其他玩家消耗灵魂时，你召唤8/10

[Pool(typeof(NecrobinderCardPool))]
public sealed class SoulSurge() : CustomCardModel(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SoulSurgePower>(),
        HoverTipFactory.FromCard<Soul>(),
        HoverTipFactory.Static(StaticHoverTip.SummonDynamic, base.DynamicVars.Summon)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new SummonVar(8)
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var owner = cardPlay.Card.Owner;
        var combatState = cardPlay.Card.CombatState;

        await PowerCmd.Apply<SoulSurgePower>(owner.Creature, DynamicVars.Summon.BaseValue, owner.Creature, this);

        IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
                                           where c != null && c.IsAlive && c.IsPlayer
                                           select c;
        foreach (Creature creature in enumerable)
        {
            List<Soul> cards = Soul.Create(creature.Player, base.DynamicVars.Cards.IntValue, base.CombatState).ToList();
            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
            if (LocalContext.IsMe(creature))
            {
                CardCmd.PreviewCardPileAdd(results);
            }
        }
    }

    // 在所有玩家的抽牌堆中加入3张灵魂。其他玩家消耗灵魂时，你召唤8/10
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Summon.UpgradeValueBy(2);
    }
}