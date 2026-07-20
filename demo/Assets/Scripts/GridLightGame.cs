using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class GridLightGame : MonoBehaviour
{
    private const int MapMin = 20;
    private const int MapMax = 30;

    private System.Random _rng;
    private int _currentLevel = 1;

    private GridLevelData _level;
    private readonly HashSet<GridPos> _playerLights = new HashSet<GridPos>();
    private GridLightBoard3D _board3D;
    private bool _isMobile;
    private bool _started;

    private GameObject _mainMenuPanel;
    private GameObject _settingsPanel;
    private GameObject _settingsConfirmMask;
    private GameObject _gameplayPanel;
    private GameObject _winMask;
    private RectTransform _settingsButtonRect;
    private RectTransform _settingsCardRect;
    private RectTransform _settingsConfirmCardRect;
    private RectTransform _winCardRect;
    private RectTransform _headerRect;
    private RectTransform _sidePanelRect;
    private Canvas _uiCanvas;
    private Text _menuLevelText;
    private Text _settingsCurrentProfileText;
    private Text _settingsCurrentAdNetworkText;
    private Text _settingsCurrentPlacementText;
    private Text _settingsConfirmBodyText;
    private Text _levelTitleText;
    private Text _statusText;
    private Text _statsText;
    private Text _adStatusText;
    private Text _winBodyText;
    private readonly Dictionary<string, Button> _profileButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Button> _adNetworkButtons = new Dictionary<string, Button>();
    private Button _placementDropdownButton;
    private Text _placementDropdownLabel;
    private GameObject _placementDropdownList;
    private ScrollRect _settingsScrollRect;
    private Vector2 _settingsCardDefaultSize;
    private bool _settingsKeyboardLifted;
    private GridLightAppProfile _pendingProfile;
    private int _lastScreenWidth;
    private int _lastScreenHeight;
    private Rect _lastSafeArea;
    private float _headerContentHeight;
    private float _sidePanelHeight;
    private float _sidePanelWidth;
    private static Sprite _roundedSprite;
    private static Sprite _settingsIconSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<GridLightGame>() != null) return;
        var go = new GameObject("GridLightGame");
        go.AddComponent<GridLightGame>();
    }

    private void Start()
    {
        if (_started) return;
        _started = true;

        _isMobile = Application.isMobilePlatform;
        GridLightTheme.ApplySceneLighting();

        var cam = Camera.main ?? FindFirstObjectByType<Camera>();
        _board3D = gameObject.AddComponent<GridLightBoard3D>();
        _board3D.Initialize(cam, _isMobile);
        _board3D.CellClicked += ToggleLight;

        BuildUi();
        ShowMainMenu();
    }

    private void Update()
    {
        UpdateSettingsKeyboardAvoidance();

        if (_uiCanvas == null) return;
        if (_lastScreenWidth == Screen.width &&
            _lastScreenHeight == Screen.height &&
            _lastSafeArea == Screen.safeArea)
        {
            return;
        }

        ApplyGameplayLayout();
    }

    private void BuildUi()
    {
        EnsureEventSystem();

        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _uiCanvas = canvasGo.GetComponent<Canvas>();
        _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = _isMobile ? new Vector2(1080, 1920) : new Vector2(1280, 720);
        scaler.matchWidthOrHeight = _isMobile ? 0f : 0.5f;

        // Root must stay transparent so the 3D board shows through during gameplay.
        var root = CreatePanel(canvasGo.transform, "Root", new Color(0f, 0f, 0f, 0f), false);
        Stretch(root.GetComponent<RectTransform>());

        BuildMainMenu(root.transform);
        BuildSettingsScreen(root.transform);
        BuildSettingsConfirmation(root.transform);
        BuildGameplayUi(root.transform);
        BuildWinScreen(root.transform);
        ApplyGameplayLayout();
    }

    private void BuildMainMenu(Transform root)
    {
        _mainMenuPanel = CreatePanel(root, "MainMenu", GridLightTheme.BgWarm, false);
        Stretch(_mainMenuPanel.GetComponent<RectTransform>());
        GridLightMenuBackdrop.Create(_mainMenuPanel.transform);

        var card = CreatePanel(_mainMenuPanel.transform, "MenuCard", GridLightTheme.Panel, true, true);
        var cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(580f, 500f);

        var v = card.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(44, 44, 48, 44);
        v.spacing = 22;
        v.childAlignment = TextAnchor.MiddleCenter;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = false;
        v.childForceExpandHeight = false;

        var title = CreateText(card.transform, "GameTitle", "Grid Light", 68, FontStyle.Bold, TextAnchor.MiddleCenter, 0f, 0f, 78f);
        title.GetComponent<LayoutElement>().preferredWidth = 480f;
        var subtitle = CreateText(card.transform, "Subtitle", "Light Fill", 36, FontStyle.Normal, TextAnchor.MiddleCenter, 0f, 0f, 48f);
        subtitle.color = GridLightTheme.TextMuted;
        subtitle.GetComponent<LayoutElement>().preferredWidth = 480f;
        _menuLevelText = CreateText(card.transform, "MenuLevel", "Progress: Level 1", 30, FontStyle.Normal, TextAnchor.MiddleCenter, 0f, 0f, 52f);
        _menuLevelText.color = GridLightTheme.TextMuted;
        _menuLevelText.GetComponent<LayoutElement>().preferredWidth = 480f;
        CreateButton(card.transform, "Start Game", StartFromMenu, GridLightTheme.Accent, 280f, 76f, 28);

        var settingsButton = CreateSettingsIconButton(_mainMenuPanel.transform, ShowSettings);
        _settingsButtonRect = settingsButton.GetComponent<RectTransform>();
        _settingsButtonRect.anchorMin = _settingsButtonRect.anchorMax = new Vector2(1f, 1f);
        _settingsButtonRect.pivot = new Vector2(1f, 1f);
        _settingsButtonRect.sizeDelta = new Vector2(68f, 68f);
    }

    private void BuildSettingsScreen(Transform root)
    {
        _settingsPanel = CreatePanel(root, "Settings", GridLightTheme.BgWarm, false);
        Stretch(_settingsPanel.GetComponent<RectTransform>());
        GridLightMenuBackdrop.Create(_settingsPanel.transform);

        var card = CreatePanel(_settingsPanel.transform, "SettingsCard", GridLightTheme.Panel, true, true);
        _settingsCardRect = card.GetComponent<RectTransform>();
        _settingsCardRect.anchorMin = _settingsCardRect.anchorMax = new Vector2(0.5f, 0.5f);
        _settingsCardDefaultSize = new Vector2(620f, _isMobile ? 980f : 760f);
        _settingsCardRect.sizeDelta = _settingsCardDefaultSize;

        var viewportGo = new GameObject("SettingsViewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
        viewportGo.transform.SetParent(card.transform, false);
        var viewportRect = viewportGo.GetComponent<RectTransform>();
        Stretch(viewportRect);
        viewportRect.offsetMin = new Vector2(20f, 18f);
        viewportRect.offsetMax = new Vector2(-20f, -18f);
        var viewportImage = viewportGo.GetComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
        viewportImage.raycastTarget = true;

        var contentGo = new GameObject("SettingsContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(viewportGo.transform, false);
        var contentRect = contentGo.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        var layout = contentGo.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 160);
        layout.spacing = 14;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = contentGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _settingsScrollRect = viewportGo.GetComponent<ScrollRect>();
        _settingsScrollRect.viewport = viewportRect;
        _settingsScrollRect.content = contentRect;
        _settingsScrollRect.horizontal = false;
        _settingsScrollRect.vertical = true;
        _settingsScrollRect.movementType = ScrollRect.MovementType.Clamped;
        _settingsScrollRect.scrollSensitivity = 28f;

        var title = CreateText(contentGo.transform, "SettingsTitle", "Settings", 48, FontStyle.Bold, TextAnchor.MiddleCenter, 0f, 0f, 64f);
        title.GetComponent<LayoutElement>().preferredWidth = 520f;

        _settingsCurrentProfileText = CreateText(contentGo.transform, "CurrentProfile", "", 24, FontStyle.Bold, TextAnchor.MiddleCenter, 0f, 0f, 72f);
        _settingsCurrentProfileText.color = GridLightTheme.TextMuted;
        _settingsCurrentProfileText.GetComponent<LayoutElement>().preferredWidth = 520f;

        var hint = CreateText(contentGo.transform, "ProfileHint", "Choose the MSP demo profile used on the next launch.", 22, FontStyle.Normal, TextAnchor.MiddleCenter, 0f, 0f, 48f);
        hint.color = GridLightTheme.TextMuted;
        hint.GetComponent<LayoutElement>().preferredWidth = 520f;

        foreach (var profile in GridLightAppProfiles.All)
        {
            var selectedProfile = profile;
            var button = CreateButton(
                contentGo.transform,
                profile.DisplayName,
                () => OnProfileSelected(selectedProfile),
                GridLightTheme.PanelDeep,
                520f,
                68f,
                26);
            button.gameObject.name = $"Btn_Profile_{profile.Id}";
            _profileButtons[profile.Id] = button;
        }

        var adNetworkTitle = CreateText(contentGo.transform, "AdNetworkTitle", "Ad_Network", 30, FontStyle.Bold, TextAnchor.MiddleCenter, 0f, 0f, 48f);
        adNetworkTitle.GetComponent<LayoutElement>().preferredWidth = 520f;

        _settingsCurrentAdNetworkText = CreateText(contentGo.transform, "CurrentAdNetwork", "", 24, FontStyle.Normal, TextAnchor.MiddleCenter, 0f, 0f, 40f);
        _settingsCurrentAdNetworkText.color = GridLightTheme.TextMuted;
        _settingsCurrentAdNetworkText.GetComponent<LayoutElement>().preferredWidth = 520f;

        foreach (var adNetwork in GridLightAppProfiles.AdNetworks)
        {
            var selectedNetwork = adNetwork;
            var button = CreateButton(
                contentGo.transform,
                adNetwork,
                () => OnAdNetworkSelected(selectedNetwork),
                GridLightTheme.PanelDeep,
                520f,
                58f,
                22);
            button.gameObject.name = $"Btn_AdNetwork_{adNetwork}";
            _adNetworkButtons[adNetwork] = button;
        }

        var placementTitle = CreateText(contentGo.transform, "PlacementTitle", "Placement", 30, FontStyle.Bold, TextAnchor.MiddleCenter, 0f, 0f, 48f);
        placementTitle.GetComponent<LayoutElement>().preferredWidth = 520f;

        _settingsCurrentPlacementText = CreateText(contentGo.transform, "CurrentPlacement", "", 22, FontStyle.Normal, TextAnchor.MiddleCenter, 0f, 0f, 40f);
        _settingsCurrentPlacementText.color = GridLightTheme.TextMuted;
        _settingsCurrentPlacementText.GetComponent<LayoutElement>().preferredWidth = 520f;

        CreatePlacementDropdown(contentGo.transform);

        CreateButton(contentGo.transform, "Back", ShowMainMenu, GridLightTheme.AccentSoft, 220f, 60f, 24);
        _settingsPanel.SetActive(false);
    }

    private void BuildSettingsConfirmation(Transform root)
    {
        _settingsConfirmMask = CreatePanel(root, "SettingsConfirmMask", GridLightTheme.Overlay);
        Stretch(_settingsConfirmMask.GetComponent<RectTransform>());

        var card = CreatePanel(_settingsConfirmMask.transform, "SettingsConfirmCard", GridLightTheme.Panel, true, true);
        _settingsConfirmCardRect = card.GetComponent<RectTransform>();
        SetAnchors(
            _settingsConfirmCardRect,
            0.5f,
            0.5f,
            0.5f,
            0.5f,
            -300f,
            _isMobile ? -220f : -180f,
            300f,
            _isMobile ? 220f : 180f);

        var title = CreateText(card.transform, "ConfirmTitle", "Switch AppProfile?", 38, FontStyle.Bold, TextAnchor.UpperCenter, 0f, 0f, 54f);
        StretchTop(title.GetComponent<RectTransform>(), 34f);

        _settingsConfirmBodyText = CreateText(card.transform, "ConfirmBody", "", 25, FontStyle.Normal, TextAnchor.MiddleCenter, 0f, 0f, 140f);
        var bodyRect = _settingsConfirmBodyText.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0.5f);
        bodyRect.anchorMax = new Vector2(1f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.sizeDelta = new Vector2(-64f, 150f);
        bodyRect.anchoredPosition = new Vector2(0f, 8f);
        _settingsConfirmBodyText.color = GridLightTheme.TextMuted;
        UnityEngine.Object.Destroy(_settingsConfirmBodyText.GetComponent<LayoutElement>());

        var buttonRow = new GameObject("ConfirmButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonRow.transform.SetParent(card.transform, false);
        var rowRect = buttonRow.GetComponent<RectTransform>();
        rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 0f);
        rowRect.pivot = new Vector2(0.5f, 0f);
        rowRect.sizeDelta = new Vector2(500f, 64f);
        rowRect.anchoredPosition = new Vector2(0f, 30f);

        var rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 20;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = true;

        CreateButton(buttonRow.transform, "Cancel", HideProfileConfirmation, GridLightTheme.PanelDeep, 190f, 62f, 23);
        CreateButton(buttonRow.transform, "Switch & Relaunch", ConfirmProfileChange, GridLightTheme.Accent, 250f, 62f, 23);

        _settingsConfirmMask.SetActive(false);
    }

    private void ShowSettings()
    {
        HideWin();
        _mainMenuPanel.SetActive(false);
        _gameplayPanel.SetActive(false);
        _settingsPanel.SetActive(true);
        _board3D.SetBoardVisible(false);
        _board3D.SetViewportMode(false);
        if (_placementDropdownList != null)
            _placementDropdownList.SetActive(false);
        RestoreSettingsCardLayout();
        RefreshSettingsSelection();
        ApplyGameplayLayout();
    }

    private void OnProfileSelected(GridLightAppProfile profile)
    {
        if (profile == null || profile.Id == GridLightAppProfiles.Current.Id) return;

        _pendingProfile = profile;
        _settingsConfirmBodyText.text =
            $"Switch to {profile.DisplayName}?\n\n" +
            "The app will relaunch after saving to apply the new profile credentials.";
        _settingsConfirmMask.SetActive(true);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_settingsConfirmCardRect);
    }

    private void HideProfileConfirmation()
    {
        _pendingProfile = null;
        _settingsConfirmMask.SetActive(false);
    }

    private void ConfirmProfileChange()
    {
        if (_pendingProfile == null)
        {
            HideProfileConfirmation();
            return;
        }

        var selectedProfile = _pendingProfile;
        _pendingProfile = null;
        GridLightAppProfiles.Save(selectedProfile);
        RefreshSettingsSelection();
        _settingsConfirmMask.SetActive(false);
        GridLightAppRestart.CloseAfterProfileChange();
    }

    private void OnAdNetworkSelected(string adNetwork)
    {
        if (string.Equals(adNetwork, GridLightAppProfiles.CurrentAdNetwork, StringComparison.Ordinal))
            return;

        GridLightAppProfiles.SaveAdNetwork(adNetwork);
        RefreshSettingsSelection();
    }

    private void RefreshSettingsSelection()
    {
        var current = GridLightAppProfiles.Current;
        _settingsCurrentProfileText.text =
            $"Current profile: {current.DisplayName}\nAPI Key: {GridLightAppProfiles.CurrentPrebidApiKey}";

        foreach (var profile in GridLightAppProfiles.All)
        {
            if (!_profileButtons.TryGetValue(profile.Id, out var button)) continue;

            var selected = profile.Id == current.Id;
            button.image.color = selected ? GridLightTheme.AccentSoft : GridLightTheme.PanelDeep;
            var label = button.transform.Find("Label")?.GetComponent<Text>();
            if (label != null)
                label.text = selected ? $"✓  {profile.DisplayName}" : profile.DisplayName;
        }

        var selectedAdNetwork = GridLightAppProfiles.CurrentAdNetwork;
        _settingsCurrentAdNetworkText.text = $"Current Ad_Network: {selectedAdNetwork}";

        foreach (var adNetwork in GridLightAppProfiles.AdNetworks)
        {
            if (!_adNetworkButtons.TryGetValue(adNetwork, out var button)) continue;

            var selected = string.Equals(adNetwork, selectedAdNetwork, StringComparison.Ordinal);
            button.image.color = selected ? GridLightTheme.AccentSoft : GridLightTheme.PanelDeep;
            var label = button.transform.Find("Label")?.GetComponent<Text>();
            if (label != null)
                label.text = selected ? $"✓  {adNetwork}" : adNetwork;
        }

        var placement = GridLightAppProfiles.CurrentPlacement;
        _settingsCurrentPlacementText.text = $"Current Placement: {placement}";
        if (_placementDropdownLabel != null)
            _placementDropdownLabel.text = placement;
    }

    private void TogglePlacementDropdown()
    {
        if (_placementDropdownList == null) return;
        _placementDropdownList.SetActive(!_placementDropdownList.activeSelf);
    }

    private void OnPlacementOptionSelected(string placement)
    {
        GridLightAppProfiles.SavePlacement(placement);
        _placementDropdownLabel.text = placement;
        _settingsCurrentPlacementText.text = $"Current Placement: {placement}";
        if (_placementDropdownList != null)
            _placementDropdownList.SetActive(false);
    }

    private static List<string> GetPlacementOptions()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return new List<string>
        {
            "demo-android-interstitial",
            "scoopz-android-launch-prod",
            "scoopz-android-foryou-test-moloco-interstitial",
            "msp-android-launch-fullscreen-interstitial-prod2",
        };
#else
        return new List<string>
        {
            "demo-ios-launch-fullscreen",
            "msp-ios-launch-fullscreen-interstitial-prod2",
            "scoopz-ios-launch-prod",
            "scoopz-ios-launch-test-moloco",
        };
#endif
    }

    private void UpdateSettingsKeyboardAvoidance(bool force = false)
    {
        if (_settingsPanel == null || _settingsCardRect == null || _uiCanvas == null)
            return;

        if (!_settingsPanel.activeInHierarchy)
        {
            if (_settingsKeyboardLifted)
                RestoreSettingsCardLayout();
            return;
        }

        if (_settingsKeyboardLifted || force)
            RestoreSettingsCardLayout();
    }

    private void RestoreSettingsCardLayout()
    {
        _settingsCardRect.anchorMin = _settingsCardRect.anchorMax = new Vector2(0.5f, 0.5f);
        _settingsCardRect.pivot = new Vector2(0.5f, 0.5f);
        _settingsCardRect.sizeDelta = _settingsCardDefaultSize;
        _settingsCardRect.anchoredPosition = Vector2.zero;
        _settingsKeyboardLifted = false;
    }

    private float GetSoftKeyboardHeightUi()
    {
        if (!TouchScreenKeyboard.visible)
            return 0f;

        var area = TouchScreenKeyboard.area;
        var heightPx = area.height;
        if (heightPx < 1f)
            heightPx = Screen.height * (_isMobile ? 0.42f : 0.3f);

        return heightPx / Mathf.Max(0.01f, _uiCanvas.scaleFactor);
    }

    private void ScrollPlacementInputIntoView()
    {
        if (_settingsScrollRect == null || _placementDropdownButton == null)
            return;

        Canvas.ForceUpdateCanvases();

        var content = _settingsScrollRect.content;
        var viewport = _settingsScrollRect.viewport;
        if (content == null || viewport == null)
            return;

        var contentHeight = content.rect.height;
        var viewportHeight = viewport.rect.height;
        if (contentHeight <= viewportHeight + 1f)
        {
            _settingsScrollRect.verticalNormalizedPosition = 1f;
            return;
        }

        var item = _placementDropdownButton.GetComponent<RectTransform>();
        var itemCenterInContent = content.InverseTransformPoint(item.TransformPoint(item.rect.center));
        // Content pivot is top; y decreases downward.
        var distanceFromTop = -itemCenterInContent.y;
        var target = distanceFromTop - viewportHeight * 0.55f;
        var scrollable = contentHeight - viewportHeight;
        var normalized = 1f - Mathf.Clamp01(target / scrollable);
        _settingsScrollRect.verticalNormalizedPosition = normalized;
    }

    private void BuildGameplayUi(Transform root)
    {
        _gameplayPanel = CreatePanel(root, "Gameplay", new Color(0, 0, 0, 0), false);
        Stretch(_gameplayPanel.GetComponent<RectTransform>());
        _gameplayPanel.SetActive(false);

        _headerContentHeight = _isMobile ? 96f : 72f;
        _sidePanelHeight = _isMobile ? 380f : 0f;
        _sidePanelWidth = _isMobile ? 0f : 300f;

        var header = CreatePanel(_gameplayPanel.transform, "Header", GridLightTheme.PanelDeep);
        _headerRect = header.GetComponent<RectTransform>();
        // Temporary; ApplyGameplayLayout sets final offsets including safe-area inset.
        SetAnchors(_headerRect, 0f, 1f, 1f, 1f, 0f, -_headerContentHeight, 0f, 0f);

        _levelTitleText = CreateText(header.transform, "LevelTitle", "Level 1", 30, FontStyle.Bold, TextAnchor.MiddleLeft, 0f, 0f, 48f);
        var titleRt = _levelTitleText.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0f);
        titleRt.anchorMax = new Vector2(0.55f, 1f);
        titleRt.offsetMin = new Vector2(28f, 8f);
        titleRt.offsetMax = new Vector2(0f, -8f);
        titleRt.pivot = new Vector2(0f, 0.5f);

        _statusText = CreateText(header.transform, "Status", "", 22, FontStyle.Normal, TextAnchor.MiddleRight, 0f, 0f, 48f);
        var statusRt = _statusText.GetComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0.45f, 0f);
        statusRt.anchorMax = new Vector2(1f, 1f);
        statusRt.offsetMin = new Vector2(0f, 8f);
        statusRt.offsetMax = new Vector2(-168f, -8f);
        statusRt.pivot = new Vector2(1f, 0.5f);
        _statusText.color = GridLightTheme.TextMuted;

        var backBtn = CreateButton(header.transform, "Home", ShowMainMenu, GridLightTheme.AccentSoft, 128f, 52f, 22);
        var backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(1f, 0f);
        backRect.anchorMax = new Vector2(1f, 0f);
        backRect.pivot = new Vector2(1f, 0f);
        backRect.sizeDelta = new Vector2(128f, 52f);
        backRect.anchoredPosition = new Vector2(-24f, 12f);
        UnityEngine.Object.Destroy(backBtn.GetComponent<LayoutElement>());

        var side = CreatePanel(_gameplayPanel.transform, "SidePanel", GridLightTheme.Panel, true, true);
        _sidePanelRect = side.GetComponent<RectTransform>();

        var vLayout = side.AddComponent<VerticalLayoutGroup>();
        vLayout.padding = new RectOffset(24, 24, 20, 20);
        vLayout.spacing = 12;
        vLayout.childAlignment = TextAnchor.UpperCenter;
        vLayout.childControlHeight = true;
        vLayout.childControlWidth = true;
        vLayout.childForceExpandHeight = false;
        vLayout.childForceExpandWidth = false;

        var contentWidth = _isMobile ? 640f : 252f;
        var hint = CreateText(side.transform, "Hint", "Tap cells to place lamps and light every square.", 22, FontStyle.Normal, TextAnchor.UpperLeft, 0f, 0f, 64f);
        hint.color = GridLightTheme.TextMuted;
        hint.GetComponent<LayoutElement>().preferredWidth = contentWidth;
        _statsText = CreateText(side.transform, "StatsBody", "-", 22, FontStyle.Normal, TextAnchor.UpperLeft, 0f, 0f, 140f);
        _statsText.GetComponent<LayoutElement>().preferredWidth = contentWidth;
        _adStatusText = CreateText(side.transform, "AdStatus", "Ad: —", 20, FontStyle.Normal, TextAnchor.UpperLeft, 0f, 0f, 64f);
        _adStatusText.color = GridLightTheme.TextMuted;
        _adStatusText.GetComponent<LayoutElement>().preferredWidth = contentWidth;
        CreateButton(side.transform, "Reset Level", ResetLevel, GridLightTheme.PanelDeep, 220f, 56f, 22);
        CreateButton(side.transform, "Show Answer", ShowAnswer, GridLightTheme.AccentSoft, 220f, 56f, 22);
    }

    private void ApplyGameplayLayout()
    {
        if (_headerRect == null || _sidePanelRect == null || _uiCanvas == null) return;

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        _lastSafeArea = Screen.safeArea;

        var scale = Mathf.Max(0.01f, _uiCanvas.scaleFactor);
        var safe = Screen.safeArea;
        var topInsetPx = Mathf.Max(0f, Screen.height - safe.yMax);
        var bottomInsetPx = Mathf.Max(0f, safe.y);
        var rightInsetPx = Mathf.Max(0f, Screen.width - safe.xMax);
        var topInset = topInsetPx / scale;
        var bottomInset = bottomInsetPx / scale;
        var rightInset = rightInsetPx / scale;

        if (_settingsButtonRect != null)
        {
            var edgePad = _isMobile ? 24f : 18f;
            _settingsButtonRect.anchoredPosition = new Vector2(
                -(rightInset + edgePad),
                -(topInset + edgePad));
        }

        // Keep header below the notch / status bar with a little extra breathing room.
        var topPad = topInset + (_isMobile ? 18f : 10f);
        var headerTotal = topPad + _headerContentHeight;
        SetAnchors(_headerRect, 0f, 1f, 1f, 1f, 0f, -headerTotal, 0f, 0f);

        // Content sits in the lower (non-inset) band of the header.
        var titleRt = _levelTitleText.GetComponent<RectTransform>();
        titleRt.offsetMin = new Vector2(28f, 8f);
        titleRt.offsetMax = new Vector2(0f, -topPad - 4f);

        var statusRt = _statusText.GetComponent<RectTransform>();
        statusRt.offsetMin = new Vector2(0f, 8f);
        statusRt.offsetMax = new Vector2(-168f, -topPad - 4f);

        var backRect = _headerRect.Find("Btn_Home") as RectTransform;
        if (backRect != null)
            backRect.anchoredPosition = new Vector2(-24f, 12f);

        if (_isMobile)
        {
            var sideH = _sidePanelHeight + bottomInset;
            _sidePanelRect.anchorMin = new Vector2(0f, 0f);
            _sidePanelRect.anchorMax = new Vector2(1f, 0f);
            _sidePanelRect.pivot = new Vector2(0.5f, 0f);
            _sidePanelRect.sizeDelta = new Vector2(0f, sideH);
            _sidePanelRect.anchoredPosition = Vector2.zero;

            var topFrac = headerTotal * scale / Screen.height;
            var bottomFrac = sideH * scale / Screen.height;
            var gap = 0.012f;
            var y = bottomFrac + gap;
            var h = Mathf.Max(0.2f, 1f - topFrac - bottomFrac - gap * 2f);
            _board3D?.SetGameplayViewport(new Rect(0f, y, 1f, h));
        }
        else
        {
            _sidePanelRect.anchorMin = new Vector2(1f, 0f);
            _sidePanelRect.anchorMax = new Vector2(1f, 1f);
            _sidePanelRect.pivot = new Vector2(1f, 0.5f);
            _sidePanelRect.sizeDelta = new Vector2(_sidePanelWidth, -headerTotal);
            _sidePanelRect.anchoredPosition = new Vector2(0f, -headerTotal * 0.5f);

            var topFrac = headerTotal * scale / Screen.height;
            var rightFrac = _sidePanelWidth * scale / Screen.width;
            var gap = 0.01f;
            var y = gap;
            var h = Mathf.Max(0.2f, 1f - topFrac - gap * 2f);
            var w = Mathf.Max(0.35f, 1f - rightFrac - gap);
            _board3D?.SetGameplayViewport(new Rect(0f, y, w, h));
        }
    }

    private void BuildWinScreen(Transform root)
    {
        _winMask = CreatePanel(root, "WinMask", GridLightTheme.Overlay);
        Stretch(_winMask.GetComponent<RectTransform>());
        _winMask.SetActive(false);

        var winCard = CreatePanel(_winMask.transform, "WinCard", GridLightTheme.Panel, false, true);
        _winCardRect = winCard.GetComponent<RectTransform>();
        SetAnchors(_winCardRect, 0.5f, 0.5f, 0.5f, 0.5f, -260f, -180f, 260f, 180f);

        var winTitle = CreateText(winCard.transform, "WinTitle", "Cleared!", 44, FontStyle.Bold, TextAnchor.UpperCenter, 0f, 0f, 56f);
        StretchTop(winTitle.GetComponent<RectTransform>(), 28f);
        _winBodyText = CreateText(winCard.transform, "WinBody", "Level 1 complete", 26, FontStyle.Normal, TextAnchor.MiddleCenter, 0f, 0f, 40f);
        var bodyRect = _winBodyText.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0.5f);
        bodyRect.anchorMax = new Vector2(1f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.sizeDelta = new Vector2(-48f, 40f);
        bodyRect.anchoredPosition = new Vector2(0f, 18f);
        _winBodyText.color = GridLightTheme.TextMuted;

        var btnRow = new GameObject("WinButtons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnRow.transform.SetParent(winCard.transform, false);
        var rowRect = btnRow.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0f);
        rowRect.anchorMax = new Vector2(0.5f, 0f);
        rowRect.pivot = new Vector2(0.5f, 0f);
        rowRect.sizeDelta = new Vector2(400f, 60f);
        rowRect.anchoredPosition = new Vector2(0f, 32f);
        var h = btnRow.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 20;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;

        CreateButton(btnRow.transform, "Next Level", GoNextLevel, GridLightTheme.Success, 168f, 60f, 22);
        CreateButton(btnRow.transform, "Main Menu", ShowMainMenuFromWin, GridLightTheme.Accent, 168f, 60f, 22);
    }

    private void ShowMainMenu()
    {
        HideWin();
        UnsubscribeAdStatus();
        _gameplayPanel.SetActive(false);
        _settingsPanel.SetActive(false);
        HideProfileConfirmation();
        if (_placementDropdownList != null)
            _placementDropdownList.SetActive(false);
        RestoreSettingsCardLayout();
        _mainMenuPanel.SetActive(true);
        _board3D.SetBoardVisible(false);
        _board3D.SetViewportMode(false);

        var saved = Mathf.Max(1, PlayerPrefs.GetInt(GridLightTheme.LevelPrefsKey, 1));
        _menuLevelText.text = $"Progress: Level {saved}";
    }

    private void StartFromMenu()
    {
        _currentLevel = Mathf.Max(1, PlayerPrefs.GetInt(GridLightTheme.LevelPrefsKey, 1));
        StartLevel(_currentLevel);
    }

    private void StartLevel(int levelNumber)
    {
        HideWin();
        _board3D.SetInputEnabled(true);
        _currentLevel = Mathf.Max(1, levelNumber);
        _mainMenuPanel.SetActive(false);
        _gameplayPanel.SetActive(true);
        ApplyGameplayLayout();
        _board3D.SetViewportMode(true);
        _board3D.SetBoardVisible(true);
        _levelTitleText.text = $"Level {_currentLevel}";
        SubscribeAdStatus();
        RefreshAdStatusUi();

        _rng = new System.Random(_currentLevel * 9973 + 42);
        SetStatus("Generating level...");

        for (var i = 0; i < 80; i++)
        {
            var level = GenerateLevel();
            if (level == null) continue;
            _level = level;
            _playerLights.Clear();
            _board3D.BuildLevel(_level);
            RefreshBoard();
            SetStatus($"Need {_level.TargetLights} lamps");
            return;
        }

        _level = CreateFallbackLevel();
        _playerLights.Clear();
        _board3D.BuildLevel(_level);
        RefreshBoard();
        SetStatus($"Need {_level.TargetLights} lamps");
    }

    private void GoNextLevel()
    {
        HideWin();
        StartLevel(_currentLevel + 1);
    }

    private void ShowMainMenuFromWin()
    {
        HideWin();
        ShowMainMenu();
    }

    private void OnLevelComplete()
    {
        PlayerPrefs.SetInt(GridLightTheme.LevelPrefsKey, _currentLevel + 1);
        PlayerPrefs.Save();
        _board3D.SetInputEnabled(false);

        if (GridLightMspAds.Instance != null)
            GridLightMspAds.Instance.TryShowInterstitial(ShowLevelCompleteUi);
        else
            ShowLevelCompleteUi();
    }

    private void ShowLevelCompleteUi()
    {
        _winBodyText.text = $"Level {_currentLevel} complete!";
        _winMask.SetActive(true);
        Canvas.ForceUpdateCanvases();
        if (_winCardRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_winCardRect);
        SetStatus("Perfect clear!");
    }

    private void ResetLevel()
    {
        if (_level == null) return;
        HideWin();
        _playerLights.Clear();
        RefreshBoard();
        SetStatus("Reset");
    }

    private void ShowAnswer()
    {
        if (_level == null) return;
        HideWin();
        _playerLights.Clear();
        foreach (var p in _level.Solution) _playerLights.Add(p);
        RefreshBoard();
        SetStatus("Answer shown");
        if (CheckWin()) OnLevelComplete();
    }

    private void ToggleLight(GridPos p)
    {
        if (_level == null || _level.Obstacles.Contains(p)) return;

        if (_playerLights.Contains(p)) _playerLights.Remove(p);
        else _playerLights.Add(p);

        RefreshBoard();
        if (CheckWin())
        {
            OnLevelComplete();
            return;
        }

        var lit = GridLightLogic.GetIlluminated(_playerLights, _level.Cells, _level.Obstacles);
        var total = GridLightLogic.LightableCells(_level.Cells, _level.Obstacles).Count;
        if (GridLightLogic.LightsSeeEachOther(_playerLights, _level.Cells, _level.Obstacles))
            SetStatus("Lamps must not face each other");
        else if (lit.Count < total)
            SetStatus($"{total - lit.Count} cells still dark");
        else
            SetStatus("Lit, but lamp count mismatch");
    }

    private void RefreshBoard()
    {
        if (_level == null) return;
        var conflicts = GridLightLogic.BuildConflictLights(_playerLights, _level.Cells, _level.Obstacles);
        _board3D.UpdateVisuals(_playerLights, conflicts);
        UpdateStats();
    }

    private void UpdateStats()
    {
        if (_level == null) return;
        var lit = GridLightLogic.GetIlluminated(_playerLights, _level.Cells, _level.Obstacles);
        var total = GridLightLogic.LightableCells(_level.Cells, _level.Obstacles).Count;
        var conflicts = GridLightLogic.LightsSeeEachOther(_playerLights, _level.Cells, _level.Obstacles);
        _statsText.text =
            $"Cells: {total}   Blocks: {_level.Obstacles.Count}\n" +
            $"Lamps: {_playerLights.Count} / {_level.TargetLights}\n" +
            $"Lit: {lit.Count} / {total}\n" +
            (conflicts ? "Lamps face each other" : "Keep placing...");
    }

    private bool CheckWin()
    {
        if (_level == null) return false;
        if (_playerLights.Count != _level.TargetLights) return false;
        return GridLightLogic.IsValidPlacement(_playerLights, _level.Cells, _level.Obstacles);
    }

    private void HideWin()
    {
        _winMask.SetActive(false);
        _board3D.SetInputEnabled(true);
    }

    private void SetStatus(string message) => _statusText.text = message;

    private void SubscribeAdStatus()
    {
        GridLightMspAds.StatusChanged -= OnAdStatusChanged;
        GridLightMspAds.StatusChanged += OnAdStatusChanged;
    }

    private void UnsubscribeAdStatus()
    {
        GridLightMspAds.StatusChanged -= OnAdStatusChanged;
    }

    private void OnAdStatusChanged(GridLightMspLoadStatus status) => RefreshAdStatusUi(status);

    private void RefreshAdStatusUi(GridLightMspLoadStatus status = null)
    {
        if (_adStatusText == null) return;

        if (status == null)
        {
            var ads = GridLightMspAds.Instance;
            status = ads != null
                ? ads.CurrentStatus
                : new GridLightMspLoadStatus(GridLightMspLoadState.Initializing);
        }

        _adStatusText.text = status.DisplayText;
        _adStatusText.color = status.State switch
        {
            GridLightMspLoadState.Ready => GridLightTheme.Success,
            GridLightMspLoadState.Failed => new Color32(196, 88, 72, 255),
            GridLightMspLoadState.Showing => GridLightTheme.Accent,
            _ => GridLightTheme.TextMuted,
        };
    }

    private GridLevelData CreateFallbackLevel()
    {
        var cells = new HashSet<GridPos>
        {
            new GridPos(0, 0),
            new GridPos(1, 0),
            new GridPos(2, 0),
        };
        var obstacles = new HashSet<GridPos>();
        var solution = new HashSet<GridPos> { new GridPos(1, 0) };

        return new GridLevelData
        {
            Cells = cells,
            Obstacles = obstacles,
            Solution = solution,
            MinLights = solution.Count,
            TargetLights = solution.Count,
            Seed = _currentLevel,
        };
    }

    private GridLevelData GenerateLevel()
    {
        var cells = GenerateMap(MapMin, MapMax);
        if (!IsConnected(cells)) return null;

        var obstacles = new HashSet<GridPos>();
        var candidates = cells.ToList();
        Shuffle(candidates);
        var obstacleTarget = _rng.Next(3, 8);
        foreach (var p in candidates)
        {
            if (obstacles.Count >= obstacleTarget) break;
            obstacles.Add(p);
            if (!IsLightableConnected(cells, obstacles)) obstacles.Remove(p);
        }

        var min = FindMinimumSolutions(cells, obstacles, 40);
        if (min == null || min.Solutions.Count == 0) return null;

        var selected = min.Solutions[_rng.Next(min.Solutions.Count)];
        var unique = FindSolutionsWithCount(cells, obstacles, min.Count, 2);
        if (unique.Count < 1) return null;

        return new GridLevelData
        {
            Cells = cells,
            Obstacles = obstacles,
            Solution = selected,
            MinLights = min.Count,
            TargetLights = min.Count,
            Seed = _rng.Next(1, 999999999),
        };
    }

    private HashSet<GridPos> GenerateMap(int minCells, int maxCells)
    {
        var target = _rng.Next(minCells, maxCells + 1);
        var cells = new HashSet<GridPos> { new GridPos(0, 0) };
        var frontier = new List<GridPos> { new GridPos(0, 0) };

        while (cells.Count < target && frontier.Count > 0)
        {
            var idx = _rng.Next(frontier.Count);
            var c = frontier[idx];
            var neighbors = GridLightLogic.Dirs.Select(d => c + d).ToList();
            Shuffle(neighbors);
            var grew = false;
            foreach (var n in neighbors)
            {
                if (cells.Contains(n)) continue;
                cells.Add(n);
                frontier.Add(n);
                grew = true;
                break;
            }
            if (!grew) frontier.RemoveAt(idx);
        }

        Normalize(cells);
        return cells;
    }

    private static void Normalize(HashSet<GridPos> cells)
    {
        var minX = cells.Min(p => p.X);
        var minY = cells.Min(p => p.Y);
        if (minX == 0 && minY == 0) return;
        var arr = cells.Select(p => new GridPos(p.X - minX, p.Y - minY)).ToArray();
        cells.Clear();
        foreach (var p in arr) cells.Add(p);
    }

    private static bool IsConnected(HashSet<GridPos> cells)
    {
        if (cells.Count == 0) return false;
        var start = cells.First();
        var seen = new HashSet<GridPos> { start };
        var q = new Queue<GridPos>();
        q.Enqueue(start);
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            foreach (var d in GridLightLogic.Dirs)
            {
                var n = c + d;
                if (!cells.Contains(n) || seen.Contains(n)) continue;
                seen.Add(n);
                q.Enqueue(n);
            }
        }
        return seen.Count == cells.Count;
    }

    private static bool IsLightableConnected(HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        var playable = GridLightLogic.LightableCells(cells, obstacles);
        if (playable.Count <= 1) return true;
        var set = new HashSet<GridPos>(playable);
        var seen = new HashSet<GridPos> { playable[0] };
        var q = new Queue<GridPos>();
        q.Enqueue(playable[0]);
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            foreach (var d in GridLightLogic.Dirs)
            {
                var n = c + d;
                if (!set.Contains(n) || seen.Contains(n)) continue;
                seen.Add(n);
                q.Enqueue(n);
            }
        }
        return seen.Count == playable.Count;
    }

    private static SolveResult FindMinimumSolutions(HashSet<GridPos> cells, HashSet<GridPos> obstacles, int limit)
    {
        var ctx = BuildSolverContext(cells, obstacles);
        if (ctx == null) return null;
        var upper = Mathf.Min(ctx.Candidates.Count, 9);
        for (var n = 1; n <= upper; n++)
        {
            var sols = SearchSolutions(ctx, n, limit);
            if (sols.Count > 0) return new SolveResult { Count = n, Solutions = sols };
        }
        return null;
    }

    private static List<HashSet<GridPos>> FindSolutionsWithCount(HashSet<GridPos> cells, HashSet<GridPos> obstacles, int count, int limit)
    {
        var ctx = BuildSolverContext(cells, obstacles);
        return ctx == null ? new List<HashSet<GridPos>>() : SearchSolutions(ctx, count, limit);
    }

    private static SolveContext BuildSolverContext(HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        var list = GridLightLogic.LightableCells(cells, obstacles);
        if (list.Count > 30) return null;
        var index = list.Select((pos, idx) => (pos, idx)).ToDictionary(x => x.pos, x => x.idx);
        var full = list.Count == 0 ? 0u : ((1u << list.Count) - 1u);
        var candidates = new List<Candidate>();
        foreach (var pos in list)
        {
            var mask = 1u << index[pos];
            foreach (var d in GridLightLogic.Dirs)
            {
                foreach (var rc in GridLightLogic.RayCells(pos, d, cells, obstacles))
                    mask |= 1u << index[rc];
            }
            candidates.Add(new Candidate { Pos = pos, Cover = mask, CellIndex = index[pos] });
        }

        var conflictMasks = new Dictionary<GridPos, uint>();
        foreach (var a in list)
        {
            uint cfm = 0;
            foreach (var b in list)
            {
                if (a.Equals(b)) continue;
                if (GridLightLogic.PairSeesEachOther(a, b, cells, obstacles))
                    cfm |= 1u << index[b];
            }
            conflictMasks[a] = cfm;
        }

        var coverLists = new List<int>[list.Count];
        for (var i = 0; i < list.Count; i++)
        {
            var cellBit = 1u << i;
            coverLists[i] = new List<int>();
            for (var j = 0; j < candidates.Count; j++)
            {
                if ((candidates[j].Cover & cellBit) != 0) coverLists[i].Add(j);
            }
        }

        return new SolveContext
        {
            FullMask = full,
            Candidates = candidates,
            ConflictMasks = conflictMasks,
            CoverLists = coverLists,
        };
    }

    private static List<HashSet<GridPos>> SearchSolutions(SolveContext ctx, int targetCount, int limit)
    {
        var results = new List<HashSet<GridPos>>();
        var chosen = new List<GridPos>();

        void Bt(uint litMask, uint placedMask, int depth)
        {
            if (results.Count >= limit) return;
            var uncovered = ctx.FullMask & ~litMask;
            if (uncovered == 0)
            {
                if (depth == targetCount) results.Add(new HashSet<GridPos>(chosen));
                return;
            }
            if (depth >= targetCount) return;

            var lowBit = uncovered & (uint)-(int)uncovered;
            var cellIdx = LowestBitIndex(lowBit);
            var options = ctx.CoverLists[cellIdx];
            if (options == null || options.Count == 0) return;

            var viable = new List<int>();
            foreach (var oi in options)
            {
                var c = ctx.Candidates[oi];
                var bit = 1u << c.CellIndex;
                if ((placedMask & bit) != 0) continue;
                if ((placedMask & ctx.ConflictMasks[c.Pos]) != 0) continue;
                viable.Add(oi);
            }
            if (viable.Count == 0) return;

            viable.Sort((a, b) =>
                Popcount(ctx.Candidates[b].Cover & uncovered).CompareTo(Popcount(ctx.Candidates[a].Cover & uncovered)));

            foreach (var idx in viable)
            {
                var c = ctx.Candidates[idx];
                chosen.Add(c.Pos);
                Bt(litMask | c.Cover, placedMask | (1u << c.CellIndex), depth + 1);
                chosen.RemoveAt(chosen.Count - 1);
                if (results.Count >= limit) return;
            }
        }

        Bt(0u, 0u, 0);
        return results;
    }

    private static int Popcount(uint x)
    {
        var n = 0;
        while (x != 0) { n += (int)(x & 1); x >>= 1; }
        return n;
    }

    private static int LowestBitIndex(uint bit)
    {
        var idx = 0;
        while ((bit >>= 1) != 0) idx++;
        return idx;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static void EnsureEventSystem()
    {
        var existing = FindFirstObjectByType<EventSystem>();
        if (existing != null)
        {
            var legacy = existing.GetComponent<StandaloneInputModule>();
            if (legacy != null) Destroy(legacy);
            if (existing.GetComponent<InputSystemUIInputModule>() == null)
            {
                var module = existing.gameObject.AddComponent<InputSystemUIInputModule>();
                module.AssignDefaultActions();
            }
            return;
        }

        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private static GameObject CreatePanel(Transform parent, string name, Color32 color, bool receiveRaycasts = true, bool rounded = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = receiveRaycasts && color.a > 5;
        if (rounded)
        {
            image.sprite = GetRoundedSprite();
            image.type = Image.Type.Sliced;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = GridLightTheme.PanelStroke;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }
        return go;
    }

    private static Text CreateText(Transform parent, string name, string text, int fontSize, FontStyle style, TextAnchor anchor, float leftInset = 0f, float rightInset = 0f, float height = 42f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(-(leftInset + rightInset), height);
        rect.anchoredPosition = new Vector2((leftInset - rightInset) * 0.5f, 0f);

        var layout = go.GetComponent<LayoutElement>();
        layout.preferredHeight = height;
        layout.flexibleWidth = 0f;

        var t = go.GetComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.alignment = anchor;
        t.color = GridLightTheme.TextDark;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private static Button CreateSettingsIconButton(Transform parent, Action onClick)
    {
        var go = new GameObject("Btn_Settings", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = GridLightTheme.AccentSoft;
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.raycastTarget = true;

        var colors = go.GetComponent<Button>().colors;
        colors.highlightedColor = Color.Lerp(GridLightTheme.AccentSoft, Color.white, 0.12f);
        colors.pressedColor = Color.Lerp(GridLightTheme.AccentSoft, Color.black, 0.12f);
        colors.selectedColor = colors.highlightedColor;
        go.GetComponent<Button>().colors = colors;

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.18f);
        shadow.effectDistance = new Vector2(0f, -3f);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(go.transform, false);
        var iconRect = iconGo.GetComponent<RectTransform>();
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(34f, 34f);
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = GetSettingsIconSprite();
        iconImage.color = GridLightTheme.TextDark;
        iconImage.raycastTarget = false;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => onClick());
        return btn;
    }

    private static Button CreateButton(Transform parent, string title, Action onClick, Color32 color, float width = 200f, float height = 56f, int fontSize = 22)
    {
        var go = new GameObject($"Btn_{title}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;

        var layout = go.GetComponent<LayoutElement>();
        layout.preferredHeight = height;
        layout.minHeight = height;
        layout.preferredWidth = width > 0 ? width : 200f;
        layout.flexibleWidth = 0f;

        var colors = go.GetComponent<Button>().colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.12f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.12f);
        colors.selectedColor = colors.highlightedColor;
        go.GetComponent<Button>().colors = colors;

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.18f);
        shadow.effectDistance = new Vector2(0f, -3f);

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => onClick());

        var text = CreateText(go.transform, "Label", title, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
        text.color = GridLightTheme.TextDark;
        Stretch(text.GetComponent<RectTransform>());
        UnityEngine.Object.Destroy(text.GetComponent<LayoutElement>());
        return btn;
    }

    private void CreatePlacementDropdown(Transform parent)
    {
        var buttonGo = new GameObject("PlacementDropdown", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonGo.transform.SetParent(parent, false);

        var image = buttonGo.GetComponent<Image>();
        image.color = GridLightTheme.PanelDeep;
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;

        var layout = buttonGo.GetComponent<LayoutElement>();
        layout.preferredHeight = 58f;
        layout.minHeight = 58f;
        layout.preferredWidth = 520f;
        layout.flexibleWidth = 0f;

        var colors = buttonGo.GetComponent<Button>().colors;
        colors.highlightedColor = Color.Lerp(GridLightTheme.PanelDeep, Color.white, 0.12f);
        colors.pressedColor = Color.Lerp(GridLightTheme.PanelDeep, Color.black, 0.12f);
        colors.selectedColor = colors.highlightedColor;
        buttonGo.GetComponent<Button>().colors = colors;

        var btn = buttonGo.GetComponent<Button>();
        btn.onClick.AddListener(TogglePlacementDropdown);
        _placementDropdownButton = btn;

        _placementDropdownLabel = CreateText(buttonGo.transform, "Label", "", 22, FontStyle.Normal, TextAnchor.MiddleLeft);
        _placementDropdownLabel.color = GridLightTheme.TextDark;
        var labelRect = _placementDropdownLabel.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(18f, 8f);
        labelRect.offsetMax = new Vector2(-40f, -8f);
        UnityEngine.Object.Destroy(_placementDropdownLabel.GetComponent<LayoutElement>());

        var arrow = CreateText(buttonGo.transform, "Arrow", "\u25BC", 20, FontStyle.Normal, TextAnchor.MiddleCenter);
        arrow.color = GridLightTheme.TextDark;
        var arrowRect = arrow.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1f, 0.5f);
        arrowRect.anchorMax = new Vector2(1f, 0.5f);
        arrowRect.pivot = new Vector2(1f, 0.5f);
        arrowRect.sizeDelta = new Vector2(24f, 22f);
        arrowRect.anchoredPosition = new Vector2(-12f, 0f);
        UnityEngine.Object.Destroy(arrow.GetComponent<LayoutElement>());

        _placementDropdownList = new GameObject("PlacementList", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        _placementDropdownList.transform.SetParent(parent, false);
        _placementDropdownList.SetActive(false);

        var listImage = _placementDropdownList.GetComponent<Image>();
        listImage.color = GridLightTheme.PanelDeep;
        listImage.sprite = GetRoundedSprite();
        listImage.type = Image.Type.Sliced;

        var listLayout = _placementDropdownList.AddComponent<VerticalLayoutGroup>();
        listLayout.childAlignment = TextAnchor.UpperCenter;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandWidth = false;
        listLayout.childForceExpandHeight = false;
        listLayout.spacing = 4;
        listLayout.padding = new RectOffset(8, 8, 8, 8);

        var listFitter = _placementDropdownList.AddComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        listFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var listLayoutEl = _placementDropdownList.GetComponent<LayoutElement>();
        listLayoutEl.preferredWidth = 520f;

        var options = GetPlacementOptions();
        foreach (var option in options)
        {
            var capturedOption = option;
            CreateButton(_placementDropdownList.transform, option, () => OnPlacementOptionSelected(capturedOption), GridLightTheme.PanelDeep, 500f, 44f, 18);
        }
    }

    private static InputField CreateInputField(
        Transform parent,
        string name,
        string placeholder,
        Action<string> onEndEdit,
        float width,
        float height,
        int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.color = GridLightTheme.PanelDeep;
        image.raycastTarget = true;
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;

        var layout = go.GetComponent<LayoutElement>();
        layout.preferredHeight = height;
        layout.minHeight = height;
        layout.preferredWidth = width;
        layout.flexibleWidth = 0f;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 8f);
        textRect.offsetMax = new Vector2(-18f, -8f);

        var text = textGo.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Normal;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = GridLightTheme.TextDark;
        text.supportRichText = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
        placeholderGo.transform.SetParent(go.transform, false);
        var placeholderRect = placeholderGo.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(18f, 8f);
        placeholderRect.offsetMax = new Vector2(-18f, -8f);

        var placeholderText = placeholderGo.GetComponent<Text>();
        placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholderText.fontSize = fontSize - 2;
        placeholderText.fontStyle = FontStyle.Italic;
        placeholderText.alignment = TextAnchor.MiddleLeft;
        placeholderText.color = GridLightTheme.TextMuted;
        placeholderText.text = placeholder;
        placeholderText.supportRichText = false;

        var inputField = go.GetComponent<InputField>();
        inputField.textComponent = text;
        inputField.placeholder = placeholderText;
        inputField.lineType = InputField.LineType.SingleLine;
        inputField.onEndEdit.AddListener(value => onEndEdit?.Invoke(value));

        return inputField;
    }

    private static Sprite GetRoundedSprite()
    {
        if (_roundedSprite != null) return _roundedSprite;

        const int size = 64;
        const int radius = 14;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var inside =
                    IsInsideRoundedRect(x + 0.5f, y + 0.5f, size, radius);
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        tex.Apply(false, true);
        _roundedSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return _roundedSprite;
    }

    private static Sprite GetSettingsIconSprite()
    {
        if (_settingsIconSprite != null) return _settingsIconSprite;

        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var cx = size * 0.5f;
        var cy = size * 0.5f;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, IsInsideGearIcon(x + 0.5f, y + 0.5f, cx, cy) ? Color.white : Color.clear);
            }
        }

        tex.Apply(false, true);
        _settingsIconSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f);
        return _settingsIconSprite;
    }

    private static bool IsInsideGearIcon(float x, float y, float cx, float cy)
    {
        var dx = x - cx;
        var dy = y - cy;
        var dist = Mathf.Sqrt(dx * dx + dy * dy);
        var angle = Mathf.Atan2(dy, dx);
        const float teeth = 8f;
        const float outerBase = 24f;
        const float outerAmp = 4f;
        const float innerR = 17f;
        const float hubOuter = 9f;
        const float hubInner = 3.5f;

        var outerR = outerBase + outerAmp * Mathf.Cos(teeth * angle);
        if (dist <= outerR && dist >= innerR) return true;
        return dist <= hubOuter && dist >= hubInner;
    }

    private static bool IsInsideRoundedRect(float x, float y, int size, int radius)
    {
        var min = radius;
        var max = size - radius;
        if (x >= min && x <= max) return y >= 0 && y <= size;
        if (y >= min && y <= max) return x >= 0 && x <= size;

        var cx = x < min ? min : max;
        var cy = y < min ? min : max;
        var dx = x - cx;
        var dy = y - cy;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void StretchTop(RectTransform rt, float topInset)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -topInset);
        rt.sizeDelta = new Vector2(-48f, rt.sizeDelta.y);
    }

    private static void SetAnchors(RectTransform rt, float minX, float minY, float maxX, float maxY, float left, float bottom, float right, float top)
    {
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(right, top);
    }

    private class Candidate
    {
        public GridPos Pos;
        public uint Cover;
        public int CellIndex;
    }

    private class SolveContext
    {
        public uint FullMask;
        public List<Candidate> Candidates;
        public Dictionary<GridPos, uint> ConflictMasks;
        public List<int>[] CoverLists;
    }

    private class SolveResult
    {
        public int Count;
        public List<HashSet<GridPos>> Solutions;
    }
}
