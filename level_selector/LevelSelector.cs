using Godot;
using System;

public partial class LevelSelector : Node2D
{
    [Export]
    private Camera2D _camera;
    private bool _isHover = false;
    public static Action<PackedScene> OnLevelEnter;
    [Export]
    private ColorRect _screenColorRect;
    [Export]
    private Control _hint;
    private bool _isOnLevel = false;
    [Export]
    private CanvasLayer _canvasLayer;
    [Export]
    private Panel _levelSelectorTitle;
    [Export]
    private Node2D _nodes;

    public override void _Ready()
    {
        OnLevelEnter += (packedLevel) =>
        {
            if (!_isOnLevel)
            {
                _isOnLevel = true;
                var tween = GetTree().CreateTween();
                tween.TweenProperty(_screenColorRect, "size", new Vector2(1280, 720), 1f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
                tween.Finished += () =>
                {
                    var timer = GetTree().CreateTimer(1);
                    timer.Timeout += () =>
                    {
                        _nodes.Visible = false;
                        _levelSelectorTitle.Visible = false;
                        _canvasLayer.AddChild(packedLevel.Instantiate());
                        _hint.Visible = true;
                        var tween = GetTree().CreateTween();
                        tween.TweenProperty(_screenColorRect, "size", new Vector2(0, 720), 1f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
                    };
                };
            }
        };
    }


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
