using System.Collections.Generic;
using System.IO;
using Godot;

public partial class DeveloperCommands : Node
{
    private const string UNLOCK_ALL_KEY = "F5";
    private const string COMPLETE_CURRENT_KEY = "F6";
    private const string CLEAR_SAVE_KEY = "F7";
    
    public override void _EnterTree()
    {
        GD.Print("开发者快捷键：F5=解锁全关卡，F6=直接过关，F7=清除存档\n");
        GD.Print("1. F5 (解锁全部关卡)");
        GD.Print("   - 必须在选关界面使用");
        GD.Print("   - 自动扫描 `LevelSelector` 场景中 `Nodes` 节点下的所有 `LevelNode`");
        GD.Print("   - 使用每个 `LevelNode` 的 `PackedLevel` 属性确定关卡ID");
        GD.Print("   - 只解锁尚未完成的关卡");
        GD.Print("   - 自动刷新关卡节点的颜色显示");
        GD.Print("");
        GD.Print("2. F6 (直接过关当前关卡)");
        GD.Print("   - 可在任何关卡场景中使用");
        GD.Print("   - 自动识别当前关卡并标记为完成");
        GD.Print("   - 触发正常的关卡完成流程");
        GD.Print("");
        GD.Print("3. F7 (清除存档)");
        GD.Print("   - 在任何场景中使用");
        GD.Print("   - 如果在选关界面，会自动重新加载场景\n");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            // F5: 解锁全部关卡
            if (keyEvent.Keycode == Key.F5)
            {
                UnlockAllLevels();
                GetViewport().SetInputAsHandled();
            }
            // F6: 直接过关当前所在关卡
            else if (keyEvent.Keycode == Key.F6)
            {
                CompleteCurrentLevel();
                GetViewport().SetInputAsHandled();
            }
            // F7: 清除存档
            else if (keyEvent.Keycode == Key.F7)
            {
                ClearSaveData();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void UnlockAllLevels()
    {
        if (LevelSelector.Instance == null)
        {
            GD.PrintErr("当前不在选关界面！请在选关界面使用 F5 解锁全关卡。");
            return;
        }
        var saveManager = GetNode<SaveManager>("/root/SaveManager");
        var nodesParent = LevelSelector.Instance.Nodes;

        var levelNodes = new List<LevelNode>();

        foreach (var child in nodesParent.GetChildren())
        {
            if (child is LevelNode levelNode)
            {
                string levelPath = levelNode.PackedLevel.ResourcePath;
                string levelId = Path.GetFileNameWithoutExtension(levelPath);
                
                if (saveManager.IsLevelCleared(levelId))
                {
                    GD.Print($"关卡 {levelId} ({levelNode.Name}) 已解锁，跳过");
                    continue;
                }
                
                saveManager.SubmitLevelClear(levelId, new List<Step>());
                GD.Print($"已解锁关卡: {levelId} ({levelNode.Name})");
            }
        }
        
        GetTree().ReloadCurrentScene();
    }

    private void CompleteCurrentLevel()
    {
        var levelManager = LevelManager.Instance;
        var saveManager = GetNode<SaveManager>("/root/SaveManager");
        
        string currentLevel = levelManager.CurrentLevelName;
        if (string.IsNullOrEmpty(currentLevel))
        {
            currentLevel = Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath);
        }
        
        saveManager.SubmitLevelClear(currentLevel, new List<Step>());
        GameEventSignals.Instance?.EmitSignal(GameEventSignals.SignalName.LevelComplete, currentLevel);
        if (LevelSelector.Instance == null)
        {
            GetTree().ChangeSceneToFile("res://level_selector/level_selector.tscn");
        }
        else
        {
            levelManager.EmitLevelExit(true);
        }
    }
    
    private void ClearSaveData()
    {
        var saveManager = GetNode<SaveManager>("/root/SaveManager");
        saveManager.ClearSaveData();

        if (LevelSelector.Instance != null)
        {
            GetTree().ReloadCurrentScene();
        }
    }
}