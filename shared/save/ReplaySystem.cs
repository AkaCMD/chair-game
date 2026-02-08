using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class ReplaySystem : Node
{
    [Export] public float Interval = 0.2f;

    public async void StartReplay(string levelId)
    {
        if (Game.Instance.IsReplaying) return;
        Game.Instance.IsReplaying = true;

        var saveManager = GetNode<SaveManager>("/root/SaveManager");
        List<Step> solution = saveManager.GetReplayData(levelId);
        if (solution == null)
        {
            GD.Print("No solution record available for this level.");
            Game.Instance.IsReplaying = false;
            return;
        }

        Player.Instance.InputBuffer.Clear();
        Game.Instance.ExecuteReset();

        // Wait for one frame to ensure the level is fully loaded and all nodes are initialized
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        foreach (var step in solution)
        {
            if (step.Type == StepType.Move)
            {
                Player.Instance.InjectMoveInput(new Vector2I(step.DirX, step.DirY));
            }
            else
            {
                Player.Instance.InjectActionInput();
            }

            await WaitForAllMovementComplete();
            await ToSignal(GetTree().CreateTimer(Interval), SceneTreeTimer.SignalName.Timeout);
        }
        EndReplay();
    }

    private void EndReplay()
    {
        Game.Instance.IsReplaying = false;
    }

    private async Task WaitForAllMovementComplete()
    {
        while ((Game.Instance != null && Game.Instance.HasMoverSliding()) || 
               (Player.Instance != null && Player.Instance.InputBuffer.Count > 0))
        {
            await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);
        }
    }
}
