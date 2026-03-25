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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Test.Code.Extensions;

namespace Test.Code.Cards.BombCar;
// 宣言
// 消耗1点生命，获得3层力量。

public static class BombCarCardUtil
{
    public static async void HpLoss(PlayerChoiceContext choiceContext, Player owner, DynamicVarSet vars, CardModel card)
    {
        await CreatureCmd.Damage(choiceContext, owner.Creature, vars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
    }

    public static async void HpLoss(PlayerChoiceContext choiceContext, Player owner, decimal num, CardModel card)
    {
        await CreatureCmd.Damage(choiceContext, owner.Creature, num, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
    }
}
