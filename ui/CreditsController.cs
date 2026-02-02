using Godot;
using System.Collections.Generic;

public partial class CreditsController : Control
{
    [Signal]
    public delegate void CreditsFinishedEventHandler();

    [Export] private Label _creditsLabel;
    [Export] private Timer _displayTimer;
    [Export] private AudioStreamPlayer _bgmPlayer;
    [Export] private float _displayInterval = 2.0f; // Time between each credit line

    private List<string> _creditLines = new List<string>();
    private int _currentIndex = 0;
    private bool _isShowing = false;
    private bool _creditsDone = false; // Credits displayed but waiting for skip input

    // Store reference to paused level music
    private AudioStreamPlayer _pausedLevelMusic = null;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.All;

        if (_displayTimer != null)
        {
            _displayTimer.WaitTime = _displayInterval;
            _displayTimer.Timeout += OnDisplayTimerTimeout;
        }

        // Set BGM to loop
        if (_bgmPlayer != null)
        {
            _bgmPlayer.Finished += OnBGMFinished;
        }

        // Load all credit lines from localization
        LoadCreditLines();
    }

    private void LoadCreditLines()
    {
        _creditLines.Clear();

        // Load credit lines from credit_00 to credit_06
        for (int i = 0; i <= 6; i++)
        {
            string key = $"credit_{i:D2}"; // credit_00, credit_01, ..., credit_06
            string text = Tr(key);

            // Only add if translation exists (not empty and not the key itself)
            if (!string.IsNullOrEmpty(text) && text != key)
            {
                _creditLines.Add(text);
            }
        }

        GD.Print($"Loaded {_creditLines.Count} credit lines");
    }

    public void StartCredits()
    {
        if (_isShowing) return;

        _isShowing = true;
        _creditsDone = false;
        _currentIndex = 0;
        Visible = true;

        // Grab focus for keyboard input
        if (IsInsideTree())
        {
            GrabFocus();
        }

        // Pause level music before starting credits BGM
        PauseLevelMusic();

        // Start BGM
        if (_bgmPlayer != null && !_bgmPlayer.Playing)
        {
            _bgmPlayer.Play();
        }

        // Start with first line
        ShowCurrentLine();
    }

    public void StopCredits()
    {
        _isShowing = false;
        _creditsDone = false;

        if (_displayTimer != null)
        {
            _displayTimer.Stop();
        }

        // Stop BGM
        if (_bgmPlayer != null)
        {
            _bgmPlayer.Stop();
        }

        // Resume level music after stopping credits BGM
        ResumeLevelMusic();

        // Release focus
        if (IsInsideTree())
        {
            ReleaseFocus();
        }

        Visible = false;

        // Reset label
        if (_creditsLabel != null)
        {
            _creditsLabel.Text = "";
        }
    }

    // Pause the level music (Music node in level_selector scene)
    private void PauseLevelMusic()
    {
        _pausedLevelMusic = null;

        // Try to find the Music node
        var root = GetTree().Root;

        // First try by known path
        var musicNode = root.GetNodeOrNull<AudioStreamPlayer>("/root/LevelSelector/Music");
        if (musicNode == null)
        {
            // Fallback: search recursively for a node named "Music"
            musicNode = FindMusicNodeInTree(root);
        }

        if (musicNode != null && musicNode.Playing)
        {
            // Pause without checking what it's playing
            musicNode.StreamPaused = true;
            _pausedLevelMusic = musicNode;
            GD.Print($"Paused level music: {musicNode.Name} (path: {musicNode.GetPath()})");
        }
    }

    // Recursively search for a node named "Music" that is an AudioStreamPlayer
    private AudioStreamPlayer FindMusicNodeInTree(Node node)
    {
        if (node is AudioStreamPlayer player && node.Name == "Music")
        {
            return player;
        }

        foreach (var child in node.GetChildren())
        {
            var found = FindMusicNodeInTree(child);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    // Resume the previously paused level music
    private void ResumeLevelMusic()
    {
        if (_pausedLevelMusic != null && IsInstanceValid(_pausedLevelMusic))
        {
            _pausedLevelMusic.StreamPaused = false;
            GD.Print($"Resumed level music: {_pausedLevelMusic.Name}");
        }
        _pausedLevelMusic = null;
    }

    private void ShowCurrentLine()
    {
        if (_currentIndex >= _creditLines.Count)
        {
            // All credits shown, but keep BGM playing and wait for user input
            _creditsDone = true;

            // Clear text or show final message (optional)
            if (_creditsLabel != null)
            {
                // Keep last line visible or show "Press any key to continue"
                // For now, keep last line
            }

            // Stop timer since credits are done
            if (_displayTimer != null)
            {
                _displayTimer.Stop();
            }

            return;
        }

        if (_creditsLabel != null)
        {
            string line = _creditLines[_currentIndex];
            _creditsLabel.Text = line;
        }

        // Start timer for next line
        if (_displayTimer != null)
        {
            _displayTimer.Start();
        }
    }

    private void OnDisplayTimerTimeout()
    {
        if (!_isShowing) return;

        _currentIndex++;
        ShowCurrentLine();
    }

    private void OnBGMFinished()
    {
        // Loop BGM
        if (_bgmPlayer != null && _isShowing)
        {
            _bgmPlayer.Play();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isShowing || @event == null) return;

        // Only accept input after credits are done displaying
        if (_creditsDone && @event.IsPressed() && !@event.IsEcho())
        {
            // Skip credits and BGM
            EmitSignal(nameof(CreditsFinished));
            StopCredits();

            // Accept the event to prevent propagation
            var viewport = GetViewport();
            if (viewport != null)
            {
                viewport.SetInputAsHandled();
            }
        }
    }

    public bool IsShowing()
    {
        return _isShowing;
    }

    public bool IsCreditsDone()
    {
        return _creditsDone;
    }

    public void SetDisplayInterval(float interval)
    {
        _displayInterval = interval;
        if (_displayTimer != null)
        {
            _displayTimer.WaitTime = _displayInterval;
        }
    }

    public override void _ExitTree()
    {
        // Clean up event subscriptions before node is removed from tree
        if (IsInstanceValid(_displayTimer))
        {
            _displayTimer.Timeout -= OnDisplayTimerTimeout;
        }

        if (IsInstanceValid(_bgmPlayer))
        {
            _bgmPlayer.Finished -= OnBGMFinished;
        }

        base._ExitTree();
    }
}
