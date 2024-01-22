using Godot;

[GlobalClass]
public partial class FactionResource : Resource
{
    [Export]
    public string? Name { get; set; }
}
