using Godot;

public class GrabBoxCommand : IAction
{
    private Box _box;
    private Vector2I _boxPos;
    private string _boxScenePath;
    
    public GrabBoxCommand(Box boxToGrab)
    {
        _box = boxToGrab;
        _boxPos = boxToGrab.GridPosition;
        _boxScenePath = boxToGrab.SceneFilePath;
    }

    public void ExecuteCommand()
    {
        Player.Instance.HasBox = true;
        _box.QueueFree();
    }
    
    public void UndoCommand()
    {
        Player.Instance.HasBox = false;

        var packedBox = GD.Load<PackedScene>(_boxScenePath);
        var newBox = packedBox.Instantiate<Box>();
        Game.Instance.ObjectsTileMapLayer.AddChild(newBox);
        newBox.GridPosition = _boxPos;
    }
}