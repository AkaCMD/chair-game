using Godot;

public partial class ShineLoop : Sprite2D
{
	[Export] public float Sweep = 0.25f;
	[Export] public float Pause = 1.0f;

	float t = 0f;
	Tween tw;

	public override void _Ready()
	{
		if (Material is ShaderMaterial sm)
			Material = (ShaderMaterial)sm.Duplicate();

		t = 0f;
	}

	public override void _Process(double delta)
	{
		if (Material is not ShaderMaterial mat) return;

		if (tw != null && tw.IsRunning()) return;

		t -= (float)delta;
		if (t > 0f) return;

		tw?.Kill();
		tw = null;

		mat.SetShaderParameter("progress", -1.0f);
		tw = CreateTween();
		tw.TweenProperty(mat, "shader_parameter/progress", 2.0f, Sweep)
		  .SetTrans(Tween.TransitionType.Sine)
		  .SetEase(Tween.EaseType.InOut);

		t = Pause;
	}
}
