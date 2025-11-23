using Godot;

public class SitChairCommand : IAction
{
    private Chair _chair;
    private Vector2I _originalPosition;
    private Vector2I _playerOriginalGridPosition;
    private Vector2I _direction;
    private Vector2I _playerPreviousDirection;
    private Vector2I _playerPreviousPreviousDirection;
    public SitChairCommand(Chair chair)
    {
        _chair = chair;
        _originalPosition = _chair.GridPosition;
        _playerPreviousPreviousDirection = Player.Instance.PreviousPreviousDirection;
        _playerPreviousDirection = Player.Instance.PreviousDirection;
        _playerOriginalGridPosition = Player.Instance.GridPosition;
    }
    public void ExecuteCommand()
    {
        Player.Instance.SoundSlide.Stop();
        Utils.PlayWithRandomPitch(Player.Instance.SoundSlide);
        
        Player.Instance.PreviousPreviousDirection = Player.Instance.PreviousDirection;
        Player.Instance.PreviousDirection = Player.Instance.Direction;
        Player.Instance.IsSit = true;
        Player.Instance.ChairInstance = _chair;
        _chair.GridPosition = new Vector2I(999, 999);
        _direction = _chair.Direction;
        Player.Instance.GridPosition += Player.Instance.Direction;
    }

    public void UndoCommand()
    {
        _chair.GridPosition = _originalPosition;
        _chair.Direction = _direction;
        Player.Instance.IsSit = false;
        Player.Instance.PreviousDirection = _playerPreviousPreviousDirection;
        Player.Instance.Direction = _playerPreviousDirection;
        Player.Instance.GridPosition = _playerOriginalGridPosition;
    }
}
