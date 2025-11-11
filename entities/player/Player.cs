// Derives from Mover, handles character movement input.

using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Player : Mover
{
    public static Player instance { get; private set; }
    public Vector2I Direction = Vector2I.Zero;

    private int prevHorInput = 0;
    private int prevVerInput = 0;
    
    public List<Vector2I> InputBuffer = new List<Vector2I>();

    public override void _EnterTree()
    {
        instance = this;
    }

    public override void _Process(double delta)
    {
        if (!Game.Instance.holdingUndo)
        {
            BufferInput();
        }

        if (CanInput())
        {
            CheckBufferedInput();
        }
    }

    public bool CanInput()
    {
        return !Game.Instance.IsMoving && !Game.Instance.holdingUndo;
    }

    public void ClearInputBuffer()
    {
        InputBuffer.Clear();
        prevHorInput = 0;
        prevVerInput = 0;
        Direction = Vector2I.Zero;
    }

    public void BufferInput()
    {
        Vector2I newDir =  (Vector2I)Input.GetVector("move_left", 
                "move_right", 
                "move_up", 
                "move_down")
            .Round();
        int newHor = newDir.X;
        int newVer = newDir.Y;
        bool shouldBufferInput = 
            (newHor != prevHorInput || newVer != prevVerInput) && // input is different from last time it was checked
            !((newHor == 0 && newVer == prevVerInput) || (newVer == 0 && newHor == prevHorInput)); // the change isn't just due to releasing a key
        
        Vector2I dir = Vector2I.Zero;

        if (InputBuffer.Count == 0)
        {
            if (shouldBufferInput || CanInput())
            {
                dir = CalculateNewDirFromInput(Direction);
            }
        }
        else
        {
            if (shouldBufferInput)
            {
                dir = CalculateNewDirFromInput(InputBuffer.Last());
            }
        }

        if (dir != Vector2I.Zero)
        {
            InputBuffer.Add(dir);
        }
        
        prevHorInput = newHor;
        prevVerInput = newVer;
    }

    public void CheckBufferedInput()
    {
        if (InputBuffer.Count == 0)
        {
            return;
        }

        Direction = InputBuffer.First();
        InputBuffer.RemoveAt(0);
        
        if (TryPlanMove(Direction))
        {
            Game.Instance.MoveStart();
        }
    }

    public Vector2I CalculateNewDirFromInput(Vector2I currentDir)
    {
        Vector2I dir = (Vector2I)Input.GetVector("move_left", 
                "move_right", 
                "move_up", 
                "move_down")
            .Round();
        if (dir == Vector2I.Zero)
        {
            return Vector2I.Zero;
        }

        int hor = dir.X;
        int ver = dir.Y;

        if (hor != 0 && ver != 0)
        {
            if (currentDir == Vector2I.Right || currentDir == Vector2I.Left)
            {
                hor = 0;
            }
            else
            {
                ver = 0;
            }
        }
        
        if (hor == 1)
        {
            return Vector2I.Right;
        }
        else if (hor == -1)
        {
            return Vector2I.Left;
        }
        else if (ver == 1)
        {
            return Vector2I.Down;
        }
        else if (ver == -1)
        {
            return Vector2I.Up;
        }

        return Vector2I.Zero;
    }
}
