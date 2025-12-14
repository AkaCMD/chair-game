using Godot;

public partial class Trashbin : Box, IInteractable
{
    [Export] private Label _hintLabel;

    public override void _Ready()
    {
        base._Ready();
        if (_hintLabel == null)
        {
            _hintLabel = GetNode<Label>("HintLabel");
        }

        HideHint();
    }

    public void Interact()
    {
        // Show dialogue
    }

    public void ShowHint()
    {
        if (_hintLabel != null)
        {
            _hintLabel.Visible = true;
        }
    }

    public void HideHint()
    {
        if (_hintLabel != null)
        {
            _hintLabel.Visible = false;
        }
    }
}
