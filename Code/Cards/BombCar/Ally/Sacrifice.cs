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
// 选择1名队友，对其造成5点伤害。使自身获得5点力量，抽2张牌
// 选择1名队友，对其造成3点伤害。使自身获得5点力量，抽2张牌

[Pool(typeof(ColorlessCardPool))]
public sealed class Sacrifice() : CustomCardModel(2, CardType.Attack, CardRarity.Common, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];
    
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DamageVar(5, ValueProp.Move),
        new PowerVar<StrengthPower>(5)
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, this);

        await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
	}

	protected override void OnUpgrade()
	{
        EnergyCost.UpgradeBy(-1);
		DynamicVars.Damage.UpgradeValueBy(-2m);
	}
}
