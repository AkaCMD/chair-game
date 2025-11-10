// Derives from Mover, handles character movement input.

using Godot;

public partial class Player : Mover
{
    public override void _Process(double delta)
    {
        if (Tween != null && Tween.IsRunning())
        {
            return;
        }

        if (!CanInput()) return;
        
        Vector2I dir = (Vector2I)Input.GetVector("move_left", 
            "move_right", 
            "move_up", 
            "move_down")
            .Round();
        if (dir == Vector2I.Zero)
        {
            return;
        }

        if (dir.X != 0)
        {
            dir.Y = 0;
        }

        if (TryPlanMove(dir))
        {
            Game.Instance.MoveStart();
        }
    }

    public bool CanInput()
    {
        return !Game.Instance.IsMoving && !Game.Instance.holdingUndo;
    }
}
