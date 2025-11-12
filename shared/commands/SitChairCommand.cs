using Godot;
using System;

public partial class SitChairCommand : IAction
{
    private Chair _chair;
    private Vector2I _originalPosition;

    public SitChairCommand(Chair chair)
    {
        _chair = chair;
        _originalPosition = _chair.GridPosition;
    }
    public void ExecuteCommand()
    {
        Player.instance.IsSit = true;
        _chair.GridPosition = new Vector2I(999, 999);
    }

    public void UndoCommand()
    {
        _chair.GridPosition = _originalPosition;
        Player.instance.IsSit = false;
    }
}
