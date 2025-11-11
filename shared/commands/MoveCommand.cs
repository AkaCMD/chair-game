using Godot;

public class MoveCommand : IAction
{
    private Vector2I _originGridPosition;
    private Mover _toMoveObject;
    
    public MoveCommand(Mover toMoveObject)
    {
        this._toMoveObject = toMoveObject;
        this._originGridPosition = toMoveObject.GridPosition;
    }

    public void ExecuteCommand()
    {
        _toMoveObject.MoveIt();
    }
    
    public void UndoCommand()
    {
        _toMoveObject.GridPosition = _originGridPosition;
    }
}