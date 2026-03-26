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

namespace Test.Code.Cards.BombCar.Attack;
// 狂乱
// 对所有敌人造成已损失生命*自身力量层数的伤害
// 选择1名队友，对其造成3点伤害。使自身获得5点力量，抽2张牌

[Pool(typeof(ColorlessCardPool))]
public sealed class Chaos() : CustomCardModel(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damageValue = (Owner.Creature.MaxHp - Owner.Creature.CurrentHp) * Owner.Creature.GetPowerAmount<StrengthPower>();
        var allEnemies = CombatState.Enemies;
        if (damageValue < 0 && allEnemies != null)
        {
            foreach (var enemy in allEnemies)
            {
                await CreatureCmd.Heal(enemy, damageValue);
            }
        }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_starry_impact")
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
