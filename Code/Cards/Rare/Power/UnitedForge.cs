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
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Cards.Extension;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Rare.Power;

// 齐心锻造
// 将三张锻造材料放入所有玩家的抽牌堆。
[Pool(typeof(RegentCardPool))]
public sealed class UnitedForge() : CustomCardModel(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<UnitedForgePower>(),
        HoverTipFactory.FromCard<ForgingMaterials>(base.IsUpgraded),
        HoverTipFactory.Static(StaticHoverTip.Forge, base.DynamicVars.Forge)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new ForgeVar(8)
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var owner = cardPlay.Card.Owner;
        var combatState = cardPlay.Card.CombatState;

        await PowerCmd.Apply<UnitedForgePower>(owner.Creature, DynamicVars.Forge.BaseValue, owner.Creature, this);

        IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
			where c != null && c.IsAlive && c.IsPlayer
			select c;
        foreach (Creature creature in enumerable)
        {
            List<ForgingMaterials> cards = ForgingMaterials.Create(creature.Player, base.DynamicVars.Cards.IntValue, base.CombatState, base.IsUpgraded).ToList();
            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
			if (LocalContext.IsMe(creature))
			{
				CardCmd.PreviewCardPileAdd(results);
			}
        }
    }

// 将三张锻造材料+放入所有玩家的抽牌堆。
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Forge.UpgradeValueBy(2);
    }
}