using Godot;
using System;

public class SlideChairCommand : IAction
{
    private Chair _chair;
    private Vector2I _direction;
    private Vector2I _position;
    private Vector2I _playerPreviousDirection;
    private Vector2I _playerPreviousPreviousDirection;
    public SlideChairCommand(Chair chair)
    {
        _chair = chair;
        _direction = chair.Direction;
        _position = chair.GridPosition;
        _playerPreviousPreviousDirection = Player.Instance.PreviousPreviousDirection;
        _playerPreviousDirection = Player.Instance.PreviousDirection;
    }
    public void ExecuteCommand()
    {
        Player.Instance.SoundSlide.Stop();
        Player.Instance.SoundSlide.Play();
        Player.Instance.SoundSlide.PitchScale = new Random().Next(-2, 2)/10f + 1;
        for (int j = 0; j < 30; j++)
        {
            if (!_chair.TryPlanMove(_chair.Direction * j) && j != 0)
            {
                _chair.TryPlanMove(_chair.Direction * (j - 1));
                _chair.MoveIt();
                break;
            }
        }
    }

    public void UndoCommand()
    {
        _chair.Direction = _direction;
        _chair.GridPosition = _position;
        Player.Instance.PreviousDirection = _playerPreviousPreviousDirection;
        Player.Instance.Direction = _playerPreviousDirection;
    }
}
