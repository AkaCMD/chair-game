using System;
using Godot;

public class DropBoxCommand : IAction
{
    private Box _box;
    private bool _wasPlacedOnChair;
    private Chair _targetChair;

    public DropBoxCommand(Box box)
    {
        _box = box;
        _wasPlacedOnChair = false;
        _targetChair = null;
    }

    public void ExecuteCommand()
    {
        Player.Instance.SoundDropBox.Stop();
        Player.Instance.SoundDropBox.Play();
        Player.Instance.SoundDropBox.PitchScale = new Random().Next(-2, 2)/10f + 1;
        _wasPlacedOnChair = false;
        _targetChair = null;
        
        var placePos = Player.Instance.GridPosition + Player.Instance.Direction;
        bool canPlaceBoxOnChair = Player.Instance.IsChair(placePos, out var chair) &&
                                  (Player.Instance.Direction == -chair.Direction);
        
        if (canPlaceBoxOnChair)
        {
            chair.HasBox = true;
            chair.BoxOnChair = _box;
            _box.GridPosition = new Vector2I(999, 999);
            
            _wasPlacedOnChair = true;
            _targetChair = chair;
        }
        else
        {
            _box.GridPosition = placePos;
            _wasPlacedOnChair = false;
        }
        
        Player.Instance.HasBox = false;
    }

    public void UndoCommand()
    {
        Player.Instance.HasBox = true;
        Player.Instance.BoxInstance = _box;
        _box.GridPosition = new Vector2I(999, 999); 
        
        if (_wasPlacedOnChair)
        {
            if (_targetChair != null)
            {
                _targetChair.HasBox = false;
                _targetChair.BoxOnChair = null;
            }
        }
    }
}