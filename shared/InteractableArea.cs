using Godot;

public partial class InteractableArea : Area2D
{
    private IInteractable _interactable;
    private bool _playerInRange = false;

    public override void _Ready()
    {
        _interactable = GetParent() as IInteractable;
        if (_interactable == null)
        {
            GD.PushError($"Parent Node {GetParent().Name} hasn't impl IInteractable interface");
            QueueFree();
            return;
        }

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        _interactable.HideHint();
    }

    public override void _Process(double delta)
    {
        if (_playerInRange && Input.IsActionJustPressed("interact"))
        {
            _interactable.Interact();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            _playerInRange = true;
            _interactable.ShowHint();
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            _playerInRange = false;
            _interactable.HideHint();
        }
    }
}
