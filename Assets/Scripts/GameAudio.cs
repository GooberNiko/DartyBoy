using UnityEngine;

/// <summary>
/// Applies persisted audio volumes. SFX is treated as the master level via the
/// AudioListener; BGM is persisted for a future dedicated music source / mixer.
/// </summary>
public static class GameAudio
{
    public static void Apply()
    {
        AudioListener.volume = Mathf.Clamp01(SaveData.SfxVolume);
    }
}
