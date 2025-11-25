using Godot;
using System;
using System.Collections.Generic;
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

        var newRecord = new LevelSaveData
        {
            IsCleared = true,
            SolutionSteps = new List<Step>(validHistory)
        };

        Data.Levels[levelId] = newRecord;
        SaveToDisk();
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
        if (Data.Levels.ContainsKey(levelId))
            return Data.Levels[levelId].SolutionSteps;
        return null;
    }
}
