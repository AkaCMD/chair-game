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
        GrabFocus();

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

        // Release focus
        ReleaseFocus();

        Visible = false;

        // Reset label
        if (_creditsLabel != null)
        {
            _creditsLabel.Text = "";
        }
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
