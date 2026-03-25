using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Cards.Extension;
using Test.Code.Extensions;
using Test.Code.Powers;

namespace Test.Code.Cards.Rare;

// 机械飞升
// 激发另一名玩家的全部充能球。若没有，则令其充能球栏+2并往其充能球栏随机填满充能球。
  
[Pool(typeof(DefectCardPool))]
public sealed class MechanicalAscension() : CustomCardModel(3, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Channeling)
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
        var allyP = allyC.Player;
        var allyPcs = allyP.PlayerCombatState;

        var orbCount = allyPcs.OrbQueue.Orbs.Count;
        if (orbCount <= 0)
        {
            await OrbCmd.AddSlots(allyP, 2);
            for (int i = 0; i < orbCount; i++)
            {
                await OrbCmd.Channel(choiceContext, OrbModel.GetRandomOrb(allyP.RunState.Rng.CombatOrbGeneration).ToMutable(), allyP);
            }
        }   
        else
        {
            for (int i = 0; i < orbCount; i++)
            {
                await OrbCmd.EvokeNext(choiceContext, allyP);
                if (i != orbCount - 1)
                {
                    await Cmd.CustomScaledWait(0.15f, 0.25f);
                }
            }
        }
    }

// 将三张锻造材料+放入所有玩家的抽牌堆。
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}