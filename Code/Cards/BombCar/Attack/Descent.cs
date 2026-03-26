using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HarmonyLib;
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

namespace Test.Code.Cards.BombCar.Attack;
// 降临
// 造成你已损失生命*X能量的伤害
// 获得X点力量，造成你已损失生命*X能量的伤害


[Pool(typeof(ColorlessCardPool))]
public sealed class Descent() : CustomCardModel(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [

    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    protected override bool HasEnergyCostX => true;

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int num = ResolveEnergyXValue();
        if (IsUpgraded)
        {
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, num, Owner.Creature, this);
        }

        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        int damageValue = num * (Owner.Creature.MaxHp - Owner.Creature.CurrentHp);
        await DamageCmd.Attack(damageValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_starry_impact")
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        ExtraHoverTips.AddItem(HoverTipFactory.FromPower<StrengthPower>());
    }
}
