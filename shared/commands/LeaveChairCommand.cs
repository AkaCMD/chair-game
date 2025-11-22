using Godot;
using System;

public class LeaveChairCommand : IAction
{
    private Vector2I _chairPosition;
    private Chair _chair;
    private Vector2I _direction;
    private Vector2I _playerPreviousDirection;
    private Vector2I _playerPreviousPreviousDirection;
    public LeaveChairCommand(Vector2I pos)
    {
        _chairPosition = pos;
        _playerPreviousDirection = Player.Instance.PreviousDirection;
        _playerPreviousPreviousDirection = Player.Instance.PreviousPreviousDirection;
    }
    public void ExecuteCommand()
    {
        Player.Instance.SoundLeaveChair.Stop();
        Player.Instance.SoundLeaveChair.Play();
        Player.Instance.SoundLeaveChair.PitchScale = new Random().Next(-2, 2)/10f + 1;
        
        _chair = Player.Instance.ChairInstance;
        _chair.GridPosition = _chairPosition;
        _chair.Direction = Player.Instance.Direction;
        Player.Instance.IsSit = false;
        Player.Instance.PreviousPreviousDirection = Player.Instance.PreviousDirection;
        Player.Instance.PreviousDirection = Player.Instance.Direction;
        _direction = _chair.Direction;
    }
    
    public void UndoCommand()
    {
        _chair.GridPosition = new Vector2I(999, 999);
        _chair.Direction = _direction;
        Player.Instance.ChairInstance = _chair;
        Player.Instance.IsSit = true;
        Player.Instance.PreviousDirection = _playerPreviousPreviousDirection;
        Player.Instance.Direction = _playerPreviousDirection;
    }
}
