using System;
using Godot;

public class GrabBoxCommand : IAction
{
    private Box _box;
    private Vector2I _originalBoxPos;
    private Vector2I _originalPlayerDirection;
    
    private Chair _sourceChair;
    private bool _wasGrabbedFromChair;
    
    public GrabBoxCommand(Box boxToGrab, Chair sourceChair)
    {
        _box = boxToGrab;
        
        _originalPlayerDirection = Player.Instance.PreviousDirection;
        _originalBoxPos = boxToGrab.GridPosition;
        
        _sourceChair = sourceChair;
        _wasGrabbedFromChair = (_sourceChair != null);
    }

    public void ExecuteCommand()
    {
        Player.Instance.SoundTakeBox.Stop();
        Player.Instance.SoundTakeBox.Play();
        Player.Instance.SoundTakeBox.PitchScale = new Random().Next(-2, 2)/10f + 1;
        Player.Instance.HasBox = true;
        Player.Instance.BoxInstance = _box;
        _box.GridPosition = new Vector2I(999, 999);
        
        if (_wasGrabbedFromChair)
        {
            _sourceChair.HasBox = false;
        }
    }
    
    public void UndoCommand()
    {
        Player.Instance.HasBox = false;
        Player.Instance.BoxInstance = null;
        Player.Instance.Direction = _originalPlayerDirection;
        
        _box.GridPosition = _originalBoxPos;
        
        if (_wasGrabbedFromChair)
        {
            _sourceChair.HasBox = true;
        }
    }
}