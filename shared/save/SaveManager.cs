using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public partial class SaveManager : Node
{
    private const string SavePath = "user://savegame.json";

    public GameSaveData Data { get; private set; }

    public override void _Ready()
    {
        LoadGame();
    }

    public void SubmitLevelClear(string levelId, List<Step> validHistory)
    {
        if (Data == null) Data = new GameSaveData();

        StringBuilder sb = new StringBuilder();
        foreach (var step in validHistory)
        {
            char c = StepToChar(step);
            sb.Append(c);
        }
        
        var newRecord = new LevelSaveData
        {
            IsCleared = true,
            SolutionString = sb.ToString()
        };
        
        Data.Levels[levelId] = newRecord;
        SaveToDisk();
    }
    
    private char StepToChar(Step step)
    {
        if (step.Type == StepType.Action) return 'X';

        if (step.DirY < 0) return 'W';
        if (step.DirX < 0) return 'A';
        if (step.DirY > 0) return 'S';
        if (step.DirX > 0) return 'D';

        return '?';
    }

    private void SaveToDisk()
    {
        try
        {
            var options = new JsonSerializerOptions {WriteIndented = true};
            string json = JsonSerializer.Serialize(Data, options);
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            file.StoreString(json);
        }
        catch (Exception e)
        {
            GD.PrintErr("Save Failed: " + e.Message);
        }
    }

    private void LoadGame()
    {
        if (!FileAccess.FileExists(SavePath))
        {
            Data = new GameSaveData();
            return;
        }

        try
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            Data = JsonSerializer.Deserialize<GameSaveData>(json);
        }
        catch
        {
            Data = new GameSaveData();
        }
    }
    
    public List<Step> GetReplayData(string levelId)
    {
        if (Data != null && Data.Levels.TryGetValue(levelId, out var record))
        {
            List<Step> reconstructedSteps = new List<Step>();
            
            foreach (char c in record.SolutionString)
            {
                Step step = CharToStep(c);
                if (step != null) reconstructedSteps.Add(step);
            }
            return reconstructedSteps;
        }
        return null;
    }
    
    private Step CharToStep(char c)
    {
        switch (char.ToUpper(c))
        {
            case 'W': return Step.CreateMove(Vector2I.Up);
            case 'S': return Step.CreateMove(Vector2I.Down);
            case 'A': return Step.CreateMove(Vector2I.Left);
            case 'D': return Step.CreateMove(Vector2I.Right);
            case 'X': return Step.CreateAction();
            default: return null;
        }
    }
}
