using Godot;

public class BreakObstacleCommand : IAction
{
    private Obstacle _obstacle;
    private Vector2I _position;

    public BreakObstacleCommand(Obstacle obs)
    {
        _obstacle = obs;
        _position = obs.GridPosition;
    }
    
    public void ExecuteCommand()
    {
        _obstacle.Break();
    }

    public void UndoCommand()
    {
        _obstacle.GridPosition = _position;
        _obstacle.Restore();
    }
}