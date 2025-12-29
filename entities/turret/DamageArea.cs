using Godot;
using System;

public partial class DamageArea : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            if (Game.Instance != null)
            {
                Game.Instance.SetGameOver();
            }
        }
    }
}
