using Godot;

public partial class Box : Mover
{
    public override bool CanMoveToward(Vector2I dir)
    {
        return false;
    }
}
