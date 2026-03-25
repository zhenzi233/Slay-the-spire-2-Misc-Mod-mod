using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;

namespace Test.Code.Cards.BombCar.Ally;
// 信仰
// 消耗一半当前生命，所有队友获得已损失生命数量的护盾
// 选择1名队友，对其造成3点伤害。使自身获得5点力量，抽2张牌

[Pool(typeof(ColorlessCardPool))]
public sealed class Faith() : CustomCardModel(2, CardType.Skill, CardRarity.Common, TargetType.AllAllies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];
    
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        decimal hp = Math.Floor((decimal) Owner.Creature.CurrentHp / 2);
        BombCarCardUtil.HpLoss(choiceContext, Owner, hp, this);

        var allys = CombatState.Allies;
        foreach (Creature ally in allys)
        {
            await CreatureCmd.GainBlock(ally, hp, ValueProp.Move, cardPlay);
        }
	}

	protected override void OnUpgrade()
	{
        EnergyCost.UpgradeBy(-1);
	}
}
