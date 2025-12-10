using Godot;

[GlobalClass]
public partial class DialogueResource : Resource
{
    [Export] public string SpeakerName;
    [Export] public Texture2D SpeakerImg;
    [Export(PropertyHint.MultilineText)] public string Text;
    [Export(PropertyHint.Range, "0.1,30.0,0.1")] public float TextSpeed;
    [Export] public AudioStream TextSound;
    [Export] public int TextVolume;
}
