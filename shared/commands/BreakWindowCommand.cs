using System.Collections.Generic;
using Godot;

public class BreakWindowCommand : IAction
{
    private GlassWindow _window;
    private Mover _breaker;
    private Vector2I _direction;
    private Vector2I _position;
    private Dictionary<Mover, Vector2I> _fallenObjects = new Dictionary<Mover, Vector2I>();
    
    public BreakWindowCommand(GlassWindow window, Mover breaker, Vector2I direction)
    {
        _window = window;
        _breaker = breaker;
        _direction = direction;
        _position = window.GridPosition;
    }
    
    public void ExecuteCommand()
    {
        // Store original positions of objects that will fall
        _fallenObjects.Clear();
        
        // Break the window (this will also trigger falls)
        _window.Break(_breaker, _direction);
        
        // Get the fallen objects from the window
        var fallen = _window.GetFallenObjects();
        foreach (var kvp in fallen)
        {
            _fallenObjects[kvp.Key] = kvp.Value;
        }
    }

    public void UndoCommand()
    {
        // Restore the window
        _window.GridPosition = _position;
        _window.Restore();
        
        // Restore fallen objects to their original positions
        foreach (var kvp in _fallenObjects)
        {
            Mover mover = kvp.Key;
            Vector2I originalPos = kvp.Value;
            
            // Check if the mover still exists
            if (IsInstanceValid(mover))
            {
                mover.GridPosition = originalPos;
            }
        }
        _fallenObjects.Clear();
    }
    
    private bool IsInstanceValid(GodotObject obj)
    {
        return obj != null && GodotObject.IsInstanceValid(obj);
    }
}