using Godot;

public class GrabBoxCommand : IAction
{
    private Box _box;
    private Vector2I _boxPos;
    private Vector2I _direction;
    
    public GrabBoxCommand(Box boxToGrab)
    {
        _box = boxToGrab;
        _boxPos = boxToGrab.GridPosition;
        _direction = Player.Instance.PreviousDirection;
    }

    public void ExecuteCommand()
    {
        Player.Instance.HasBox = true;
        _box.GridPosition = new Vector2I(999, 999);
        Player.Instance.BoxInstance = _box;
    }
    
    public void UndoCommand()
    {
        Player.Instance.HasBox = false;
        _box.GridPosition = _boxPos;
        Player.Instance.Direction = _direction;
    }
}