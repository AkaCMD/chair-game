using Godot;
using System;

public class SlideChairCommand : IAction
{
    private Chair _chair;
    private Vector2I _direction;
    private Vector2I _position;
    public SlideChairCommand(Chair chair)
    {
        _chair = chair;
        _direction = chair.Direction;
        _position = chair.GridPosition;
    }
    public void ExecuteCommand()
    {
        for (int j = 0; j < 30; j++)
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
    }
}
