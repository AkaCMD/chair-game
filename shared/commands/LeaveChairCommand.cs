using Godot;
using System;

public partial class LeaveChairCommand : IAction
{
    private Vector2I _chairPosition;
    private Chair _chair;
    private Vector2I _direction;
    private Vector2I _playerDirection;
    private Vector2I _playerPreviousDirection;
    public LeaveChairCommand(Vector2I pos)
    {
        _chairPosition = pos;
        _playerDirection = Player.instance.Direction;
        _playerPreviousDirection = Player.instance.PreviousDirection;
    }
    public void ExecuteCommand()
    {
        _chair = Player.instance.ChairInstance;
        _chair.GridPosition = _chairPosition;
        _chair.Direction = Player.instance.Direction;
        Player.instance.IsSit = false;
        _direction = _chair.Direction;
    }
    
    public void UndoCommand()
    {
        _chair.GridPosition = new Vector2I(999, 999);
        _chair.Direction = _direction;
        Player.instance.ChairInstance = _chair;
        Player.instance.IsSit = true;
        Player.instance.Direction = _playerDirection;
        Player.instance.PreviousDirection = _playerPreviousDirection;
    }
}
