using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Rare.Skill;

  
[Pool(typeof(SilentCardPool))]
public sealed class DeliveringCharcoal() : CustomCardModel(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        base.EnergyHoverTip,
        HoverTipFactory.FromPower<DeliveringCharcoalPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2)
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var owner = cardPlay.Card.Owner;
        var allyC = cardPlay.Target;
        var allyP = cardPlay.Target.Player;

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, allyP);
        await PowerCmd.Apply<DeliveringCharcoalPower>(allyC, 1, owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}