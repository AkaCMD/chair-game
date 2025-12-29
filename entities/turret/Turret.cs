using Godot;

public partial class Turret : Mover
{
    protected override void PlanMove(Vector2I dir)
    {
        IsSliding = true;
        base.PlanMove(dir);
    }
}
