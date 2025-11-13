// Derives from Mover, handles character movement input.

using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Player : Mover
{
    [Export]
    private Sprite2D _sprite;
    [Export]
    private Texture2D _texturePlayerLeft;
    [Export]
    private Texture2D _texturePlayerRight;
    [Export]
    private Texture2D _texturePlayerUp;
    [Export]
    private Texture2D _texturePlayerDown;
    [Export]
    private Texture2D _textureLeft;
    [Export]
    private Texture2D _textureRight;
    [Export]
    private Texture2D _textureUp;
    [Export]
    private Texture2D _textureDown;
    [Export]
    private Label _label;
    public Chair ChairInstance;
    public Vector2I Direction = Vector2I.Zero;
    public Vector2I PreviousDirection = Vector2I.Zero;
    private int prevHorInput = 0;
    private int prevVerInput = 0;

    public bool IsSit { get; set; } = false;
    public bool IsPreviousSit { get; set; } = false;
    
    [Export] private Texture2D _texturePlayerWithBoxLeft;
    [Export] private Texture2D _texturePlayerWithBoxRight;
    [Export] private Texture2D _texturePlayerWithBoxUp;
    [Export] private Texture2D _texturePlayerWithBoxDown;
    
    public static Player Instance { get; private set; }

    private int _prevHorInput = 0;
    private int _prevVerInput = 0;

    // public bool IsSit { get; set; } = false;
    public bool HasBox { get; set; } = false;

    public List<Vector2I> InputBuffer = new List<Vector2I>();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        _label.Text = $"Direction = {Direction}\nPrevious: {PreviousDirection}";
        if (!Game.Instance.holdingUndo)
        {
            BufferInput();
        }

        if (CanInput())
        {
            // check movement input
            CheckBufferedInput();
            
            // interact
            if (Input.IsActionJustPressed("interact") && !HasBox)
            {
                var mover = GetMover(GridPosition + Direction);
                if (mover != null && mover.IsInGroup("boxes"))
                {
                    CommandManager.ExecuteCommand(new GrabBoxCommand((Box) mover));
                }
            }
        }

        if (IsSit)
        {
            _sprite.Texture = Direction == Vector2I.Left ? _textureLeft : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Right ? _textureRight : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Up ? _textureUp : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Down ? _textureDown : _sprite.Texture;
        }
        else if (HasBox)
        {
            _sprite.Texture = Direction == Vector2I.Left ? _texturePlayerWithBoxLeft : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Right ?  _texturePlayerWithBoxRight : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Up ? _texturePlayerWithBoxUp : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Down ? _texturePlayerWithBoxDown : _sprite.Texture;
        }
        else
        {
            _sprite.Texture = Direction == Vector2I.Left ? _texturePlayerLeft : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Right ? _texturePlayerRight : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Up ? _texturePlayerUp : _sprite.Texture;
            _sprite.Texture = Direction == Vector2I.Down ? _texturePlayerDown : _sprite.Texture;
        }
    }

    public bool CanInput()
    {
        return !Game.Instance.IsMoving && !Game.Instance.holdingUndo;
    }

    public void ClearInputBuffer()
    {
        InputBuffer.Clear();
        _prevHorInput = 0;
        _prevVerInput = 0;
        Direction = Vector2I.Zero;
    }

    public void BufferInput()
    {
        Vector2I newDir = (Vector2I)Input.GetVector("move_left",
                "move_right",
                "move_up",
                "move_down")
            .Round();
        int newHor = newDir.X;
        int newVer = newDir.Y;
        bool shouldBufferInput =
            (newHor != _prevHorInput || newVer != _prevVerInput) && // input is different from last time it was checked
            !((newHor == 0 && newVer == _prevVerInput) || (newVer == 0 && newHor == _prevHorInput)); // the change isn't just due to releasing a key

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

        _prevHorInput = newHor;
        _prevVerInput = newVer;
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
        else
        {
            CommandManager.ExecuteCommand(new RotateChairCommand(Direction));
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
