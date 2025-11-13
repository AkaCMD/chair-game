using Godot;
using System;

public partial class LevelNode : Node2D
{
    [Export]
    private Area2D _area;
    [Export]
    private Sprite2D _sprite;
    [Export]
    private Label _numberUI;

    [Export]
    private Label _nameUI;
    [Export]
    private Panel _panelUI;
    [Export]
    private Panel _hoverUI;
    [Export]
    public string LevelID;
    [Export]
    public string LevelName;
    [Export]
    public LevelNode[] LevelNodes;
    [Export]
    public string LevelPath;
    private bool _isHover = false;

    public override void _Ready()
    {
        QueueRedraw();
        // _panelUI.Scale = Vector2.Zero;
        _numberUI.Text = LevelID;
        _nameUI.Text = LevelName;
        _hoverUI.Scale = new Vector2(0, 1);
        _area.MouseEntered += () =>
        {
            var tween = GetTree().CreateTween();
            var tween2 = GetTree().CreateTween();
            tween.TweenProperty(_sprite, "scale", Vector2.One, .8).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween2.TweenProperty(_hoverUI, "scale", Vector2.One, .8).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            _isHover = true;
        };
        _area.MouseExited += () =>
        {
            var tween = GetTree().CreateTween();
            var tween2 = GetTree().CreateTween();
            tween.TweenProperty(_sprite, "scale", new Vector2(.6f, .6f), .8).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
            tween2.TweenProperty(_hoverUI, "scale", new Vector2(0, 1), .8).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
            _isHover = false;
        };
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton inputEventMouseButton && _isHover)
        {
            LevelSelector.OnLevelEnter.Invoke(LevelPath);
        }
    }


    public override void _Draw()
    {
        foreach (var item in LevelNodes)
        {
            DrawLine(GlobalPosition - Position, item.GlobalPosition - Position, Color.Color8(255, 255, 255), 5);
        }
    }


}
