using Godot;

public partial class DraggableControl : Control
{
    private bool _isDragging = false;
    private Vector2 _dragOffset;

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    _isDragging = true;
                    _dragOffset = GlobalPosition - GetGlobalMousePosition();
                }
                else
                {
                    _isDragging = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            if (_isDragging)
            {
                GlobalPosition = GetGlobalMousePosition() + _dragOffset;
            }
        }
    }
}