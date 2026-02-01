using System.Collections.Generic;
using Godot;

public partial class LevelSelector : Node2D
{
    [Export] private float DRAGGING_LIMIT_X = 2000f;
    [Export] private Camera2D _camera;
    private bool _isHover = false;

    // prevent accidental touch
    [Export] private float DRAG_THRESHOLD = 20f;
    private Vector2 _dragStartPos;
    public bool IsDragging { get; private set; }

    [Export] private ColorRect _screenColorRect;
    [Export] private bool _isOnLevel = false;

    [Export] private CanvasLayer _canvasLayer;
    [Export] private Panel _levelSelectorTitle;
    [Export] public Node2D Nodes;
    [Export] private Sprite2D _breakSprite;

    private Node2D _currentLevel;
    public static LevelSelector Instance { get; private set; }

    [Export] private AudioStreamPlayer _soundLevelStart;

    // Store camera position to restore when returning to level selector
    private static Vector2 _savedCameraOffset = Vector2.Zero;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        // Restore camera position from previous visit
        if (_camera != null)
        {
            _camera.Offset = _savedCameraOffset;
        }

        var saveManager = GetNode<SaveManager>("/root/SaveManager");
        if (saveManager.IsFirstTime())
        {
            // When player enter the game the first time,
            // play some dialog
            var entryScene = GD.Load<PackedScene>("res://levels/entry/entry.tscn");
            _canvasLayer.AddChild(entryScene.Instantiate());
        }

        if (!saveManager.IsLevelCleared("tuto1") && saveManager.IsLevelCleared("beginning"))
        {
            DialogController.Instance.StartDialog(
                new List<DialogResource> { GD.Load<DialogResource>("res://dialog/dialog_resources/level_selector/dialog_levelselector.tres")});
        }
        LevelManager.Instance.OnLevelEnter += HandleLevelEnter;
        LevelManager.Instance.OnLevelExit += HandleLevelExit;
    }

    private void HandleLevelEnter(PackedScene packedLevel)
    {
        if (!_isOnLevel)
        {
            // Save camera position before entering level
            if (_camera != null)
            {
                _savedCameraOffset = _camera.Offset;
            }

            _soundLevelStart.PitchScale = 1;
            _soundLevelStart.Play();
            _isOnLevel = true;

            // Screen transition animation
            var tween = GetTree().CreateTween();
            tween.TweenProperty(_screenColorRect, "size", new Vector2(1280, 720), 1f)
                 .SetTrans(Tween.TransitionType.Cubic)
                 .SetEase(Tween.EaseType.Out);

            tween.Finished += () =>
            {
                _soundLevelStart.PitchScale = .8f;
                _soundLevelStart.Play();

                var timer = GetTree().CreateTimer(1);
                timer.Timeout += () =>
                {
                    // Hide level selector UI
                    Nodes.Visible = false;
                    _levelSelectorTitle.Visible = false;

                    // Instantiate and add the level
                    _currentLevel = (Node2D)packedLevel.Instantiate();
                    _canvasLayer.AddChild(_currentLevel);

                    // Screen transition back
                    var tween2 = GetTree().CreateTween();
                    tween2.TweenProperty(_screenColorRect, "size", new Vector2(0, 720), 1f)
                         .SetTrans(Tween.TransitionType.Cubic)
                         .SetEase(Tween.EaseType.In);
                };
            };
        }
    }

    private void HandleLevelExit(bool isPass)
    {
        if (_isOnLevel)
        {
            _soundLevelStart.PitchScale = 1;
            _soundLevelStart.Play();
            _isOnLevel = false;

            // Screen transition animation
            var tween = GetTree().CreateTween();
            tween.TweenProperty(_screenColorRect, "size", new Vector2(1280, 720), 1f)
                 .SetTrans(Tween.TransitionType.Cubic)
                 .SetEase(Tween.EaseType.Out);

            tween.Finished += () =>
            {
                _soundLevelStart.PitchScale = .8f;
                _soundLevelStart.Play();

                var timer = GetTree().CreateTimer(1);
                timer.Timeout += () =>
                {
                    // Clean up current level
                    _currentLevel?.QueueFree();
                    _currentLevel = null;

                    // Show level selector UI
                    Nodes.Visible = true;
                    _levelSelectorTitle.Visible = true;

                    // Restore camera position when returning to level selector
                    if (_camera != null)
                    {
                        _camera.Offset = _savedCameraOffset;
                    }

                    // Screen transition back
                    var tween2 = GetTree().CreateTween();
                    tween2.TweenProperty(_screenColorRect, "size", new Vector2(0, 720), 1f)
                         .SetTrans(Tween.TransitionType.Cubic)
                         .SetEase(Tween.EaseType.In);
                    
                    // Update level selector nodes
                    tween2.Finished += UpdateNodes;
                };
            };
        }
    }

    private void UpdateNodes()
    {
        foreach (var child in Nodes.GetChildren())
        {
            if (child is LevelNode levelNode)
            {
                levelNode.Init();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton inputEventMouseButton)
        {
            if (inputEventMouseButton.Pressed)
            {
                _isHover = true;
                _dragStartPos = inputEventMouseButton.Position;
                IsDragging = false;
            }
            else
            {
                _isHover = false;
            }
        }

        if (@event is InputEventMouseMotion inputEventMouseMotion)
        {
            if (_isHover)
            {
                if (!IsDragging && _dragStartPos.DistanceTo(inputEventMouseMotion.Position) > DRAG_THRESHOLD)
                {
                    IsDragging = true;
                }
            }

            // Handle camera dragging
            _camera.Offset -= _isHover ? new Vector2(inputEventMouseMotion.Relative.X, 0) : Vector2.Zero;
            _camera.Offset = new Vector2(Mathf.Max(0, Mathf.Min(DRAGGING_LIMIT_X, _camera.Offset.X)), 0);
        }
    }

    public void Break()
    {
        // Break animation
        var timer = GetTree().CreateTimer(4);
        timer.Timeout += () =>
        {
            _breakSprite.Scale = new Vector2(2, 2);
            _breakSprite.Modulate = new Color(1, 1, 1, 0);
        };

        var tween = GetTree().CreateTween();
        tween.TweenProperty(_breakSprite, "scale", new Vector2(1.181f, 1.181f), .8)
             .SetTrans(Tween.TransitionType.Quad)
             .SetEase(Tween.EaseType.Out);

        var tween2 = GetTree().CreateTween();
        tween2.TweenProperty(_breakSprite, "modulate", new Color(1, 1, 1, 1), .8)
              .SetTrans(Tween.TransitionType.Quad)
              .SetEase(Tween.EaseType.Out);
    }

    public override void _ExitTree()
    {
        LevelManager.Instance.OnLevelEnter -= HandleLevelEnter;
        LevelManager.Instance.OnLevelExit -= HandleLevelExit;
    }
}
