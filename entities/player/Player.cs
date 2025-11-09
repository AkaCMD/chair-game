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

        Vector2I dest = GridPosition + dir;

        if (IsWall(dest))
        {
            Bump(dest);
            return;
        }

        Box box = GetBox(dest);
        if (box != null)
        {
            Vector2I boxDest = dest + dir;
            if (IsWall(boxDest) || GetBox(boxDest) != null)
            {
                Bump(dest);
                return;
            }
            box.MoveTo(boxDest);
        }
        
        MoveTo(dest);
    }
}
