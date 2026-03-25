using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;

namespace Test.Code.Cards.BombCar.Ally;
// 背弃
// 选择1名队友，对其造成5点伤害。选择1张牌添加重放
// 选择1名队友，对其造成5点伤害。选择2张牌添加重放

[Pool(typeof(ColorlessCardPool))]
public sealed class UltimateBetrayal() : CustomCardModel(2, CardType.Attack, CardRarity.Common, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
    ];
    
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DamageVar(5, ValueProp.Move)
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, this);

        IEnumerable<CardModel> cards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, (int) DynamicVars.Cards.BaseValue),
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
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
