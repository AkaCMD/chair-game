using System.IO;
using Godot;

public partial class LevelNode : Node2D
{
    [Export] private Area2D _area;
    [Export] private Sprite2D _sprite;
    [Export] private Label _numberUI;

    [Export] private Label _nameUI;
    [Export] private Panel _panelUI;
    [Export] private Panel _hoverUI;
    [Export] public string LevelID;
    [Export] public string LevelName;
    [Export] public LevelNode[] LevelNodes;
    [Export] public PackedScene PackedLevel;

    [Export] public Color DisabledColor = new Color(112f/255, 112f/255, 112f/255);
    [Export] public Color NormalColor = new Color(1, 1, 1);
    private bool _isHover = false;

    // Store delegate references for proper unsubscription
    private LevelManager.OnLevelEnterEventHandler _onLevelEnterDelegate;

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

        _onLevelEnterDelegate = (packedScene) =>
        {
            _isHover = false;
        };

        LevelManager.Instance.OnLevelEnter += _onLevelEnterDelegate;
        Init();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton inputEventMouseButton && _isHover)
        {
            LevelManager.Instance.EmitLevelEnter(PackedLevel);
        }
    }


    public override void _Draw()
    {
        foreach (var item in LevelNodes)
        {
            DrawLine(GlobalPosition - Position, item.GlobalPosition - Position, Color.Color8(255, 255, 255), 5);
        }
    }

    public override void _ExitTree()
    {
        if (_onLevelEnterDelegate != null)
        {
            LevelManager.Instance.OnLevelEnter -= _onLevelEnterDelegate;
        }
    }

    private void Init()
    {
        if (IsCleared())
        {
            _hoverUI.Modulate = NormalColor;
            _panelUI.Modulate = NormalColor;
            _sprite.Modulate = NormalColor;
        }
        else
        {
            _hoverUI.Modulate = DisabledColor;
            _panelUI.Modulate = DisabledColor;
            _sprite.Modulate = DisabledColor;   
        }
    }

    public bool IsCleared()
    {
        return GetNode<SaveManager>("/root/SaveManager").IsLevelCleared(Path.GetFileNameWithoutExtension(PackedLevel.ResourcePath));
    }

    // public bool CanEnter()
    // {
    //     return true;
    // }
}
