using Godot;

public class RotateChairCommand : IAction
{
    private Vector2I _previousDirection;
    private Vector2I _previousPreviousDirection;
    public RotateChairCommand()
    {
        _previousPreviousDirection = Player.Instance.PreviousPreviousDirection;
        _previousDirection = Player.Instance.PreviousDirection;
    }
    public void ExecuteCommand()
    {
        Player.Instance.PreviousPreviousDirection = Player.Instance.PreviousDirection;
        Player.Instance.PreviousDirection = Player.Instance.Direction;
    }

    public void UndoCommand()
    {
        Player.Instance.PreviousDirection = _previousPreviousDirection;
        Player.Instance.Direction = _previousDirection;
    }
}
