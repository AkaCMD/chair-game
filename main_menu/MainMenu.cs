using Godot;

public partial class MainMenu : CanvasLayer
{
    [Export] private Button StartBtn;
    [Export] private Button ExitBtn;

    [Export] private PackedScene levelSelect;

    public override void _Ready()
    {
        base._Ready();
        StartBtn.Pressed += OnStartBtnPressed;
        ExitBtn.Pressed += () => GetTree().Quit();
    }

    private void OnStartBtnPressed()
    {
        GetTree().ChangeSceneToPacked(levelSelect);
    }
}
