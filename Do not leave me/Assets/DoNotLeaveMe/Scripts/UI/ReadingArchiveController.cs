using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("DoNotLeaveMe/Reading Archive Controller")]
// 档案列表和页面阅读器共用同一个 B 面板，不再打开外层 TutorialPopup。
public class ReadingArchiveController : MonoBehaviour
{
    public static bool IsShowing { get; private set; }

    private GameObject root;
    private RectTransform content;
    private Text emptyText;
    private Button storyTab;
    private Button controlsTab;
    private Text readerTitle;
    private Image readerImage;
    private Text readerPrompt;
    private Text pageLabel;
    private Button previousButton;
    private Button nextButton;
    private ReadingArchiveCategory category = ReadingArchiveCategory.Story;
    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private Sprite[] currentPages;
    private int currentPageIndex;
    private float previousTimeScale = 1f;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLock;
    private GameObject hudRoot;
    private bool hudWasActive;

    void Awake()
    {
        IsShowing = false;
        BuildUi();
        ReadingArchiveProgress.MigrateLegacyTutorials();
    }

    void OnDestroy()
    {
        if (IsShowing)
            RestoreGameplayState();
        IsShowing = false;
    }

    void Update()
    {
        if (!IsShowing)
        {
            if (Input.GetKeyDown(KeyCode.B) && CanOpen())
                Open();
            return;
        }

        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            PreviousPage();
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            NextPage();
    }

    bool CanOpen()
    {
        if (TutorialPopup.IsShowing || DeathScreen.IsShowing || PauseMenu.IsPaused)
            return false;
        PlayerActors actors = PlayerActors.Instance;
        return actors != null && actors.Human != null && ReadingArchiveCatalog.Instance != null;
    }

    public void Open()
    {
        if (IsShowing || root == null)
            return;

        IsShowing = true;
        previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        previousCursorVisible = Cursor.visible;
        previousCursorLock = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PauseMenu pauseMenu = GetComponent<PauseMenu>();
        hudRoot = pauseMenu != null ? pauseMenu.hudRoot : null;
        hudWasActive = hudRoot != null && hudRoot.activeSelf;
        if (hudRoot != null)
            hudRoot.SetActive(false);

        root.transform.SetAsLastSibling();
        root.SetActive(true);
        ClearReader();
        RefreshList();
        ParchmentAudio.PlayOpen();
    }

    public void Close()
    {
        if (!IsShowing)
            return;
        RestoreGameplayState();
        IsShowing = false;
        root.SetActive(false);
        ParchmentAudio.PlayClose();
    }

    void RestoreGameplayState()
    {
        Time.timeScale = previousTimeScale;
        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLock;
        if (hudRoot != null)
            hudRoot.SetActive(hudWasActive);
    }

    ReadingArchiveRole CurrentRole()
    {
        PlayerControl control = FindObjectOfType<PlayerControl>();
        return control != null && control.IsDogActive ? ReadingArchiveRole.Dog : ReadingArchiveRole.Human;
    }

    void SelectCategory(ReadingArchiveCategory value)
    {
        category = value;
        ClearReader();
        RefreshList();
    }

    void RefreshList()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
            Destroy(spawnedButtons[i]);
        spawnedButtons.Clear();

        ReadingArchiveCatalog catalog = ReadingArchiveCatalog.Instance;
        ReadingArchiveRole role = CurrentRole();
        int count = 0;
        if (catalog != null)
        {
            foreach (ReadingArchiveCatalog.Entry entry in catalog.Entries)
            {
                if (entry == null || entry.category != category ||
                    !ReadingArchiveProgress.IsDiscovered(entry.id, role))
                    continue;
                CreateEntryButton(entry, role);
                count++;
            }
        }

        emptyText.gameObject.SetActive(count == 0);
        storyTab.image.color = category == ReadingArchiveCategory.Story
            ? new Color(0.78f, 0.58f, 0.28f) : new Color(0.35f, 0.24f, 0.15f);
        controlsTab.image.color = category == ReadingArchiveCategory.Controls
            ? new Color(0.78f, 0.58f, 0.28f) : new Color(0.35f, 0.24f, 0.15f);
    }

    void CreateEntryButton(ReadingArchiveCatalog.Entry entry, ReadingArchiveRole role)
    {
        Button button = MakeButton(content, entry.title, new Vector2(0f, 58f), 21);
        button.onClick.AddListener(() => SelectEntry(entry, role));
        spawnedButtons.Add(button.gameObject);
    }

    void SelectEntry(ReadingArchiveCatalog.Entry entry, ReadingArchiveRole role)
    {
        Sprite[] source = entry.PagesFor(role);
        var validPages = new List<Sprite>();
        if (source != null)
            for (int i = 0; i < source.Length; i++)
                if (source[i] != null)
                    validPages.Add(source[i]);

        currentPages = validPages.ToArray();
        currentPageIndex = 0;
        readerTitle.text = entry.title;
        RefreshReader();
    }

    void ClearReader()
    {
        currentPages = null;
        currentPageIndex = 0;
        readerTitle.text = category == ReadingArchiveCategory.Story ? "剧情资料" : "操作提示";
        readerImage.sprite = null;
        readerImage.gameObject.SetActive(false);
        readerPrompt.gameObject.SetActive(true);
        readerPrompt.text = "请从左侧选择一条内容";
        pageLabel.text = string.Empty;
        previousButton.interactable = false;
        nextButton.interactable = false;
    }

    void RefreshReader()
    {
        if (currentPages == null || currentPages.Length == 0)
        {
            readerImage.gameObject.SetActive(false);
            readerPrompt.gameObject.SetActive(true);
            readerPrompt.text = "这条内容没有可显示的页面";
            pageLabel.text = string.Empty;
            previousButton.interactable = false;
            nextButton.interactable = false;
            return;
        }

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, currentPages.Length - 1);
        readerPrompt.gameObject.SetActive(false);
        readerImage.gameObject.SetActive(true);
        readerImage.sprite = currentPages[currentPageIndex];
        pageLabel.text = (currentPageIndex + 1) + " / " + currentPages.Length;
        previousButton.interactable = currentPageIndex > 0;
        nextButton.interactable = currentPageIndex < currentPages.Length - 1;
    }

    public void PreviousPage()
    {
        if (currentPages == null || currentPageIndex <= 0)
            return;
        currentPageIndex--;
        RefreshReader();
    }

    public void NextPage()
    {
        if (currentPages == null || currentPageIndex >= currentPages.Length - 1)
            return;
        currentPageIndex++;
        RefreshReader();
    }

    void BuildUi()
    {
        Canvas canvas = GetComponent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform;
        root = MakeObject("ReadingArchive", parent, typeof(Image));
        Stretch(root.GetComponent<RectTransform>());
        root.GetComponent<Image>().color = new Color(0.025f, 0.015f, 0.01f, 0.88f);

        GameObject panel = MakeObject("ParchmentPanel", root.transform, typeof(Image));
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.08f, 0.07f), new Vector2(0.92f, 0.93f));
        panel.GetComponent<Image>().color = new Color(0.22f, 0.14f, 0.08f, 0.99f);

        Text title = MakeText(panel.transform, "阅读档案", 38, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.05f, 0.89f), new Vector2(0.95f, 0.98f));

        storyTab = MakeButton(panel.transform, "剧情资料", new Vector2(180f, 54f), 23);
        SetRect(storyTab.GetComponent<RectTransform>(), new Vector2(0.04f, 0.80f), new Vector2(0.19f, 0.87f));
        storyTab.onClick.AddListener(() => SelectCategory(ReadingArchiveCategory.Story));
        controlsTab = MakeButton(panel.transform, "操作提示", new Vector2(180f, 54f), 23);
        SetRect(controlsTab.GetComponent<RectTransform>(), new Vector2(0.21f, 0.80f), new Vector2(0.36f, 0.87f));
        controlsTab.onClick.AddListener(() => SelectCategory(ReadingArchiveCategory.Controls));

        GameObject viewport = MakeObject("EntryViewport", panel.transform, typeof(Image), typeof(Mask), typeof(ScrollRect));
        SetRect(viewport.GetComponent<RectTransform>(), new Vector2(0.035f, 0.14f), new Vector2(0.37f, 0.77f));
        viewport.GetComponent<Image>().color = new Color(0.075f, 0.04f, 0.018f, 0.72f);
        viewport.GetComponent<Mask>().showMaskGraphic = true;

        GameObject contentObject = MakeObject("EntryContent", viewport.transform,
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = content.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 8f;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ScrollRect scroll = viewport.GetComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 32f;

        emptyText = MakeText(viewport.transform, "尚未阅读任何内容", 22, TextAnchor.MiddleCenter);
        Stretch(emptyText.rectTransform);

        GameObject readerPanel = MakeObject("EmbeddedReader", panel.transform, typeof(Image));
        SetRect(readerPanel.GetComponent<RectTransform>(), new Vector2(0.40f, 0.14f), new Vector2(0.965f, 0.87f));
        readerPanel.GetComponent<Image>().color = new Color(0.08f, 0.045f, 0.022f, 0.78f);

        readerTitle = MakeText(readerPanel.transform, "剧情资料", 28, TextAnchor.MiddleCenter);
        SetRect(readerTitle.rectTransform, new Vector2(0.05f, 0.89f), new Vector2(0.95f, 0.98f));

        GameObject imageFrame = MakeObject("PageFrame", readerPanel.transform, typeof(Image));
        SetRect(imageFrame.GetComponent<RectTransform>(), new Vector2(0.05f, 0.16f), new Vector2(0.95f, 0.87f));
        imageFrame.GetComponent<Image>().color = new Color(0.025f, 0.015f, 0.01f, 0.92f);
        GameObject imageObject = MakeObject("PageImage", imageFrame.transform, typeof(Image));
        SetRect(imageObject.GetComponent<RectTransform>(), new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.975f));
        readerImage = imageObject.GetComponent<Image>();
        readerImage.color = Color.white;
        readerImage.preserveAspect = true;
        readerImage.raycastTarget = false;

        readerPrompt = MakeText(imageFrame.transform, "请从左侧选择一条内容", 25, TextAnchor.MiddleCenter);
        Stretch(readerPrompt.rectTransform);

        previousButton = MakeButton(readerPanel.transform, "← 上一页", new Vector2(150f, 48f), 21);
        SetRect(previousButton.GetComponent<RectTransform>(), new Vector2(0.05f, 0.045f), new Vector2(0.28f, 0.13f));
        previousButton.onClick.AddListener(PreviousPage);
        nextButton = MakeButton(readerPanel.transform, "下一页 →", new Vector2(150f, 48f), 21);
        SetRect(nextButton.GetComponent<RectTransform>(), new Vector2(0.72f, 0.045f), new Vector2(0.95f, 0.13f));
        nextButton.onClick.AddListener(NextPage);
        pageLabel = MakeText(readerPanel.transform, string.Empty, 21, TextAnchor.MiddleCenter);
        SetRect(pageLabel.rectTransform, new Vector2(0.36f, 0.045f), new Vector2(0.64f, 0.13f));

        Button close = MakeButton(panel.transform, "关闭  B / Esc", new Vector2(250f, 52f), 22);
        SetRect(close.GetComponent<RectTransform>(), new Vector2(0.40f, 0.035f), new Vector2(0.60f, 0.105f));
        close.onClick.AddListener(Close);

        ClearReader();
        root.SetActive(false);
    }

    static GameObject MakeObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        for (int i = 0; i < components.Length; i++)
            if (components[i] != typeof(RectTransform))
                go.AddComponent(components[i]);
        return go;
    }

    static Text MakeText(Transform parent, string value, int size, TextAnchor anchor)
    {
        GameObject go = MakeObject("Text", parent, typeof(Text));
        Text text = go.GetComponent<Text>();
        try { text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
        catch (System.Exception) { text.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        text.text = value;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = new Color(0.95f, 0.84f, 0.62f);
        return text;
    }

    static Button MakeButton(Transform parent, string label, Vector2 preferredSize, int fontSize)
    {
        GameObject go = MakeObject(label, parent, typeof(Image), typeof(Button), typeof(LayoutElement));
        go.GetComponent<Image>().color = new Color(0.35f, 0.24f, 0.15f);
        LayoutElement element = go.GetComponent<LayoutElement>();
        element.preferredHeight = preferredSize.y;
        element.preferredWidth = preferredSize.x;
        Text text = MakeText(go.transform, label, fontSize, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
        return go.GetComponent<Button>();
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
