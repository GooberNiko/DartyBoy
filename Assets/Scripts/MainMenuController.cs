using UnityEngine;
using UnityEngine.UI;

/// <summary>Wires the main menu buttons to the UIManager. Lives on the MainMenu panel.</summary>
public class MainMenuController : MonoBehaviour
{
    void Awake()
    {
        Bind("Start", OnStart);
        Bind("Shop", OnShop);
        Bind("Settings", OnSettings);
    }

    void OnStart()    { if (UIManager.Instance != null) UIManager.Instance.StartGame(); }
    void OnShop()     { if (UIManager.Instance != null) UIManager.Instance.OpenShop(); }
    void OnSettings() { if (UIManager.Instance != null) UIManager.Instance.OpenSettings(); }

    void Bind(string childName, UnityEngine.Events.UnityAction action)
    {
        var t = transform.Find(childName);
        if (t == null) { Debug.LogWarning("[MainMenu] missing child '" + childName + "'"); return; }
        var btn = t.GetComponent<Button>();
        if (btn == null) { Debug.LogWarning("[MainMenu] '" + childName + "' has no Button"); return; }
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }
}
