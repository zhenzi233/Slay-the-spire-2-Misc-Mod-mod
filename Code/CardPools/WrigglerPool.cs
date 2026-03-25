using BaseLib.Abstracts;
using Godot;

namespace Test.Code.CardPools;

public partial class WrigglerPool : CustomCardPoolModel
{
    public override string Title => "wriggler";

    public override string EnergyColorName => "wriggler";

    public override float H => 0.95f;
    public override float S => 0.98f;
    public override float V => 0.7f;

    public override Color DeckEntryCardColor => new("681760");
    public override Color EnergyOutlineColor => new("651565");

    public override bool IsColorless => false;
}