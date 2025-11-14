using Godot;
using System;

public class SlideCommand : IAction
{
    private Vector2I _direction;
    private Vector2I _startPosition;
    public SlideCommand()
    {
        _direction = -Player.Instance.Direction;
        _startPosition = Player.Instance.GridPosition;
    }
    public void ExecuteCommand()
    {
        var maxSlide = 30;
        for (int i = 1; i <= maxSlide; i++)
        {
            var dir = _direction * i;
            if (!Player.Instance.TryPlanMove(dir))
            {
                break;
            }
        }
        Player.Instance.PreviousDirection = _direction;
    }
    
    public void UndoCommand()
    {
        Player.Instance.Direction = _direction;
        Player.Instance.GridPosition = _startPosition;
    }
}
