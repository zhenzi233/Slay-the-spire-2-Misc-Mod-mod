using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
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

namespace Test.Code.Cards.BombCar.Skill;
// 苦盐
// 消耗所有手牌，对敌人叠加手牌数量的灾厄与虚弱

[Pool(typeof(ColorlessCardPool))]
public sealed class CursedSalt() : CustomCardModel(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
	
	protected override IEnumerable<DynamicVar> CanonicalVars => [
    ];
    
	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        List<CardModel> list = PileType.Hand.GetPile(base.Owner).Cards.ToList();
        int cardCount = list.Count;
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item);
		}

        if (IsUpgraded)
        {
            cardCount = cardCount * 2;
        }

        await PowerCmd.Apply<WeakPower>(cardPlay.Target, cardCount, Owner.Creature, this);
        await PowerCmd.Apply<DoomPower>(cardPlay.Target, cardCount, Owner.Creature, this);
	}

	protected override void OnUpgrade()
    {
    }
}
