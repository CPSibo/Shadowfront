using Godot;

[GlobalClass]
public partial class UnitResource : Resource
{
    [Export]
    public string UnitTokenScene { get; set; }

    [Export]
    public float Damage { get; set; }

    [Export]
    public float MaxHealth { get; set; }

    [Export]
    public int MaxMoveDistance { get; set; }

    [Export]
    public int MaxAttackDistance { get; set; }

    public UnitResource()
    {
    }
}