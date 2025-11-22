using System.Collections.Generic;
using Godot;

public partial class CommandManager : Node
{
    public static CommandManager Instance { get; private set; }
    public static Stack<Stack<IAction>> CommandsStack = new Stack<Stack<IAction>>();

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
        Events.OnMoveComplete += AddNewTurn;
    }

    public static void Init()
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
        while (CommandsStack.Count != 0 && CommandsStack.Peek().Count == 0)
        {
            CommandsStack.Pop();
        }

        if (CommandsStack.Count == 0)
        {
            AddNewTurn();
            return;
        }
        
        while (CommandsStack.Peek().Count != 0)
        {
            IAction action = CommandsStack.Peek().Pop();
            action.UndoCommand();
        }
    }
    
    public static void ResetAll()
    {
        while (CommandsStack.Count > 0)
        {
            var turn = CommandsStack.Pop();

            while (turn.Count > 0)
            {
                var action = turn.Pop();
                action.UndoCommand();
            }
        }

        AddNewTurn();
    }
}