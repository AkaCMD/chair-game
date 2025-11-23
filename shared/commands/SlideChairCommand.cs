using Godot;

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
        Utils.PlayWithRandomPitch(Player.Instance.SoundSlide);
        for (int j = 0; j < GameConstants.MaxMovementCycles; j++)
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
