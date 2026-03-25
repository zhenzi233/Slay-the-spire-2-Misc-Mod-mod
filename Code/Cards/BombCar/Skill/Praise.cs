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
// 赞美
// 消耗1点生命，获得9层敏捷
// 消耗5点生命，抽4张牌，获得3层力量


[Pool(typeof(ColorlessCardPool))]
public sealed class Praise() : CustomCardModel(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    [
        HoverTipFactory.FromPower<DexterityPower>()
    ];
	
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(1m),
        new PowerVar<DexterityPower>(9m),
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        BombCarCardUtil.HpLoss(choiceContext, Owner, DynamicVars, this);

        await PowerCmd.Apply<DexterityPower>(Owner.Creature, DynamicVars.Dexterity.BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
        DynamicVars.Dexterity.UpgradeValueBy(9m);
	}
}
