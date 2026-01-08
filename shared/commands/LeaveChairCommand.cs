using Godot;
using System;

public class LeaveChairCommand : IAction
{
    private Vector2I _chairPosition;
    private Chair _chair;
    private Vector2I _direction;
    private Vector2I _playerPreviousDirection;
    private Vector2I _playerPreviousPreviousDirection;
    private bool _hasSetupMoveCompleteHandler = false;

    public LeaveChairCommand(Vector2I pos)
    {
        _chairPosition = pos;
        _playerPreviousDirection = Player.Instance.PreviousDirection;
        _playerPreviousPreviousDirection = Player.Instance.PreviousPreviousDirection;
    }
    public void ExecuteCommand()
    {
        Player.Instance.SoundLeaveChair.Stop();
        Utils.PlayWithRandomPitch(Player.Instance.SoundLeaveChair);

        _chair = Player.Instance.ChairInstance;
        _chair.GridPosition = _chairPosition;
        _chair.Direction = Player.Instance.Direction;

        // Mark player as leaving chair before setting IsSit to false
        Player.Instance.IsLeavingChair = true;
        Player.Instance.IsSit = false;

        Player.Instance.PreviousPreviousDirection = Player.Instance.PreviousDirection;
        Player.Instance.PreviousDirection = Player.Instance.Direction;
        _direction = _chair.Direction;

        // Set up handler to clear IsLeavingChair flag after move completes
        // The flag stays true during MoveComplete, protecting player from damage during chair exit
        // It gets cleared after all MoveComplete handlers run
        if (!_hasSetupMoveCompleteHandler)
        {
            GameEventSignals.Instance.MoveComplete += OnLeaveChairComplete;
            _hasSetupMoveCompleteHandler = true;
        }
    }

    private void OnLeaveChairComplete()
    {
        // Use CallDeferred to ensure flag is cleared after all MoveComplete handlers complete
        // This guarantees IsLeavingChair remains true during DamageArea check
        // DamageArea's _Process will detect the flag change and perform delayed check
        Callable.From(() =>
        {
            if (Player.Instance != null)
            {
                Player.Instance.IsLeavingChair = false;
            }
        }).CallDeferred();

        // Clean up event listener
        GameEventSignals.Instance.MoveComplete -= OnLeaveChairComplete;
        _hasSetupMoveCompleteHandler = false;
    }

    public void UndoCommand()
    {
        // Clean up event listener if it was set up
        if (_hasSetupMoveCompleteHandler)
        {
            GameEventSignals.Instance.MoveComplete -= OnLeaveChairComplete;
            _hasSetupMoveCompleteHandler = false;
        }

        // Clear the leaving chair flag
        Player.Instance.IsLeavingChair = false;

        _chair.GridPosition = new Vector2I(999, 999);
        _chair.Direction = _direction;
        Player.Instance.ChairInstance = _chair;
        Player.Instance.IsSit = true;
        Player.Instance.PreviousDirection = _playerPreviousPreviousDirection;
        Player.Instance.Direction = _playerPreviousDirection;
    }
}
