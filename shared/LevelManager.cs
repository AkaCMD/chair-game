using Godot;
using System;
using System.IO;

public partial class LevelManager : Node
{
    public static LevelManager Instance { get; private set; }

    public string CurrentLevelName { get; private set; }

    public static Action<PackedScene> OnLevelEnter;
    public static Action<bool> OnLevelExit;

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PushError("Multiple LevelManager instances found.");
        }
    }

    public override void _Ready()
    {
        OnLevelEnter += HandleLevelEnter;
        OnLevelExit += HandleLevelExit;
    }

    private void HandleLevelEnter(PackedScene levelScene)
    {
        CurrentLevelName = Path.GetFileNameWithoutExtension(levelScene.ResourcePath);
        GameEventSignals.Instance?.EmitSignal(GameEventSignals.SignalName.LevelStarted, CurrentLevelName);

        GD.Print($"Entering level: {CurrentLevelName}");
    }

    private void HandleLevelExit(bool isCompleted)
    {
        if (CurrentLevelName != null)
        {
            GameEventSignals.Instance?.EmitSignal(GameEventSignals.SignalName.LevelQuit, CurrentLevelName);

            // If level was completed, save progress
            if (isCompleted)
            {
                SaveLevelProgress();
            }

            GD.Print($"Exiting level: {CurrentLevelName} (completed: {isCompleted})");

            CurrentLevelName = null;
        }
    }

    private void SaveLevelProgress()
    {
        if (CurrentLevelName != null && Game.Instance != null)
        {
            var saveManager = GetNode<SaveManager>("/root/SaveManager");
            if (saveManager != null)
            {
                saveManager.SubmitLevelClear(CurrentLevelName, Game.Instance.StepHistory);
            }
        }
    }

    public bool IsInLevel()
    {
        return CurrentLevelName != null;
    }

    public void LoadLevelDirectly(string levelPath)
    {
        var levelScene = GD.Load<PackedScene>(levelPath);
        if (levelScene != null)
        {
            HandleLevelEnter(levelScene);
        }
        else
        {
            GD.PushError($"Failed to load level: {levelPath}");
        }
    }

    public void ExitCurrentLevel(bool isCompleted = false)
    {
        if (CurrentLevelName != null)
        {
            HandleLevelExit(isCompleted);
        }
    }

    public override void _ExitTree()
    {
        OnLevelEnter -= HandleLevelEnter;
        OnLevelExit -= HandleLevelExit;

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
