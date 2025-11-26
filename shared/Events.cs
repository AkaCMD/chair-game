using System;
using Godot;

public class Events
{
    public static Action<string> OnLevelStarted;
    public static Action<string> OnLevelQuit;
    public static Action<string> OnLevelComplete;

    public static Action<Vector2I> OnMoveStart;
    public static Action OnMoveComplete;

    public static Action OnPush;
    public static Action OnUndo;
    public static Action OnReset;
    public static Action OnUISelect;
    public static Action OnUISubmit;
}