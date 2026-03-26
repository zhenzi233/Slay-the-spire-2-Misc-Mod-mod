using MegaCrit.Sts2.Core.Models.Cards;
using Test.Code.Cards.Rare;
using Test.Code.Cards.Rare.Power;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class EnergyExploseFocusPower : TemporaryFocusPower
{
	public override AbstractModel OriginModel => ModelDb.Card<EnergyExplose>();
}
