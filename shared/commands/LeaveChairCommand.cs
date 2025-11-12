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
        _playerDirection = Player.Instance.Direction;
        _playerPreviousDirection = Player.Instance.PreviousDirection;
    }
    public void ExecuteCommand()
    {
        _chair = Player.Instance.ChairInstance;
        _chair.GridPosition = _chairPosition;
        _chair.Direction = Player.Instance.Direction;
        Player.Instance.IsSit = false;
        _direction = _chair.Direction;
    }
    
    public void UndoCommand()
    {
        _chair.GridPosition = new Vector2I(999, 999);
        _chair.Direction = _direction;
        Player.Instance.ChairInstance = _chair;
        Player.Instance.IsSit = true;
        Player.Instance.Direction = _playerDirection;
        Player.Instance.PreviousDirection = _playerPreviousDirection;
    }
}
