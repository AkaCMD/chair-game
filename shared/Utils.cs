using Godot;
using System;

public static class Utils
{
    public static void PlayWithRandomPitch(AudioStreamPlayer audioPlayer,
        float minPitch = 0.8f, float maxPitch = 1.2f)
    {
        if (audioPlayer == null || !GodotObject.IsInstanceValid(audioPlayer)) return;
        audioPlayer.PitchScale = (float) new Random().NextDouble() * (maxPitch - minPitch) + minPitch;
        audioPlayer.Play();
    }
}

public static class GameConstants
{
    public const int MaxMovementCycles = 30;
    public const float UndoRepeatDelay = 0.2f;
}
