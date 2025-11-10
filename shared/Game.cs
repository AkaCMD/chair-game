// Manage Movers, walls and undo/reset input

using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node
{
    public static Game Instance { get; private set; }

    public static List<Mover> Movers = new();

    [Export] private TileMapLayer _objects;

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PushError("Multiple GameManager instances found.");
        }
    }

    public override void _Ready()
    {
        CallDeferred("SetReferences");
    }

    public void SetReferences()
    {
        Movers.Clear();

        Movers = GetTree().GetNodesInGroup("movers").Cast<Mover>().ToList();
        
        GD.Print($"Find {Movers.Count} Movers: ");
        foreach (var mover in Movers)
        {
            GD.Print($"- {mover.Name} Pos: {mover.GridPosition}");
        }
    }
}
