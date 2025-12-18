using Godot;
using System;

public partial class DialogText : Node2D
{
    [Export]
    public Label DialogLabel;
    
    public void Setup(string charText, Vector2 targetPos)
    {
        DialogLabel.Text = charText;
        Position = targetPos;
        
        Scale = Vector2.Zero;
        var tween = GetTree().CreateTween();
        tween.TweenProperty(this, "scale", Vector2.One, .5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        var tween2 = GetTree().CreateTween();
        tween2.TweenProperty(this, "rotation", new Random().Next(-2, 2)/10f, .5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
    }
}
