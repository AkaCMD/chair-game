using System.Collections.Generic;
using Godot;

public partial class Interactable : Box, IInteractable
{
    [Export] private Label _hintLabel;
    [Export] private DialogResource _checkDialog;

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
        if (_checkDialog != null)
            DialogController.Instance.StartDialog(new List<DialogResource> { _checkDialog });
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
