using Godot;

public partial class DamageArea : Area2D
{
    [Export] private AnimationPlayer _animationPlayer;
    [Export] private AudioStreamPlayer _sfx;
    
    private bool _playerInArea = false;
    private bool _wasPlayerSitting = false;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        
        GameEventSignals.Instance.Undo += ResetAnimation;
        GameEventSignals.Instance.Reset += ResetAnimation;
    }

    public override void _Process(double delta)
    {
        if (!_playerInArea || Player.Instance == null) return;
        
        bool isPlayerSitting = Player.Instance.IsSit;
        
        if (_wasPlayerSitting && !isPlayerSitting && !Player.Instance.IsSliding)
        {
            TriggerAttack();
        }
        
        _wasPlayerSitting = isPlayerSitting;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            _playerInArea = true;
            _wasPlayerSitting = Player.Instance?.IsSit ?? false;
            
            if (!_wasPlayerSitting && !Player.Instance.IsSliding)
            {
                TriggerAttack();
            }
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            _playerInArea = false;
            _wasPlayerSitting = false;
        }
    }

    private void TriggerAttack()
    {
        if (_sfx != null)
            _sfx.Play();
        
        if (_animationPlayer != null)
            _animationPlayer.Play("attack");
        
        if (!Player.Instance.IsSliding)
            Game.Instance.SetGameOver();
    }

    private void ResetAnimation()
    {
        if (_animationPlayer != null)
        {
            _animationPlayer.Play("default");
        }
    }
}
