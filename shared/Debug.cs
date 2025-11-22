using Godot;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

public partial class Debug : Node
{
    public override void _Process(double delta)
    {
        ImGui.Begin("Debug Panel");
        
        // show player's direction
        ImGui.Text($"Player Direction: {Player.Instance.Direction}");
        ImGui.Separator();
        
        // show movers in the scene
        if (ImGui.CollapsingHeader("Objects List"))
        {
            ImGui.BeginChild("ObjectContainer", new Vector2(0, 150));
            foreach (var mover in Game.Instance.Movers)
            {
                if (GodotObject.IsInstanceValid(mover))
                {
                    string text = $"{mover.Name}, Pos: {mover.GridPosition}";
                    ImGui.Selectable(text);
                }
            }
            ImGui.EndChild();
        }
        ImGui.Separator();
        
        // show command stack
        if (ImGui.CollapsingHeader($"Commands ({CommandManager.CommandsStack.Count} Turns)"))
        {
            int turnIndex = CommandManager.CommandsStack.Count;
            foreach (var turnStack in CommandManager.CommandsStack)
            {
                if (ImGui.TreeNode($"Turn {turnIndex} - {turnStack.Count} Actions"))
                {
                    foreach (var action in turnStack)
                    {
                        ImGui.BulletText(action.ToString());
                    }
                    ImGui.TreePop();
                }
                turnIndex--;
            }
        }
        
        ImGui.End();
    }
}
