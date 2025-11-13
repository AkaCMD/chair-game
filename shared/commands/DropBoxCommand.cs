using Godot;
using System;

public class DropBoxCommand : IAction
{
    private Box _box;
    private Vector2I _position;
    public DropBoxCommand(Box box)
    {
        _box = box;
    }

    public void ExecuteCommand()
    {
        _position = _box.GridPosition;
        _box.GridPosition = Player.Instance.GridPosition + Player.Instance.Direction;
        Player.Instance.HasBox = false;
    }

    public void UndoCommand()
    {
        _box.GridPosition = new Vector2I(999, 999);
        Player.Instance.BoxInstance = _box;
        Player.Instance.HasBox = true;
    }
}
