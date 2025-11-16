using Godot;
using System;

public partial class DialogController : Node2D
{
    [Export]
    private PackedScene _packedDialogText;
    [Export]
    public string Text;
    private double _timer;
    private int _index;
    public override void _Process(double delta)
    {
        _timer += delta;
        if (_timer > .1 && _index < Text.Length)
        {
            var word = _packedDialogText.Instantiate<DialogText>();
            word.Position = new Vector2(20 * _index, 0);
            word.DialogLabel.Text = Text.ToCharArray()[_index].ToString();
            AddChild(word);
            _index++;
            _timer = 0;
        }
    }

}
