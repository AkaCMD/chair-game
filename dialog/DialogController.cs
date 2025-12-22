using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class DialogController : Node2D
{
    public static DialogController Instance { get; private set; }

    [ExportGroup("References")]
    [Export] private CanvasItem _dialogUI;
    [Export] private PackedScene _packedDialogText;
    [Export] private AudioStreamPlayer _soundTalk;
    [Export] private Node2D _textContainer;
    [Export] private TextureRect _portraitRect;
    [Export] private RichTextLabel _speakerName;

    [ExportGroup("Settings")]
    [Export] private float _charInterval = 0.05f;
    [Export] private float _charSpacing = 30f;
    [Export] private Array<DialogResource> _dialogQueue = new Array<DialogResource>(); // Being played when entering the scene

    private List<DialogResource> _runtimeQueue = new List<DialogResource>();
    private DialogResource _currentResource;
    private double _timer;
    private int _charIndex;
    private bool _isTyping = false;
    private string _currentDialogId = "";

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Ready()
    {
        _dialogUI.Visible = false;
        if (_dialogQueue.Count != 0)
        {
            // this plays the dialog when player entering the scene
            StartDialog(new List<DialogResource>(_dialogQueue), "AutoStartDialog");
        }
        //Test();
    }

    private void Test()
    {
        var line = GD.Load<DialogResource>("res://dialog/DialogResources/dialog_test.tres");
        StartDialog(new List<DialogResource> { line });
    }

    public void StartDialog(List<DialogResource> dialogs)
    {
        StartDialog(dialogs, "");
    }

    public void StartDialog(List<DialogResource> dialogs, string dialogId)
    {
        _runtimeQueue.Clear();
        _currentDialogId = dialogId;
        GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.DialogStart, _currentDialogId);

        foreach (var data in dialogs)
        {
            var lines = data.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var line in lines)
            {
                var runtimeData = (DialogResource) data.Duplicate();
                runtimeData.Text = line;
                _runtimeQueue.Add(runtimeData);
            }
        }

        OpenDialogUI();
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

        if (_runtimeQueue.Count == 0)
        {
            _currentResource = null;
            CloseDialogUI();
            GameEventSignals.Instance.EmitSignal(GameEventSignals.SignalName.DialogComplete, _currentDialogId);
            _currentDialogId = "";
            return;
        }

        _currentResource = _runtimeQueue[0];
        _runtimeQueue.RemoveAt(0);

        _portraitRect.Texture = _currentResource.SpeakerImg;
        _speakerName.Text = "[wave][center]" + _currentResource.SpeakerName;

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
        Utils.PlayWithRandomPitch(_soundTalk, _currentResource.PitchOffset);
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

    private void OpenDialogUI()
    {
        _dialogUI.Visible = true;
    }

    private void CloseDialogUI()
    {
        _dialogUI.Visible = false;
    }
}
