using UnityEngine;

/// <summary>
/// Central persistent save layer for DartyBoy (PlayerPrefs-backed).
/// Stores coins, owned darts (bitmask), the equipped dart, and audio volumes.
/// </summary>
public static class SaveData
{
    const string K_Coins = "db_coins";
    const string K_Owned = "db_owned_darts"; // bitmask, bit i = dart index i owned
    const string K_Equip = "db_equipped_dart";
    const string K_Sfx   = "db_vol_sfx";
    const string K_Bgm   = "db_vol_bgm";
    const string K_Init  = "db_save_init";
    const string K_Best  = "db_best_score";

    public static bool Initialized
    {
        get { return PlayerPrefs.GetInt(K_Init, 0) == 1; }
        set { PlayerPrefs.SetInt(K_Init, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static int Coins
    {
        get { return PlayerPrefs.GetInt(K_Coins, 0); }
        set { PlayerPrefs.SetInt(K_Coins, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    // Default dart (index 0) is owned out of the box.
    public static int OwnedMask
    {
        get { return PlayerPrefs.GetInt(K_Owned, 1); }
        set { PlayerPrefs.SetInt(K_Owned, value); PlayerPrefs.Save(); }
    }

    public static bool IsOwned(int index) { return (OwnedMask & (1 << index)) != 0; }
    public static void SetOwned(int index) { OwnedMask = OwnedMask | (1 << index); }

    public static int EquippedDart
    {
        get { return PlayerPrefs.GetInt(K_Equip, 0); }
        set { PlayerPrefs.SetInt(K_Equip, value); PlayerPrefs.Save(); }
    }

    public static float SfxVolume
    {
        get { return PlayerPrefs.GetFloat(K_Sfx, 1f); }
        set { PlayerPrefs.SetFloat(K_Sfx, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    public static float BgmVolume
    {
        get { return PlayerPrefs.GetFloat(K_Bgm, 1f); }
        set { PlayerPrefs.SetFloat(K_Bgm, Mathf.Clamp01(value)); PlayerPrefs.Save(); }
    }

    public static int BestScore
    {
        get { return PlayerPrefs.GetInt(K_Best, 0); }
        set { PlayerPrefs.SetInt(K_Best, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    /// <summary>Resets gameplay progress (coins, owned/equipped darts). Keeps audio prefs.</summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(K_Coins);
        PlayerPrefs.DeleteKey(K_Owned);
        PlayerPrefs.DeleteKey(K_Equip);
        PlayerPrefs.DeleteKey(K_Init);
        PlayerPrefs.DeleteKey(K_Best);
        PlayerPrefs.Save();
    }
}
