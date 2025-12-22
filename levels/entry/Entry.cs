using Godot;

public partial class Entry : Node
{
    public override void _Ready()
    {
        GameEventSignals.Instance.DialogComplete += GoToBeginningScene;
    }

    private void GoToBeginningScene(string _)
    {
        GetTree().ChangeSceneToFile("res://levels/beginning.tscn");
    }

    public override void _ExitTree()
    {
        GameEventSignals.Instance.DialogComplete -= GoToBeginningScene;
        base._ExitTree();
    }
}
