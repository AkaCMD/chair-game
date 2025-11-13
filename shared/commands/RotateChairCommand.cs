using Godot;
using System;

public partial class RotateChairCommand : IAction
{
    private Vector2I _previousDirection;
    public RotateChairCommand(Vector2I dir)
    {
        _previousDirection = dir;
    }
    public void ExecuteCommand()
    {
        Player.Instance.PreviousDirection = _previousDirection;
    }
    
    public void UndoCommand()
    {
        Player.Instance.Direction = _previousDirection;
    }
}
