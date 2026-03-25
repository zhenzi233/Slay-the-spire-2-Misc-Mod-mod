using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Rare;

  
[Pool(typeof(IroncladCardPool))]
public sealed class ThisIsTheOne() : CustomCardModel(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<ThisIsTheOnePowerA>(),
        HoverTipFactory.FromPower<ThisIsTheOnePowerB>()
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var allyC = cardPlay.Target;
        var allyP = cardPlay.Target.Player;
        var owner = cardPlay.Card.Owner;
        var combatState = cardPlay.Card.CombatState;

        if (combatState == null)
        {
            return;
        }
        IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
			where c != null && c.IsAlive && c.IsPlayer
			select c;
        foreach (Creature playerC in enumerable)
        {
            if (playerC != allyC)
            {
                await PowerCmd.Apply<ThisIsTheOnePowerA>(playerC, 1, owner.Creature, this, false);
            }
        }

        await PowerCmd.Apply<ThisIsTheOnePowerB>(allyC, 1, owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}