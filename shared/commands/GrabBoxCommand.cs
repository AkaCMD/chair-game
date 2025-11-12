using Godot;

public class GrabBoxCommand : IAction
{
    private Box _box;
    private Vector2I _boxPos;
    private Vector2I _direction;
    private Vector2I _previousDirection;
    
    public GrabBoxCommand(Box boxToGrab)
    {
        _box = boxToGrab;
        _boxPos = boxToGrab.GridPosition;
        _direction = Player.Instance.Direction;
        _previousDirection = Player.Instance.PreviousDirection;
    }

    public void ExecuteCommand()
    {
        Player.Instance.HasBox = true;
        _box.GridPosition = new Vector2I(999, 999);
    }
    
    public void UndoCommand()
    {
        Player.Instance.HasBox = false;
        _box.GridPosition = _boxPos;
        Player.Instance.Direction = _direction;
        Player.Instance.PreviousDirection = _previousDirection;
    }
}