using Godot;

public class MoveCommand : IAction
{
    private Vector2I _originGridPosition;
    private Mover _toMoveObject;
    private Vector2I _direction;
    
    public MoveCommand(Mover toMoveObject)
    {
        this._toMoveObject = toMoveObject;
        this._originGridPosition = toMoveObject.GridPosition;
        if (toMoveObject is Player player)
        {
            _direction = player.PreviousDirection;
        }
    }

    public void ExecuteCommand()
    {
        _toMoveObject.MoveIt();
    }
    
    public void UndoCommand()
    {
        _toMoveObject.GridPosition = _originGridPosition;
        if (_toMoveObject is Player player)
        {
            player.Direction = _direction;
        }
    }
}