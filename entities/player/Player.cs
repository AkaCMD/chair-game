// Derives from Mover, handles character movement input.

using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Player : Mover
{
    [Export] private Sprite2D _sprite;

    [Export] private Texture2D _texturePlayerLeft;
    [Export] private Texture2D _texturePlayerRight;
    [Export] private Texture2D _texturePlayerUp;
    [Export] private Texture2D _texturePlayerDown;

    [Export] private Texture2D _textureLeft;
    [Export] private Texture2D _textureRight;
    [Export] private Texture2D _textureUp;
    [Export] private Texture2D _textureDown;

    public Chair ChairInstance;
    public Vector2I Direction = Vector2I.Right;
    public Vector2I PreviousDirection = Vector2I.Right;
    public Vector2I PreviousPreviousDirection = Vector2I.Right;
    private int _prevHorInput = 0;
    private int _prevVerInput = 0;

    public bool IsSit { get; set; } = false;
    public bool IsPreviousSit { get; set; } = false;

    [Export] private Texture2D _texturePlayerWithBoxLeft;
    [Export] private Texture2D _texturePlayerWithBoxRight;
    [Export] private Texture2D _texturePlayerWithBoxUp;
    [Export] private Texture2D _texturePlayerWithBoxDown;

    public static Player Instance { get; private set; }

    public bool HasBox { get; set; } = false;
    public Box BoxInstance { get; set; } = null;

    public List<Vector2I> InputBuffer = new List<Vector2I>();

    public bool IsWaiting = false;

    private bool _waitForInputRelease = false;

    [Export] public AudioStreamPlayer SoundUndo;
    [Export] public AudioStreamPlayer SoundWalk;
    [Export] public AudioStreamPlayer SoundTakeBox;
    [Export] public AudioStreamPlayer SoundSlide;
    [Export] public AudioStreamPlayer SoundLeaveChair;
    [Export] public AudioStreamPlayer SoundDropBox;
    [Export] public AudioStreamPlayer SoundCrush;
    [Export] public AudioStreamPlayer SoundCollide;
    [Export] public AudioStreamPlayer SoundBreak;

    public override void _EnterTree()
    {
        Instance = this;
        AddToGroup("player");
    }

    public override void _Process(double delta)
    {
        if (IsWaiting)
        {
            return;
        }
        // Back to level selector without beat the level
        if (Input.IsActionJustPressed("escape"))
        {
            LevelSelector.OnLevelExit(false);
        }
        if (!Game.Instance.IsHoldingUndo)
        {
            BufferInput();
        }

        if (CanInput())
        {
            // check movement input
            CheckBufferedInput();

            // interact
            if (Input.IsActionJustPressed("interact"))
            {
                HandleAction();
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
            _sprite.Texture = Direction == Vector2I.Right ? _texturePlayerWithBoxRight : _sprite.Texture;
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
        return !Game.Instance.IsMoving && !Game.Instance.IsHoldingUndo;
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
        if (Game.Instance.IsReplaying) return;
        Vector2I newDir = (Vector2I)Input.GetVector("move_left",
                "move_right",
                "move_up",
                "move_down")
            .Round();

        if (_waitForInputRelease)
        {
            if (newDir == Vector2I.Zero)
            {
                _waitForInputRelease = false;
            }
            else
            {
                _prevHorInput = newDir.X;
                _prevVerInput = newDir.Y;
                return;
            }
        }

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
        var isValidMove = true;
        if (InputBuffer.Count == 0)
        {
            return;
        }
        Vector2I newDirection = InputBuffer.First();
        InputBuffer.RemoveAt(0);

        if (IsSit)
        {
            if (IsSliding) return;
            bool isSameDirection = (newDirection == Direction);
            if (isSameDirection)
            {
                Vector2I checkPos = GridPosition + Direction;
                bool hasObstacle = IsWall(checkPos) || GetMover(checkPos) != null;
                if (hasObstacle)
                {
                    IsSliding = true;
                    if (TryPlanMove(-Direction))
                    {
                        Bump(checkPos, true);
                    }
                    else
                    {
                        IsSliding = false;
                        isValidMove = false;
                    }
                }
                else
                {
                    CommandManager.ExecuteCommand(new LeaveChairCommand(GridPosition));
                }
            }
            else
            {
                Direction = newDirection;
                Utils.PlayWithRandomPitch(SoundCollide);
                CommandManager.ExecuteCommand(new RotateChairCommand());
            }
        }
        else
        {
            Direction = newDirection;
            Vector2I checkPos = GridPosition + Direction;
            Mover mover = GetMover(checkPos);

            if (mover is Chair chair)
            {
                if (Direction == -chair.Direction && !chair.HasBox && !HasBox)
                {
                    CommandManager.ExecuteCommand(new SitChairCommand(chair));
                    CommandManager.AddNewTurn();
                    InputBuffer.Clear();
                    _waitForInputRelease = true;
                    
                    Game.Instance.StepHistory.Add(Step.CreateMove(newDirection));
                    PrintSolutionSequence();
                    return;
                }
            }

            if (TryPlanMove(Direction))
            {
                Game.Instance.MoveStart();
            }
            else
            {
                Utils.PlayWithRandomPitch(SoundCollide);
                if (Direction == PreviousDirection)
                {
                    isValidMove = false;
                }
                else
                {
                    PreviousDirection = Direction;
                }
            }
        }

        if (isValidMove)
        {
            Game.Instance.StepHistory.Add(Step.CreateMove(newDirection));
            PrintSolutionSequence();
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

    public void InjectMoveInput(Vector2I direction)
    {
        InputBuffer.Add(direction);
    }

    public void InjectActionInput()
    {
        HandleAction();
    }

    private void HandleAction()
    {
        if (!IsSit)
        {
            Game.Instance.StepHistory.Add(Step.CreateAction());
            PrintSolutionSequence();
            if (HasBox)
            {
                var mover = GetMover(GridPosition + Direction);
                if ((mover == null && !IsWall(GridPosition + Direction)) ||
                    (mover is Chair chair && Direction == -chair.Direction))
                {
                    CommandManager.ExecuteCommand(new DropBoxCommand(BoxInstance));
                    CommandManager.AddNewTurn();
                }
            }
            else
            {
                var targetPos = GridPosition + Direction;
                var mover = GetMover(targetPos);

                if (mover != null && mover.IsInGroup("boxes"))
                {
                    CommandManager.ExecuteCommand(new GrabBoxCommand((Box)mover, null));
                    CommandManager.AddNewTurn();
                }
                else if (IsChair(targetPos, out Chair chair))
                {
                    bool canGrabFromChair = chair != null && chair.HasBox && (Direction == -chair.Direction);
                    if (canGrabFromChair)
                    {
                        Box boxOnChair = chair.BoxOnChair;

                        if (boxOnChair != null)
                        {
                            CommandManager.ExecuteCommand(new GrabBoxCommand(boxOnChair, chair));
                            CommandManager.AddNewTurn();
                        }
                    }
                }
            }
        }
    }

    public void PrintSolutionSequence()
    {
        if (Game.Instance.StepHistory.Count == 0)
        {
            //GD.Print("当前解法序列: []");
            return;
        }

        var sequence = new List<string>();
        foreach (var step in Game.Instance.StepHistory)
        {
            switch (step.Type)
            {
                case StepType.Move:
                    string direction = GetDirectionSymbol(step.DirX, step.DirY);
                    sequence.Add(direction);
                    break;
                case StepType.Action:
                    sequence.Add("A");
                    break;
            }
        }

        //GD.Print($"当前解法序列: [{string.Join(", ", sequence)}]");
    }

    private string GetDirectionSymbol(int dirX, int dirY)
    {
        if (dirX == 1) return "→";
        if (dirX == -1) return "←";
        if (dirY == 1) return "↓";
        if (dirY == -1) return "↑";
        return "?";
    }
}
