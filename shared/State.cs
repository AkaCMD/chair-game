// Tracks the undo stack

using System.Collections.Generic;
using Godot;

public class State
{
    public struct MoversToTrack
    {
        public Mover mover;
        public Vector2I initialPos;
        public int initialRot;
        public List<Vector2I> positions;
        public List<int> rotations;
    }

    public static List<MoversToTrack> moversToTrack = new List<MoversToTrack>();
    public static int undoIndex;

    public static void AddMover(Mover mover)
    {
        MoversToTrack newMover = new MoversToTrack();
        newMover.mover = mover;
        newMover.initialPos = mover.GridPosition;
        newMover.initialRot = (int) mover.Rotation;
        newMover.positions = new List<Vector2I>();
        newMover.rotations = new List<int>();
        moversToTrack.Add(newMover);
    }

    public static void Init()
    {
        undoIndex = 0;
        moversToTrack.Clear();
    }

    public static void AddToUndoStack()
    {
        foreach (MoversToTrack m in moversToTrack)
        {
            m.positions.Add(m.mover.GridPosition);
            m.rotations.Add((int) m.mover.Rotation);
        }
    }

    private static void RemoveFromUndoStack()
    {
        foreach (MoversToTrack m in moversToTrack)
        {
            m.positions.RemoveAt(m.positions.Count - 1);
            m.rotations.RemoveAt(m.rotations.Count - 1);
            m.mover.GridPosition = m.positions[m.positions.Count - 1];
            m.mover.Rotation = m.rotations[m.rotations.Count - 1];
        }
    }

    public static void OnMoveComplete()
    {
        undoIndex++;
        AddToUndoStack();
    }

    public static void DoUndo()
    {
        if (undoIndex > 0)
        {
            undoIndex--;
            RemoveFromUndoStack();
        }
    }

    public static void DoReset()
    {
        foreach (MoversToTrack m in moversToTrack)
        {
            m.mover.GridPosition = m.initialPos;
            m.mover.Rotation = m.initialRot;
        }
        OnMoveComplete();
    }
}
