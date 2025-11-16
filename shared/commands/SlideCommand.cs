using Godot;
using System;

public class SlideCommand : IAction
{
    private Vector2I _previousDirection;
    private Vector2I _position;
    public SlideCommand()
    {
        _previousDirection = Player.Instance.PreviousDirection;
        _position = Player.Instance.GridPosition;
    }
    public void ExecuteCommand()
    {
        Player.Instance.SoundSlide.Stop();
        Player.Instance.SoundSlide.Play();
        Player.Instance.SoundSlide.PitchScale = new Random().Next(-2, 2)/10f + 1;
        for (int j = 0; j < 30; j++)
        {
            if (!Player.Instance.TryPlanMove(Player.Instance.Direction * j))
            {
                Player.Instance.TryPlanMove(Player.Instance.Direction * (j - 1));
                break;
            }
        }
        Player.Instance.PreviousDirection = Player.Instance.Direction;
    }
    
    public void UndoCommand()
    {
        Player.Instance.PreviousDirection = Player.Instance.Direction;
        Player.Instance.Direction = _previousDirection;
        Player.Instance.PreviousDirection = _previousDirection;
        Player.Instance.GridPosition = _position;
    }
}
