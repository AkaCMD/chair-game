using Godot;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

public partial class Debug : Node
{
    public override void _Process(double delta)
    {
        ImGui.Begin("Debug Panel");
        ImGui.Text($"Player Direction: {Player.Instance.Direction}");
        ImGui.Separator();
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
        ImGui.End();
    }
}
