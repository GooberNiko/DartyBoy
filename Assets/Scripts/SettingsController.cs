using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires the Settings panel: SFX/BGM volume sliders (persisted), Reset Data, and Exit.
/// Lives on the Settings panel.
/// </summary>
public class SettingsController : MonoBehaviour
{
    private Slider sfxSlider;
    private Slider bgmSlider;

    void Awake()
    {
        sfxSlider = FindSlider("SFX Vol");
        bgmSlider = FindSlider("BGM Vol");
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        BindButton("Reset DAATA", OnResetData);
        BindButton("Exit", OnExit);
    }

    void OnEnable()
    {
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(SaveData.SfxVolume);
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(SaveData.BgmVolume);
        GameAudio.Apply();
    }

    void OnSfxChanged(float v) { SaveData.SfxVolume = v; GameAudio.Apply(); }
    void OnBgmChanged(float v) { SaveData.BgmVolume = v; GameAudio.Apply(); }

    void OnResetData()
    {
        SaveData.ResetAll();
        GameAudio.Apply();
    }

    void OnExit() { if (UIManager.Instance != null) UIManager.Instance.OpenMainMenu(); }

    Slider FindSlider(string n)
    {
        var t = transform.Find(n);
        return t != null ? t.GetComponent<Slider>() : null;
    }

    void BindButton(string n, UnityEngine.Events.UnityAction a)
    {
        var t = transform.Find(n);
        var b = t != null ? t.GetComponent<Button>() : null;
        if (b != null) { b.onClick.RemoveAllListeners(); b.onClick.AddListener(a); }
    }
}
