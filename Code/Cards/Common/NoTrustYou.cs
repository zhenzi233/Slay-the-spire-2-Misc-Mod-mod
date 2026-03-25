using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Test.Code.Extensions;

namespace Test.Code.Cards.Common;

[Pool(typeof(ColorlessCardPool))]
public sealed class NoTrustYou() : CustomCardModel(0, CardType.Skill, CardRarity.Common, TargetType.AnyAlly)
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        base.EnergyHoverTip
    ];
	
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new IntVar("Multi", 1),
		new EnergyVar(1)
	];

	public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target);
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

		var ally = cardPlay.Target.Player;
		var owner = cardPlay.Card.Owner;

		if (ally == null)
		{
			return;
		}

		var allyCs = ally.PlayerCombatState;
		if (allyCs != null)
		{
			var allyEnergy = allyCs.Energy;
			for (var i = 0; i < allyEnergy; i++)
			{
				await PlayerCmd.LoseEnergy(1, ally);
				var gainEnergy = DynamicVars["Multi"].IntValue;
				await PlayerCmd.GainEnergy(gainEnergy, owner);
				if (i != allyEnergy - 1)
				{
					await Cmd.CustomScaledWait(0.15f, 0.25f);
				}
			}
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars["Multi"].UpgradeValueBy(1m);
	}
}
