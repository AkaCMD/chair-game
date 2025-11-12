using Godot;

public class SitChairCommand : IAction
{
    private Chair _chair;
    private Vector2I _originalPosition;
    private Vector2I _direction;
    private Vector2I _playerDirection;
    private Vector2I _playerPreviousDirection;

    public SitChairCommand(Chair chair)
    {
        _chair = chair;
        _originalPosition = _chair.GridPosition;
        _playerDirection = Player.Instance.Direction;
        _playerPreviousDirection = Player.Instance.PreviousDirection;
    }
    public void ExecuteCommand()
    {
        Player.Instance.IsSit = true;
        Player.Instance.ChairInstance = _chair;
        _chair.GridPosition = new Vector2I(999, 999);
        _direction = _chair.Direction;
        for (int j = 0; j < 50; j++)
        {
            if (!Player.Instance.TryPlanMove(Player.Instance.Direction * j))
            {
                Player.Instance.TryPlanMove(Player.Instance.Direction * (j - 1));
                break;
            }
        }
        CommandManager.ExecuteCommand(new MoveCommand(Player.Instance));
    }

    public void UndoCommand()
    {
        _chair.GridPosition = _originalPosition;
        _chair.Direction = _direction;
        Player.Instance.IsSit = false;
        Player.Instance.Direction = _playerDirection;
        Player.Instance.PreviousDirection = _playerPreviousDirection;
    }
}
