using System.Collections.Generic;
using Godot;

public partial class DialogController : Node2D
{
    [ExportGroup("References")] 
    [Export] private CanvasItem _dialogUI;
    [Export] private PackedScene _packedDialogText;
    [Export] public string Text;
    [Export] private AudioStreamPlayer _soundTalk;
    [Export] private Node2D _textContainer;

    [ExportGroup("Settings")] 
    [Export] private float _charInterval = 0.05f;
    [Export] private float _charSpacing = 25f;

    private List<DialogResource> _currentDialogQueue = new List<DialogResource>();
    private DialogResource _currentResource;
    private double _timer;
    private int _charIndex;
    private bool _isTyping = false;

    public override void _Ready()
    {
        var line1 = new DialogResource {Text = "你好世界"};
        var line2 = new DialogResource {Text = "再见世界"};
        StartDialog(new List<DialogResource> { line1, line2 });
    }

    public void StartDialog(List<DialogResource> dialogs)
    {
        _currentDialogQueue = new List<DialogResource>(dialogs);
        ShowNextLine();
    }
    
    public override void _Process(double delta)
    {
        if (!_isTyping || _currentResource == null) return;

        _timer += delta;
        if (_timer > _charInterval)
        {
            _timer = 0;
            SpawnNextChar();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") || @event is InputEventMouseButton
            {
                Pressed: true,
                ButtonIndex: MouseButton.Left
            })
        {
            if (_isTyping)
            {
                FinishCurrentLineImmediately();
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    private void ShowNextLine()
    {
        foreach (var child in _textContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (_currentDialogQueue.Count == 0)
        {
            GD.Print("Dialog Finished");
            _currentResource = null;
            _dialogUI.Visible = false;
            return;
        }

        _currentResource = _currentDialogQueue[0];
        _currentDialogQueue.RemoveAt(0);

        _charIndex = 0;
        _timer = 0;
        _isTyping = true;
    }

    private void SpawnNextChar()
    {
        if (_charIndex >= _currentResource.Text.Length)
        {
            _isTyping = false;
            return;
        } 
        CreateCharNode(_charIndex);
        Utils.PlayWithRandomPitch(_soundTalk);
        _charIndex++;
    }

    private void FinishCurrentLineImmediately()
    {
        while (_charIndex < _currentResource.Text.Length)
        {
            CreateCharNode(_charIndex);
            _charIndex++;
        }

        _isTyping = false;
    }

    private void CreateCharNode(int index)
    {
        var charNode = _packedDialogText.Instantiate<DialogText>();
        _textContainer.AddChild(charNode);
        
        // TODO: 换行
        var pos = new Vector2(index * _charSpacing, 0);
        charNode.Setup(_currentResource.Text[index].ToString(), pos);
    }
}
