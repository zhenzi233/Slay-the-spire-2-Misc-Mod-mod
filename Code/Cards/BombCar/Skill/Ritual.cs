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
// 仪式
// 消耗5点生命，获得已损失生命数量的护盾，使自身获得仪式
// 消耗5点生命，抽4张牌，获得3层力量

[Pool(typeof(ColorlessCardPool))]
public sealed class Ritual() : CustomCardModel(2, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips {
        get
        {
            List<IHoverTip> list = new List<IHoverTip>();
            list.Add(HoverTipFactory.FromPower<RitualPower>());
            return list;
        }
    }
	
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(5m),
        new PowerVar<RitualPower>(1m),
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        BombCarCardUtil.HpLoss(choiceContext, Owner, DynamicVars, this);

        var losedHp = Owner.Creature.MaxHp - Owner.Creature.CurrentHp;

        await CreatureCmd.GainBlock(Owner.Creature, losedHp, ValueProp.Move, cardPlay);

        await PowerCmd.Apply<RitualPower>(Owner.Creature, DynamicVars["RitualPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
        AddKeyword(CardKeyword.Retain);
	}
}
