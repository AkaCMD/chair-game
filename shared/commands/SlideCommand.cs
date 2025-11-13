using Godot;
using System;

public class SlideCommand : IAction
{
    private Vector2I _direction;
    private Vector2I _position;
    public SlideCommand()
    {
        _direction = Player.Instance.Direction;
        _position = Player.Instance.GridPosition;
    }
    public void ExecuteCommand()
    {
        for (int j = 0; j < 30; j++)
        {
            if (!Player.Instance.TryPlanMove(Player.Instance.Direction * j))
            {
                Player.Instance.TryPlanMove(Player.Instance.Direction * (j - 1));
                break;
            }
        }
        Player.Instance.PreviousDirection = _direction;
    }
    
    public void UndoCommand()
    {
        Player.Instance.Direction = _direction;
        Player.Instance.GridPosition = _position;
    }
}
