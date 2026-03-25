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

namespace Test.Code.Cards.Rare;

[Pool(typeof(IroncladCardPool))]
public sealed class VampiricShield() : CustomCardModel(0, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var allyC = cardPlay.Target;
        var allyP = cardPlay.Target.Player;
        var owner = cardPlay.Card.Owner;

        if (allyP == null)
        {
            return;
        }

        double allyBlock = allyC.Block;
        decimal gainBlock = (decimal)Math.Floor(allyBlock / 2);

        await CreatureCmd.LoseBlock(allyC, gainBlock);
        await CreatureCmd.Heal(owner.Creature, gainBlock, true);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}