using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Dart shop with an animated horizontal carousel. Darts are laid out in a row;
/// the centered (selected) dart is larger and fully opaque while neighbours shrink
/// and fade. Left/Right change the selection and the whole row eases smoothly to it.
/// The row can also be swiped/dragged directly (touch or mouse); on release it
/// snaps to the nearest dart.
/// Lives on the Shop panel. Dart entries come from the "DARTS" child container
/// (sibling order); names/prices below are indexed to match that order.
/// </summary>
public class ShopController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Economy (index 0 = default dart, free & owned)")]
    [SerializeField] private int startingCoins = 0;
    [SerializeField] private string[] displayNames = { "Default", "Rainbow", "Dart 3", "Dart 4", "Dart 5", "Dart 6", "Dart 7" };
    [SerializeField] private int[] prices = { 0, 150, 300, 500, 750, 1000, 1500 };

    [Header("Carousel")]
    [SerializeField] private float spacing = 340f;        // horizontal gap between darts (panel-local units)
    [SerializeField] private float baseY = 261f;          // vertical position of the row
    [SerializeField] private float selectedScale = 0.52f; // scale of the centered dart
    [SerializeField] private float sideScale = 0.28f;     // scale of the neighbours
    [SerializeField] private float scrollSpeed = 12f;     // higher = snappier slide / snap-back
    [SerializeField] private float visibleRange = 3f;     // darts further than this are hidden

    private Transform dartsRoot;
    private RectTransform dartsRect;
    private Transform[] darts;
    private Image[] dartImages;
    private TMP_Text nameLabel;
    private TMP_Text priceLabel;
    private TMP_Text coinsLabel;
    private Button buyButton;
    private TMP_Text buyLabel;
    private int index;
    private float scroll;       // animated, fractional selection position
    private bool dragging;
    private float lastLocalX;   // pointer x (carousel-local) from the previous drag frame

    void Awake() { EnsureInit(); }

    void OnEnable()
    {
        EnsureInit();
        dragging = false;
        scroll = index; // open centered, no slide
        Refresh();
        Layout();
    }

    void Update()
    {
        if (darts == null || darts.Length == 0) return;
        // Ease the row toward the selected index while not being dragged
        // (unscaled time so it animates even when the game is paused).
        if (!dragging)
        {
            float k = 1f - Mathf.Exp(-scrollSpeed * Time.unscaledDeltaTime);
            scroll = Mathf.Lerp(scroll, index, k);
            if (Mathf.Abs(scroll - index) < 0.0005f) scroll = index;
        }
        Layout();
    }

    // ----- Swipe / drag -----

    public void OnBeginDrag(PointerEventData e)
    {
        if (darts == null || darts.Length == 0) return;
        dragging = true;
        lastLocalX = LocalX(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!dragging) return;
        float x = LocalX(e);
        float delta = x - lastLocalX;
        lastLocalX = x;

        // Dragging the content right (delta > 0) moves toward earlier darts.
        scroll = Mathf.Clamp(scroll - delta / Mathf.Max(1f, spacing), 0f, darts.Length - 1);

        // Keep the labels following the dart currently closest to centre.
        int nearest = Mathf.Clamp(Mathf.RoundToInt(scroll), 0, darts.Length - 1);
        if (nearest != index) { index = nearest; Refresh(); }
        Layout();
    }

    public void OnEndDrag(PointerEventData e)
    {
        dragging = false;
        if (darts != null && darts.Length > 0)
        {
            index = Mathf.Clamp(Mathf.RoundToInt(scroll), 0, darts.Length - 1);
            Refresh(); // Update() now eases scroll -> index, snapping to centre.
        }
    }

    float LocalX(PointerEventData e)
    {
        var rect = dartsRect != null ? dartsRect : (RectTransform)transform;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out local);
        return local.x;
    }

    /// <summary>
    /// Builds the cached references once. Safe to call repeatedly and after a domain
    /// reload (which nulls private fields without re-running Awake).
    /// </summary>
    void EnsureInit()
    {
        if (darts != null) return;

        if (!SaveData.Initialized)
        {
            SaveData.Initialized = true;
            SaveData.Coins = startingCoins;
        }

        dartsRoot = transform.Find("DARTS");
        dartsRect = dartsRoot as RectTransform;
        int n = dartsRoot != null ? dartsRoot.childCount : 0;
        darts = new Transform[n];
        dartImages = new Image[n];
        for (int i = 0; i < n; i++)
        {
            darts[i] = dartsRoot.GetChild(i);
            dartImages[i] = darts[i].GetComponent<Image>();
            var rt = darts[i] as RectTransform;
            if (rt != null) rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            if (dartImages[i] != null) dartImages[i].raycastTarget = false;
        }

        // The panel background must be a raycast target so swipes are captured
        // anywhere on the shop (buttons still receive taps since they don't drag).
        var bg = GetComponent<Image>();
        if (bg != null) bg.raycastTarget = true;

        nameLabel  = FindText("Dart Name");
        priceLabel = FindText("Price");
        coinsLabel = FindText("Coins");

        var buyT = transform.Find("Buy");
        if (buyT != null)
        {
            buyButton = buyT.GetComponent<Button>();
            buyLabel = buyT.GetComponentInChildren<TMP_Text>();
        }

        BindButton("Left", OnLeft);
        BindButton("Right", OnRight);
        BindButton("Exit", OnExit);
        if (buyButton != null) { buyButton.onClick.RemoveAllListeners(); buyButton.onClick.AddListener(OnBuy); }

        index = n > 0 ? Mathf.Clamp(SaveData.EquippedDart, 0, n - 1) : 0;
        scroll = index;
    }

    void OnLeft()  { if (darts == null || darts.Length == 0) return; index = Mathf.Max(0, index - 1); Refresh(); }
    void OnRight() { if (darts == null || darts.Length == 0) return; index = Mathf.Min(darts.Length - 1, index + 1); Refresh(); }
    void OnExit()  { if (UIManager.Instance != null) UIManager.Instance.OpenMainMenu(); }

    void OnBuy()
    {
        if (darts == null || darts.Length == 0) return;
        if (SaveData.IsOwned(index))
        {
            SaveData.EquippedDart = index;
        }
        else
        {
            int price = PriceOf(index);
            if (SaveData.Coins >= price)
            {
                SaveData.Coins -= price;
                SaveData.SetOwned(index);
                SaveData.EquippedDart = index;
            }
        }
        Refresh();
    }

    /// <summary>Positions/scales/fades every dart based on the animated scroll value.</summary>
    void Layout()
    {
        // Runtime-only: SortByDepth reorders siblings for draw order, and the dart
        // identity/index mapping is read from sibling order at init — so never let
        // this run (and risk being saved) in edit mode.
        if (!Application.isPlaying) return;
        if (darts == null) return;
        for (int i = 0; i < darts.Length; i++)
        {
            var rt = darts[i] as RectTransform;
            if (rt == null) continue;

            float offset = i - scroll;
            float ad = Mathf.Abs(offset);
            bool visible = ad <= visibleRange;
            if (rt.gameObject.activeSelf != visible) rt.gameObject.SetActive(visible);
            if (!visible) continue;

            rt.anchoredPosition = new Vector2(offset * spacing, baseY);

            float t = Mathf.Clamp01(1f - ad);                  // 1 at centre, 0 at the first neighbour
            float s = Mathf.Lerp(sideScale, selectedScale, t);
            if (ad > 1f) s *= Mathf.Lerp(1f, 0.75f, Mathf.Clamp01(ad - 1f)); // shrink distant darts a touch more
            rt.localScale = new Vector3(s, s, s);

            var img = dartImages[i];
            if (img != null)
            {
                var c = img.color;
                c.a = Mathf.Clamp01(1f - 0.42f * ad);
                img.color = c;
            }
        }
        SortByDepth();
    }

    /// <summary>Draw order so the dart nearest the centre renders on top.</summary>
    void SortByDepth()
    {
        for (int i = 0; i < darts.Length; i++)
        {
            if (darts[i] == null || !darts[i].gameObject.activeSelf) continue;
            float adI = Mathf.Abs(i - scroll);
            int siblingTarget = 0;
            for (int j = 0; j < darts.Length; j++)
            {
                if (j == i || darts[j] == null || !darts[j].gameObject.activeSelf) continue;
                if (Mathf.Abs(j - scroll) > adI) siblingTarget++;
            }
            darts[i].SetSiblingIndex(siblingTarget);
        }
    }

    void Refresh()
    {
        bool owned = SaveData.IsOwned(index);
        bool equipped = owned && SaveData.EquippedDart == index;
        int price = PriceOf(index);

        if (nameLabel != null)  nameLabel.text = NameOf(index);
        if (coinsLabel != null) coinsLabel.text = "Coins: " + SaveData.Coins;
        if (priceLabel != null) priceLabel.text = owned ? "Owned" : price.ToString();
        if (buyLabel != null)   buyLabel.text = equipped ? "Equipped" : (owned ? "Equip" : "Buy");
        if (buyButton != null)  buyButton.interactable = !equipped && (owned || SaveData.Coins >= price);
    }

    int PriceOf(int i) { return (prices != null && i < prices.Length) ? prices[i] : 0; }
    string NameOf(int i) { return (displayNames != null && i < displayNames.Length) ? displayNames[i] : ("Dart " + (i + 1)); }

    TMP_Text FindText(string n)
    {
        var t = transform.Find(n);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    void BindButton(string n, UnityEngine.Events.UnityAction a)
    {
        var t = transform.Find(n);
        var b = t != null ? t.GetComponent<Button>() : null;
        if (b != null) { b.onClick.RemoveAllListeners(); b.onClick.AddListener(a); }
    }
}
