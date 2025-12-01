using Godot;

public partial class GameEventSignals : Node
{
    [Signal] public delegate void LevelStartedEventHandler(string levelName);
    [Signal] public delegate void LevelQuitEventHandler(string levelName);
    [Signal] public delegate void LevelCompleteEventHandler(string levelName);
    [Signal] public delegate void MoveStartEventHandler(Vector2I direction);
    [Signal] public delegate void MoveCompleteEventHandler();
    [Signal] public delegate void PushEventHandler();
    [Signal] public delegate void UndoEventHandler();
    [Signal] public delegate void ResetEventHandler();
    [Signal] public delegate void UISelectEventHandler();
    [Signal] public delegate void UISubmitEventHandler();

    public static GameEventSignals Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }
}
