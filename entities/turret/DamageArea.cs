using Godot;

public partial class DamageArea : Area2D
{
    [Export] private AnimationPlayer _animationPlayer;
    [Export] private AudioStreamPlayer _sfx;

    private bool _wasPlayerLeavingChair = false;

    public override void _Ready()
    {
        GameEventSignals.Instance.Undo += ResetAnimation;
        GameEventSignals.Instance.Reset += ResetAnimation;

        GameEventSignals.Instance.MoveComplete += CheckDamage;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GameEventSignals.Instance.Undo -= ResetAnimation;
            GameEventSignals.Instance.Reset -= ResetAnimation;
            GameEventSignals.Instance.MoveComplete -= CheckDamage;
        }

        base.Dispose(disposing);
    }

    public override void _Process(double delta)
    {
        // Check for delayed damage after chair exit
        if (Player.Instance != null && _wasPlayerLeavingChair && !Player.Instance.IsLeavingChair)
        {
            _wasPlayerLeavingChair = false;

            // Player just finished leaving chair, check if they're in danger now
            if (!Player.Instance.IsSit && !Player.Instance.IsSliding && IsPlayerOverlapping())
            {
                TriggerAttack();
            }
        }

        // Track when player is leaving chair
        if (Player.Instance != null && Player.Instance.IsLeavingChair && !_wasPlayerLeavingChair)
        {
            _wasPlayerLeavingChair = true;
        }
    }

    private void CheckDamage()
    {
        if (Player.Instance == null) return;

        // Skip damage check if player is sitting, sliding, or leaving a chair
        if (Player.Instance.IsSit || Player.Instance.IsSliding || Player.Instance.IsLeavingChair)
        {
            return;
        }

        if (IsPlayerOverlapping())
        {
            TriggerAttack();
        }
    }

    public bool IsPlayerOverlapping()
    {
        var bodies = GetOverlappingBodies();
        foreach (var body in bodies)
        {
            if (body.IsInGroup("player")) return true;
        }
        return false;
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
