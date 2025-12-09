using Godot;

public class SlideChairCommand : IAction
{
    private Chair _chair;
    private Vector2I _direction;
    private Vector2I _position;
    private Vector2I _playerPreviousDirection;
    private Vector2I _playerPreviousPreviousDirection;
    private bool _wasSuccessful;

    public SlideChairCommand(Chair chair)
    {
        _chair = chair;
        _direction = chair.Direction;
        _position = chair.GridPosition;
        _playerPreviousPreviousDirection = Player.Instance.PreviousPreviousDirection;
        _playerPreviousDirection = Player.Instance.PreviousDirection;
        _wasSuccessful = false;
    }
    public void ExecuteCommand()
    {
        if (_chair.TryPlanMove(_chair.Direction))
        {
            Player.Instance.SoundSlide.Stop();
            Utils.PlayWithRandomPitch(Player.Instance.SoundSlide);
            _chair.MoveIt();
            _wasSuccessful = true;
        }
        else
        {
            _wasSuccessful = false;
        }
    }

    public void UndoCommand()
    {
        if (_wasSuccessful)
        {
            _chair.Direction = _direction;
            _chair.GridPosition = _position;
            Player.Instance.PreviousDirection = _playerPreviousPreviousDirection;
            Player.Instance.Direction = _playerPreviousDirection;
        }
    }
}
