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
    [Export] private float _charIntervalZh = 0.05f;  // Character interval for Chinese
    [Export] private float _charIntervalEn = 0.033f; // Character interval for English (approximately 1.5x faster)
    [Export] private float _charSpacingZh = 30f;
    [Export] private float _charSpacingEn = 15f;
    [Export] private int _maxCharsPerLineZh = 20;
    [Export] private int _maxCharsPerLineEn = 40;
    [Export] private float _lineSpacing = 40f;
    [Export] private Array<DialogResource> _dialogQueue = new Array<DialogResource>(); // Being played when entering the scene

    private List<DialogResource> _runtimeQueue = new List<DialogResource>();
    private DialogResource _currentResource;
    private double _timer;
    private int _charIndex;
    private bool _isTyping = false;
    private string _currentDialogId = "";

    // For English word wrapping and audio
    private bool _isEnglish = false;
    private List<int> _wordStartIndices = new List<int>(); // Start index of each word in the current line
    private List<int> _wordLengths = new List<int>();     // Length of each word
    private List<Vector2> _characterPositions = new List<Vector2>(); // Pre-calculated positions for all characters

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
        Visible = true;
        _dialogUI.Visible = false;
        if (_dialogQueue.Count != 0)
        {
            // This plays the dialog when player enters the scene
            StartDialog(new List<DialogResource>(_dialogQueue), "AutoStartDialog");
        }
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
            data.Text = Tr(data.Text);
            // Split text by newlines (preserve intentional line breaks)
            var lines = data.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var line in lines)
            {
                var runtimeData = (DialogResource)data.Duplicate();
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
        float charInterval = GetCharInterval();
        if (_timer > charInterval)
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
        _currentResource.SpeakerName = Tr(_currentResource.SpeakerName);
        _speakerName.Text = "[wave][center]" + _currentResource.SpeakerName;

        _charIndex = 0;
        _timer = 0;
        _isTyping = true;

        // Detect language and prepare for English word wrapping
        _isEnglish = !TranslationServer.GetLocale().StartsWith("zh");
        _wordStartIndices.Clear();
        _wordLengths.Clear();
        _characterPositions.Clear();

        if (_isEnglish && _currentResource.Text != null)
        {
            PrepareEnglishWordInfo(_currentResource.Text);
            CalculateEnglishCharacterPositions();
        }
    }

    private void PrepareEnglishWordInfo(string text)
    {
        _wordStartIndices.Clear();
        _wordLengths.Clear();

        bool inWord = false;
        int wordStart = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            bool isWordChar = IsEnglishWordChar(c);

            if (isWordChar && !inWord)
            {
                // Start of a word
                wordStart = i;
                inWord = true;
            }
            else if (!isWordChar && inWord)
            {
                // End of a word
                _wordStartIndices.Add(wordStart);
                _wordLengths.Add(i - wordStart);
                inWord = false;
            }
        }

        // Add the last word if we're still in a word
        if (inWord)
        {
            _wordStartIndices.Add(wordStart);
            _wordLengths.Add(text.Length - wordStart);
        }
    }

    private bool IsEnglishWordChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '\'' || c == '-';
    }

    private void CalculateEnglishCharacterPositions()
    {
        _characterPositions.Clear();
        if (_currentResource.Text == null) return;

        string text = _currentResource.Text;
        float spacing = _charSpacingEn;
        float maxLineWidth = _maxCharsPerLineEn * spacing;

        float currentX = 0;
        float currentY = 0;
        int wordIndex = 0;
        int charIndex = 0;

        while (charIndex < text.Length)
        {
            // Check if current character is part of a word
            bool isInWord = false;
            int wordStart = -1;
            int wordLength = 0;

            for (int i = 0; i < _wordStartIndices.Count; i++)
            {
                if (charIndex >= _wordStartIndices[i] &&
                    charIndex < _wordStartIndices[i] + _wordLengths[i])
                {
                    isInWord = true;
                    wordStart = _wordStartIndices[i];
                    wordLength = _wordLengths[i];
                    wordIndex = i;
                    break;
                }
            }

            if (isInWord)
            {
                // We're inside a word
                float wordWidth = wordLength * spacing;

                // Check if word fits on current line
                if (currentX + wordWidth > maxLineWidth && currentX > 0)
                {
                    // Word doesn't fit, move to next line
                    currentX = 0;
                    currentY += _lineSpacing;
                }

                // Calculate positions for all characters in this word
                for (int i = 0; i < wordLength; i++)
                {
                    _characterPositions.Add(new Vector2(currentX + i * spacing, currentY));
                }

                currentX += wordWidth;
                charIndex += wordLength;
                wordIndex++;
            }
            else
            {
                // We're on a non-word character (space or punctuation)
                // Check if we need to wrap (for very long lines with no spaces)
                if (currentX + spacing > maxLineWidth && currentX > 0)
                {
                    currentX = 0;
                    currentY += _lineSpacing;
                }

                _characterPositions.Add(new Vector2(currentX, currentY));
                currentX += spacing;
                charIndex++;
            }
        }
    }

    private void SpawnNextChar()
    {
        if (_charIndex >= _currentResource.Text.Length)
        {
            _isTyping = false;
            return;
        }

        // Check if we should play sound for this character
        bool shouldPlaySound = true;
        if (_isEnglish)
        {
            // Find which word we're in (if any)
            int wordIndex = -1;
            for (int i = 0; i < _wordStartIndices.Count; i++)
            {
                if (_charIndex >= _wordStartIndices[i] &&
                    _charIndex < _wordStartIndices[i] + _wordLengths[i])
                {
                    wordIndex = i;
                    break;
                }
            }

            // Play sound only at the start of each word
            if (wordIndex != -1)
            {
                shouldPlaySound = (_charIndex == _wordStartIndices[wordIndex]);
            }
            else
            {
                // For non-word characters (punctuation), play sound normally
                shouldPlaySound = true;
            }
        }

        CreateCharNode(_charIndex);

        if (shouldPlaySound)
        {
            Utils.PlayWithRandomPitch(_soundTalk, _currentResource.PitchOffset);
        }

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

        Vector2 position;

        if (_isEnglish && _characterPositions.Count > index)
        {
            // Use pre-calculated positions for English (with word wrapping)
            position = _characterPositions[index];
        }
        else
        {
            // Chinese or fallback: simple character-based layout
            float spacing = GetCharSpacing();
            int maxChars = GetMaxCharsPerLine();

            int col = index;
            int row = 0;
            if (maxChars > 0)
            {
                col = index % maxChars;
                row = index / maxChars;
            }

            position = new Vector2(col * spacing, row * _lineSpacing);
        }

        charNode.Setup(_currentResource.Text[index].ToString(), position);
    }

    // Get character interval based on current language
    private float GetCharInterval()
    {
        string locale = TranslationServer.GetLocale();
        if (locale.StartsWith("zh"))
        {
            return _charIntervalZh;
        }
        else
        {
            // English and other languages use faster interval
            return _charIntervalEn;
        }
    }

    // Get character spacing based on current language
    private float GetCharSpacing()
    {
        string locale = TranslationServer.GetLocale();
        if (locale.StartsWith("zh"))
        {
            return _charSpacingZh;
        }
        else
        {
            // Use English spacing for other languages
            return _charSpacingEn;
        }
    }

    // Get maximum characters per line based on current language
    private int GetMaxCharsPerLine()
    {
        string locale = TranslationServer.GetLocale();
        if (locale.StartsWith("zh"))
        {
            return _maxCharsPerLineZh;
        }
        else
        {
            // Use English maximum characters for other languages
            return _maxCharsPerLineEn;
        }
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
