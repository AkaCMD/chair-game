using Godot;
using System;
using System.Collections.Generic;

public enum StepType
{
    Move,
    Action,
}

public class Step
{
    public StepType Type { get; set; }
    public int DirX { get; set; }
    public int DirY { get; set; }
    
    public Step() {}

    public static Step CreateMove(Vector2I dir)
    {
        return new Step
        {
            Type = StepType.Move,
            DirX = dir.X,
            DirY = dir.Y
        };
    }

    public static Step CreateAction()
    {
        return new Step
        {
            Type = StepType.Action,
            DirX = 0,
            DirY = 0
        };
    }
}

public class LevelSaveData
{
    public bool IsCleared { get; set; }
    public string SolutionString { get; set; } = "";
}

public class GameSaveData
{
    public Dictionary<string, LevelSaveData> Levels { get; set; } = new Dictionary<string, LevelSaveData>();
}
