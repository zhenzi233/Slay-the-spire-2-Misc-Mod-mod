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
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.BombCar.Ally;
// 献身
// 本回合敌人造成的伤害只会打在你身上
// 本回合敌人造成的伤害只会打在你身上，你的护盾不再消失

[Pool(typeof(ColorlessCardPool))]
public sealed class Devotion() : CustomCardModel(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
    
	protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var allys = CombatState.Allies;
        foreach (Creature ally in allys)
        {
            await PowerCmd.Apply<DevotionDefectPower>(ally, 1, Owner.Creature, this);
        }

        if (IsUpgraded)
        {
            await PowerCmd.Apply<BarricadePower>(Owner.Creature, 1, Owner.Creature, this);
        }
	}

	protected override void OnUpgrade()
	{
	}
}
