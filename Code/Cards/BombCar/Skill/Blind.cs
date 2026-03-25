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
// 盲目
// 消耗3点生命，获得9层覆甲
// 消耗5点生命，抽4张牌，获得3层力量

[Pool(typeof(ColorlessCardPool))]
public sealed class Blind() : CustomCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    [
        HoverTipFactory.FromPower<PlatingPower>()
    ];
	
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(3m),
        new PowerVar<PlatingPower>(9m),
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        BombCarCardUtil.HpLoss(choiceContext, Owner, DynamicVars, this);

        await PowerCmd.Apply<DexterityPower>(Owner.Creature, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
        DynamicVars["PlatingPower"].UpgradeValueBy(9m);
	}
}
