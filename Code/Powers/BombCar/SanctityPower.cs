using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Test.Code.Cards.BombCar.Skill;

namespace Test.Code.Powers.BombCar;


public class SanctityPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Sanctity>();
}
