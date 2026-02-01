using Godot;
using System.Collections.Generic;

public partial class CreditsController : Control
{
    [Export] private Label _creditsLabel;
    [Export] private Timer _displayTimer;
    [Export] private float _displayInterval = 2.0f; // Time between each credit line

    private List<string> _creditLines = new List<string>();
    private int _currentIndex = 0;
    private bool _isShowing = false;

    public override void _Ready()
    {
        if (_displayTimer != null)
        {
            _displayTimer.WaitTime = _displayInterval;
            _displayTimer.Timeout += OnDisplayTimerTimeout;
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
        _currentIndex = 0;
        Visible = true;

        // Start with first line
        ShowCurrentLine();
    }

    public void StopCredits()
    {
        _isShowing = false;
        if (_displayTimer != null)
        {
            _displayTimer.Stop();
        }
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
            // All credits shown, stop
            StopCredits();
            return;
        }

        if (_creditsLabel != null)
        {
            string line = _creditLines[_currentIndex];
            _creditsLabel.Text = line;

            // Simply show text without animation
            // No scale effect, just display
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

    public bool IsShowing()
    {
        return _isShowing;
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
        // Clean up event subscription before node is removed from tree
        if (IsInstanceValid(_displayTimer))
        {
            _displayTimer.Timeout -= OnDisplayTimerTimeout;
        }

        base._ExitTree();
    }
}
