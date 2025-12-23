using Godot;

public partial class Entry : Node
{
    public override void _Ready()
    {
        GameEventSignals.Instance.DialogComplete += GoToBeginningScene;
    }

    private void GoToBeginningScene(string _)
    {
        LevelManager.Instance.LoadLevelDirectly("res://levels/beginning.tscn");
        QueueFree();
    }

    public override void _ExitTree()
    {
        GameEventSignals.Instance.DialogComplete -= GoToBeginningScene;
        base._ExitTree();
    }
}
