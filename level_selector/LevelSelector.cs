using Godot;
using System;
using System.IO;

public partial class LevelSelector : Node2D
{
    [Export]
    private Camera2D _camera;
    private bool _isHover = false;
    public static Action<PackedScene> OnLevelEnter;
    public static Action<bool> OnLevelExit;
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
    [Export]
    private Sprite2D _breakSprite;
    private Node2D _currentLevel;
    public static LevelSelector Instance { get; private set; }
    [Export]
    private AudioStreamPlayer _soundLevelStart;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        OnLevelEnter += packedLevel =>
        {
            Game.Instance.CurrentLevelName = Path.GetFileNameWithoutExtension(packedLevel.ResourcePath);
            if (!_isOnLevel)
            {
                _soundLevelStart.PitchScale = 1;
                _soundLevelStart.Play();
                _isOnLevel = true;
                var tween = GetTree().CreateTween();
                tween.TweenProperty(_screenColorRect, "size", new Vector2(1280, 720), 1f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
                tween.Finished += () =>
                {
                    _soundLevelStart.PitchScale = .8f;
                    _soundLevelStart.Play();
                    var timer = GetTree().CreateTimer(1);
                    timer.Timeout += () =>
                    {
                        _nodes.Visible = false;
                        _levelSelectorTitle.Visible = false;
                        _currentLevel = (Node2D)packedLevel.Instantiate();
                        _canvasLayer.AddChild(_currentLevel);
                        _hint.Visible = true;
                        var tween = GetTree().CreateTween();
                        tween.TweenProperty(_screenColorRect, "size", new Vector2(0, 720), 1f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
                    };
                };
            }
        };
        OnLevelExit += (isPass) =>
        {
            if (_isOnLevel)
            {
                _soundLevelStart.PitchScale = 1;
                _soundLevelStart.Play();
                _isOnLevel = false;
                var tween = GetTree().CreateTween();
                tween.TweenProperty(_screenColorRect, "size", new Vector2(1280, 720), 1f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
                tween.Finished += () =>
                {
                    _soundLevelStart.PitchScale = .8f;
                    _soundLevelStart.Play();
                    var timer = GetTree().CreateTimer(1);
                    timer.Timeout += () =>
                    {
                        _currentLevel.QueueFree();
                        _nodes.Visible = true;
                        _levelSelectorTitle.Visible = true;
                        _hint.Visible = false;
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

    public void Break()
    {
        var timer = GetTree().CreateTimer(4);
        timer.Timeout += () =>
        {
            _breakSprite.Scale = new Vector2(2, 2);
            _breakSprite.Modulate = new Color(1, 1, 1, 0);
        };
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_breakSprite, "scale", new Vector2(1.181f, 1.181f), .8).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        var tween2 = GetTree().CreateTween();
        tween2.TweenProperty(_breakSprite, "modulate", new Color(1, 1, 1, 1), .8).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
    }
}
