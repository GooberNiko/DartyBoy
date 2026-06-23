using UnityEngine;

/// <summary>
/// Top-level screen switcher. Exactly one screen is shown at a time.
/// Lives on the root "Game" canvas and boots into the main menu.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject shop;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject game;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GameAudio.Apply();
        OpenMainMenu();
    }

    void ShowOnly(GameObject target)
    {
        if (mainMenu != null) mainMenu.SetActive(target == mainMenu);
        if (shop != null)     shop.SetActive(target == shop);
        if (settings != null) settings.SetActive(target == settings);
        if (game != null)     game.SetActive(target == game);
    }

    public void OpenMainMenu() { ShowOnly(mainMenu); }
    public void OpenShop()     { ShowOnly(shop); }
    public void OpenSettings() { ShowOnly(settings); }

    public void StartGame()
    {
        ShowOnly(null); // hide all menus; gameplay renders against the camera sky + HUD overlay
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
    }
}
