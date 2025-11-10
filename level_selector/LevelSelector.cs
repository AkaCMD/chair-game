using Godot;
using System;

public partial class LevelSelector : Node2D
{
    [Export]
    private Camera2D _camera;
    private bool _isHover = false;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion inputEventMouseMotion)
        {
            _camera.Offset -= _isHover ? new Vector2(inputEventMouseMotion.Relative.X, 0) : Vector2.Zero;
            _camera.Offset = new Vector2(Mathf.Max(0, Mathf.Min(1000, _camera.Offset.X)), 0);

        }
        if (@event is InputEventMouseButton inputEventMouseButton)
        {
            _isHover = inputEventMouseButton.IsPressed();
        }
    }


}
