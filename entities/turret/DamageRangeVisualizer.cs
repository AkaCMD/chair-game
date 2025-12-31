using Godot;
using System.Collections.Generic;

public partial class DamageRangeVisualizer : Node2D
{
    private List<MeshInstance2D> _meshes = new List<MeshInstance2D>();

    public override void _Ready()
    {
        // 获取父节点DamageArea
        DamageArea damageArea = GetParent<DamageArea>();
        if (damageArea == null)
        {
            GD.PrintErr("DamageRangeVisualizer must be a child of a DamageArea node");
            return;
        }

        // 为每个CollisionShape2D创建可视化mesh
        CreateVisualizationMeshes(damageArea);
    }

    private void CreateVisualizationMeshes(DamageArea damageArea)
    {
        // 遍历所有CollisionShape2D子节点
        foreach (Node child in damageArea.GetChildren())
        {
            if (child is CollisionShape2D collisionShape)
            {
                var shape = collisionShape.Shape;

                if (shape is RectangleShape2D rectShape)
                {
                    CreateRectangleMesh(collisionShape, rectShape);
                }
                // 可以在这里添加其他形状的支持
            }
        }
    }

    private void CreateRectangleMesh(CollisionShape2D collisionShape, RectangleShape2D rectShape)
    {
        // 创建 MeshInstance2D
        var meshInstance = new MeshInstance2D();
        meshInstance.Name = "DamageRangeMesh";

        // 设置 Z 索引，确保显示在炮塔下方
        meshInstance.ZIndex = -1;
        meshInstance.ZAsRelative = true;

        // 创建矩形 mesh
        var quadMesh = new QuadMesh();
        quadMesh.Size = rectShape.Size;

        meshInstance.Mesh = quadMesh;
        meshInstance.Position = collisionShape.Position;
        meshInstance.Rotation = collisionShape.Rotation;
        meshInstance.Scale = collisionShape.Scale;

        // 创建 ShaderMaterial
        var shaderMaterial = new ShaderMaterial();
        var shader = GD.Load<Shader>("res://shader/damage_range_simple.gdshader");
        shaderMaterial.Shader = shader;

        // 设置 shader 参数
        shaderMaterial.SetShaderParameter("size", rectShape.Size);
        shaderMaterial.SetShaderParameter("border_width", 8.0f); // 边框宽度
        shaderMaterial.SetShaderParameter("border_color", new Color(1.0f, 0.0f, 0.0f, 0.9f)); // 红色边框，较高不透明度
        shaderMaterial.SetShaderParameter("fill_color", new Color(1.0f, 0.2f, 0.2f, 0.8f)); // 浅红色填充
        shaderMaterial.SetShaderParameter("pulse_speed", 3f); // 较慢的脉冲速度

        meshInstance.Material = shaderMaterial;

        // 添加到场景
        AddChild(meshInstance);
        _meshes.Add(meshInstance);
    }

    public override void _ExitTree()
    {
        // 清理创建的 mesh
        foreach (var mesh in _meshes)
        {
            if (mesh != null && IsInstanceValid(mesh))
            {
                mesh.QueueFree();
            }
        }
        _meshes.Clear();
        base._ExitTree();
    }
}
