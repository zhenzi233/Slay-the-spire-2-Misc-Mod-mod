using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Test.Code.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace Test.Code.Powers;




public sealed class DevotionAttackPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => CustomPackedIconPath;

    private class Data
	{
		public readonly List<Creature> coveredCreatures = new List<Creature>();
	}

	private const string _coveringKey = "Covering";

	protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new StringVar("Covering")
    ];

	protected override object InitInternalData()
	{
		return new Data();
	}

	public void AddCoveredCreature(Creature c)
	{
		List<Creature> coveredCreatures = GetInternalData<Data>().coveredCreatures;
		if (!GetInternalData<Data>().coveredCreatures.Contains(c))
		{
			coveredCreatures.Add(c);
		}
		StringVar stringVar = (StringVar)base.DynamicVars["Covering"];
		stringVar.StringValue = "";
		for (int i = 0; i < coveredCreatures.Count; i++)
		{
			stringVar.StringValue += PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, coveredCreatures[i].Player.NetId);
			if (i == coveredCreatures.Count - 2)
			{
				stringVar.StringValue += ", and ";
			}
			else if (i < coveredCreatures.Count - 2)
			{
				stringVar.StringValue += ", ";
			}
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return 1m;
		}
		if (!props.IsPoweredAttack_())
		{
			return 1m;
		}
		return GetInternalData<Data>().coveredCreatures.Count + 1;
	}
}