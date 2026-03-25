using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;

namespace Test.Code.Cards.BombCar.Skill;
// 狂热
// 消耗一半当前生命，抽4张牌，将其打出

[Pool(typeof(ColorlessCardPool))]
public sealed class BlindFrenzy() : CustomCardModel(3, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
	
	protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            List<DynamicVar> list = new List<DynamicVar>();
            list.Add(new CardsVar(4));
            return list;
        }
    }
	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        var lossHp = Math.Floor((decimal) Owner.Creature.CurrentHp / 2);
        BombCarCardUtil.HpLoss(choiceContext, Owner, lossHp, this);
        await CardPileCmd.AutoPlayFromDrawPile(choiceContext, Owner, (int) DynamicVars.Cards.BaseValue, CardPilePosition.Top, false);
	}

	protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
