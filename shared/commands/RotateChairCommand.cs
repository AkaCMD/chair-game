using Godot;

public class RotateChairCommand : IAction
{
    private Vector2I _dir;
    private Vector2I _prevDir;
    public RotateChairCommand(Vector2I dir)
    {
        _prevDir = Player.Instance.Direction;
        _dir = dir;
    }
    public void ExecuteCommand()
    {
        Player.Instance.PreviousDirection = Player.Instance.Direction;
        Player.Instance.Direction = _dir;
    }
    
    public void UndoCommand()
    {
        Player.Instance.PreviousDirection = Player.Instance.Direction;
        Player.Instance.Direction = _prevDir;
    }
}