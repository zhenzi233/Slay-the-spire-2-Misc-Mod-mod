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
using Test.Code.Powers.BombCar;

namespace Test.Code.Cards.BombCar.Skill;
// 圣洁
// 使所有敌人在本回合内失去9点力量
// 消耗5点生命，抽4张牌，获得3层力量


[Pool(typeof(ColorlessCardPool))]
public sealed class Sanctity() : CustomCardModel(2, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];
	
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(9m),
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        foreach (Creature enemy in CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<SanctityPower>(enemy, -DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
        EnergyCost.UpgradeBy(-2);
	}
}
