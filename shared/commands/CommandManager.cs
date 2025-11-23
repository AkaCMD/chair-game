using System.Collections.Generic;
using Godot;

public partial class CommandManager : Node
{
    public static CommandManager Instance { get; private set; }
    public static Stack<Stack<IAction>> CommandsStack { get; private set; } = new Stack<Stack<IAction>>();

    public override void _EnterTree()
    {
        InitializeInstance();
        Events.OnMoveComplete += AddNewTurn;
    }

    private void InitializeInstance()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }

    public static void Initialize()
    {
        CommandsStack.Clear();
        AddNewTurn();
    }

    public static void ExecuteCommand(IAction action)
    {
        action.ExecuteCommand();

        if (CommandsStack.Count == 0)
        {
            AddNewTurn();
        }

        CommandsStack.Peek().Push(action);
    }

    public static void AddNewTurn()
    {
        if (!Game.Instance.HasMoverSliding())
        {
            CommandsStack.Push(new Stack<IAction>());
        }
    }

    public static void UndoCommand()
    {
        CleanEmptyTurns();

        if (CommandsStack.Count == 0)
        {
            AddNewTurn();
            return;
        }

        UndoCurrentTurn();
    }

    private static void CleanEmptyTurns()
    {
        while (CommandsStack.Count > 0 && CommandsStack.Peek().Count == 0)
        {
            CommandsStack.Pop();
        }
    }

    private static void UndoCurrentTurn()
    {
        var currentTurn = CommandsStack.Peek();

        while (currentTurn.Count > 0)
        {
            IAction action = currentTurn.Pop();
            action.UndoCommand();
        }
    }

    public static void ResetAll()
    {
        while (CommandsStack.Count > 0)
        {
            UndoTurn(CommandsStack.Pop());
        }

        AddNewTurn();
    }

    private static void UndoTurn(Stack<IAction> turn)
    {
        while (turn.Count > 0)
        {
            IAction action = turn.Pop();
            action.UndoCommand();
        }
    }
}
