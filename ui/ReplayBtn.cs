using System.IO;
using Godot;

public partial class ReplayBtn : Control
{
    [Export] private Button _btn;
    
    public override void _Ready()
    {
        var saveManager = GetNode<SaveManager>("/root/SaveManager");
        var currentLevelName = LevelManager.Instance?.CurrentLevelName ?? Path.GetFileNameWithoutExtension(GetTree().CurrentScene.SceneFilePath); 
        if (saveManager.GetReplayData(currentLevelName) != null)
        {
            Visible = true;
            _btn.Pressed += () =>
                GetNode<ReplaySystem>("/root/ReplaySystem").StartReplay(currentLevelName);
        }
        else
        {
            Visible = false;
        }
    }
}
