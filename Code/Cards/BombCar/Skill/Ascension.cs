using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;

namespace Test.Code.Cards.BombCar.Ally;
// 升格
// 选择1张牌添加重放，获得3点能量，将3张灵魂添加到你的手牌
// 选择1张牌添加重放，获得3点能量，将3张灵魂添加到你的手牌


[Pool(typeof(ColorlessCardPool))]
public sealed class Ascension() : CustomCardModel(2, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
        HoverTipFactory.FromCard<Soul>()
    ];
    
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Replay", 1),
        new EnergyVar(3),
        new CardsVar(3)
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        IEnumerable<CardModel> cards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, (int) DynamicVars["Replay"].BaseValue),
            (CardModel c) => !c.Keywords.Contains(CardKeyword.Unplayable),
            this);

        foreach (CardModel card in cards)
        {
            if (card != null)
            {
                card.BaseReplayCount += 1;
                CardCmd.Preview(card);
            }
        }

        await PlayerCmd.GainEnergy(3, Owner);

        var soul = Soul.Create(Owner, 3, CombatState);
        
        if (LocalContext.IsMe(Owner))
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(soul, PileType.Hand, true));
        }
	}

	protected override void OnUpgrade()
	{
		EnergyCost.UpgradeBy(-1);
	}
}
