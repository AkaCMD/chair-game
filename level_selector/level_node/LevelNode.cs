using System.IO;
using Godot;

[Tool]
public partial class LevelNode : Node2D
{
    [Export] private Area2D _area;
    [Export] private Sprite2D _sprite;
    [Export] private Label _numberUI;

    [Export] private Label _nameUI;
    [Export] private Panel _panelUI;
    [Export] private Panel _hoverUI;
    
    private string _levelID;
    [Export]
    public string LevelID
    {
        get => _levelID;
        set
        {
            _levelID = value;
            UpdateDisplay();
        }
    }
    
    private string _levelName = "";
    [Export]
    public string LevelName
    {
        get => _levelName;
        set
        {
            _levelName = value;
            UpdateDisplay();
        }
    }
    
    private LevelNode[] _levelNodes = new LevelNode[0];
    [Export]
    public LevelNode[] LevelNodes
    {
        get => _levelNodes;
        set
        {
            _levelNodes = value ?? new LevelNode[0];
            QueueRedraw();
        }
    }
    
    [Export] public PackedScene PackedLevel;

    [Export] public Color DisabledColor = new Color(112f / 255, 112f / 255, 112f / 255);
    [Export] public Color NormalColor = new Color(1, 1, 1);
    private bool _isHover = false;

    // Store delegate references for proper unsubscription
    private LevelManager.OnLevelEnterEventHandler _onLevelEnterDelegate;

    public override void _Ready()
    {
        UpdateDisplay();
        QueueRedraw();
        
        if (!Engine.IsEditorHint())
        {
            _hoverUI.Scale = new Vector2(0, 1);
            _area.MouseEntered += () =>
            {
                var tween = GetTree().CreateTween();
                var tween2 = GetTree().CreateTween();
                tween.TweenProperty(_panelUI, "rotation", .2f, .5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
                tween2.TweenProperty(_hoverUI, "scale", Vector2.One, .5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
                _isHover = true;
            };
            _area.MouseExited += () =>
            {
                var tween = GetTree().CreateTween();
                var tween2 = GetTree().CreateTween();
                tween.TweenProperty(_panelUI, "rotation", 0f, .5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
                tween2.TweenProperty(_hoverUI, "scale", new Vector2(0, 0), .5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                _isHover = false;
            };

            _onLevelEnterDelegate = (packedScene) =>
            {
                _isHover = false;
            };

            LevelManager.Instance.OnLevelEnter += _onLevelEnterDelegate;
            Init();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Engine.IsEditorHint() && @event is InputEventMouseButton inputEventMouseButton && _isHover)
        {
            if (inputEventMouseButton.IsReleased())
            {
                if (LevelSelector.Instance.IsDragging)
                {
                    return;
                }
                
                if (IsCleared() || CanEnter())
                {
                    LevelManager.Instance.EmitLevelEnter(PackedLevel);
                }
            }
        }
    }

    public override void _Draw()
    {
        foreach (var item in LevelNodes)
        {
            if (item != null)
            {
                DrawLine(GlobalPosition - Position, item.GlobalPosition - Position, Color.Color8(255, 255, 255), 5);
            }
        }
    }

    public override void _ExitTree()
    {
        if (!Engine.IsEditorHint() && _onLevelEnterDelegate != null)
        {
            LevelManager.Instance.OnLevelEnter -= _onLevelEnterDelegate;
        }
    }

    private void UpdateDisplay()
    {
        if (_numberUI == null || _nameUI == null)
        {
            return;
        }
        
        _numberUI.Text = LevelID;
        _nameUI.Text = LevelName;
        
        QueueRedraw();
    }

    private void Init()
    {
        if (IsCleared())
        {
            _hoverUI.Modulate = NormalColor;
            _panelUI.Modulate = NormalColor;
            _sprite.Modulate = NormalColor;
        }
        else if (CanEnter())
        {
            _hoverUI.Modulate = NormalColor;
            _panelUI.Modulate = NormalColor;
            _sprite.Modulate = DisabledColor;
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
        if (Engine.IsEditorHint() || PackedLevel == null || PackedLevel.ResourcePath == null)
            return true;
            
        return GetNode<SaveManager>("/root/SaveManager").IsLevelCleared(Path.GetFileNameWithoutExtension(PackedLevel.ResourcePath));
    }

    public bool CanEnter()
    {
        if (Engine.IsEditorHint())
            return true;
            
        bool canEnter = true;
        foreach (var node in LevelNodes)
        {
            if (node != null && !node.IsCleared())
            {
                canEnter = false;
            }
        }
        return canEnter;
    }
}
