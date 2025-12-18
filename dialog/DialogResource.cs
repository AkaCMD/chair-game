using Godot;

[GlobalClass]
public partial class DialogResource : Resource
{
    [Export] public string SpeakerName;
    [Export] public Texture2D SpeakerImg;
    [Export(PropertyHint.MultilineText)] public string Text;
    [Export(PropertyHint.Range, "0.1,30.0,0.1")] public float TextSpeed;
    [Export(PropertyHint.Range, "-0.8, 0.8")] public float PitchOffset;
}
