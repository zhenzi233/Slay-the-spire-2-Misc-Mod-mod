using MegaCrit.Sts2.Core.Models.Cards;
using Test.Code.Cards.Rare;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class EnergyExploseFocusPower : TemporaryFocusPower
{
	public override AbstractModel OriginModel => ModelDb.Card<EnergyExplose>();
}
