using Godot;

public partial class HintItem : Control
{
    [Export]
    private Label _label;
    [Export]
    private string _labelText;

    public override void _Ready()
    {
        _label.Text = _labelText;
    }
}
