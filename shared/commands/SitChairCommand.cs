using Godot;
using System;

public partial class SitChairCommand : IAction
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
        _playerDirection = Player.instance.Direction;
        _playerPreviousDirection = Player.instance.PreviousDirection;
    }
    public void ExecuteCommand()
    {
        Player.instance.IsSit = true;
        Player.instance.ChairInstance = _chair;
        _chair.GridPosition = new Vector2I(999, 999);
        _direction = _chair.Direction;
        for (int j = 0; j < 50; j++)
        {
            if (!Player.instance.TryPlanMove(Player.instance.Direction * j))
            {
                Player.instance.TryPlanMove(Player.instance.Direction * (j - 1));
                break;
            }
        }
        CommandManager.ExecuteCommand(new MoveCommand(Player.instance));
    }

    public void UndoCommand()
    {
        _chair.GridPosition = _originalPosition;
        _chair.Direction = _direction;
        Player.instance.IsSit = false;
        Player.instance.Direction = _playerDirection;
        Player.instance.PreviousDirection = _playerPreviousDirection;
    }
}
