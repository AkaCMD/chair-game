using Godot;

public partial class DamageArea : Area2D
{
    [Export] private AnimationPlayer _animationPlayer;
    [Export] private AudioStreamPlayer _sfx;
    
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        GameEventSignals.Instance.Undo += ResetAnimation;
        GameEventSignals.Instance.Reset += ResetAnimation;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            if (_sfx != null)
                _sfx.Play();
            if (_animationPlayer != null)
                _animationPlayer.Play("attack");
            if (!Player.Instance.IsSliding)
                Game.Instance.SetGameOver();
        }
    }
    
    private void ResetAnimation()
    {
        if (_animationPlayer != null)
        {
            _animationPlayer.Play("default");
        }
    }
}
