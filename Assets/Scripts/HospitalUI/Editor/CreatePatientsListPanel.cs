using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Воссоздаёт панель списка пациентов 1 в 1: поиск, список пациентов, список сессий, блок аналитики, кнопка Fermer.
/// Меню: Tools → Hospital → Create Patient List Panel (under selection or Canvas).
/// </summary>
public static class CreatePatientsListPanel
{
    const float PanelWidth = 920f;
    const float PanelHeight = 660f;
    const float OverlayAlpha = 0.4f;
    const float Padding = 12f;
    const float SearchHeight = 40f;
    const float ButtonHeight = 38f;
    const float LabelHeight = 24f;
    const float RightColumnSessionsRatio = 0.52f; // 52% список сессий, 48% аналитика

    [MenuItem("Tools/Hospital/Create Patient List Panel")]
    public static void CreateFromMenu()
    {
        Create(null);
    }

    public static void Create(Transform forceParent)
    {
        Transform parent = forceParent != null ? forceParent : GetParent();
        if (parent == null)
        {
            Debug.LogWarning("Select a Canvas or UI parent in the hierarchy, then run Tools → Hospital → Create Patient List Panel.");
            return;
        }

        Undo.SetCurrentGroupName("Create Patient List Panel");
        int group = Undo.GetCurrentGroup();

        GameObject listPanel = CreateListPanel(parent);
        RectTransform searchRect = CreateSearchField(listPanel.transform);
        RectTransform patientsContent = CreateScrollView(listPanel.transform, "PatientsScrollView", "PatientsContent", new Vector2(0.35f, 1f));
        RectTransform labelSessions = CreateLabel(listPanel.transform, "Séances");
        RectTransform sessionsContent = CreateScrollView(listPanel.transform, "SessionsScrollView", "SessionsContent", new Vector2(0.35f, 1f));
        RectTransform labelAnalytics = CreateLabel(listPanel.transform, "Analytique");
        RectTransform analyticsContent = CreateAnalyticsBlock(listPanel.transform);
        CreateFermerButton(listPanel.transform);

        PositionAreas(listPanel.transform, searchRect, patientsContent, labelSessions, sessionsContent, labelAnalytics, analyticsContent);

        PatientsListUI ui = forceParent != null ? forceParent.GetComponentInParent<PatientsListUI>() : Object.FindObjectOfType<PatientsListUI>();
        if (ui == null && forceParent != null)
            ui = Object.FindObjectOfType<PatientsListUI>();
        if (ui != null)
        {
            Undo.RecordObject(ui, "Assign List Panel");
            var so = new SerializedObject(ui);
            so.FindProperty("ListPanel").objectReferenceValue = listPanel;
            so.FindProperty("SearchFieldLegacy").objectReferenceValue = searchRect.GetComponentInChildren<InputField>(true);
            so.FindProperty("PatientsContent").objectReferenceValue = patientsContent;
            so.FindProperty("SessionsContent").objectReferenceValue = sessionsContent;
            so.FindProperty("AnalyticsContent").objectReferenceValue = analyticsContent;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("PatientsListUI references assigned. If you use TMP, assign SearchFieldTMP in Inspector.");
        }
        else
        {
            Debug.Log("Patient List Panel created. Add PatientsListUI to a GameObject and assign: ListPanel, SearchFieldLegacy, PatientsContent, SessionsContent, AnalyticsContent.");
        }

        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = listPanel;
    }

    static Transform GetParent()
    {
        if (Selection.activeGameObject != null)
        {
            var rect = Selection.activeGameObject.GetComponent<RectTransform>();
            if (rect != null) return Selection.activeGameObject.transform;
            var canvas = Selection.activeGameObject.GetComponent<Canvas>();
            if (canvas != null) return Selection.activeGameObject.transform;
        }
        var c = Object.FindObjectOfType<Canvas>();
        return c != null ? c.transform : null;
    }

    static GameObject CreateListPanel(Transform parent)
    {
        var go = new GameObject("PatientsListPanel");
        Undo.RegisterCreatedObjectUndo(go, "ListPanel");
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        rect.anchoredPosition = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, OverlayAlpha);

        return go;
    }

    static RectTransform CreateSearchField(Transform parent)
    {
        var go = new GameObject("SearchField");
        Undo.RegisterCreatedObjectUndo(go, "Search");
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -Padding);
        rect.sizeDelta = new Vector2(0, SearchHeight);
        rect.offsetMin = new Vector2(Padding, -Padding - SearchHeight);
        rect.offsetMax = new Vector2(-Padding, -Padding);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 0.95f, 0.9f);

        var input = go.AddComponent<InputField>();
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 4);
        textRect.offsetMax = new Vector2(-10, -4);
        var text = textGo.AddComponent<Text>();
        text.text = "";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = Color.black;
        input.textComponent = text;

        var placeholderGo = new GameObject("Placeholder");
        placeholderGo.transform.SetParent(go.transform, false);
        var phRect = placeholderGo.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = new Vector2(10, 4);
        phRect.offsetMax = new Vector2(-10, -4);
        var ph = placeholderGo.AddComponent<Text>();
        ph.text = "Recherche nom / prénom";
        ph.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ph.fontSize = 16;
        ph.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        input.placeholder = ph;

        return rect;
    }

    static RectTransform CreateScrollView(Transform parent, string scrollName, string contentName, Vector2 widthAnchor)
    {
        var scrollGo = new GameObject(scrollName);
        Undo.RegisterCreatedObjectUndo(scrollGo, scrollName);
        scrollGo.transform.SetParent(parent, false);

        var scrollRect = scrollGo.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        var img = scrollGo.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);

        var viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollGo.transform, false);
        var viewportRect = viewportGo.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(2, 2);
        viewportRect.offsetMax = new Vector2(-2, -2);
        viewportGo.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 0.99f);
        viewportGo.AddComponent<Mask>().showMaskGraphic = false;

        var contentGo = new GameObject(contentName);
        contentGo.transform.SetParent(viewportGo.transform, false);
        var contentRect = contentGo.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 400);

        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        scroll.vertical = true;

        return contentRect;
    }

    static RectTransform CreateLabel(Transform parent, string text)
    {
        var go = new GameObject("Label_" + text);
        Undo.RegisterCreatedObjectUndo(go, "Label");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, LabelHeight);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 14;
        t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.95f, 0.95f, 0.95f);
        return rect;
    }

    static RectTransform CreateAnalyticsBlock(Transform parent)
    {
        var panelGo = new GameObject("AnalyticsPanel");
        Undo.RegisterCreatedObjectUndo(panelGo, "AnalyticsPanel");
        panelGo.transform.SetParent(parent, false);

        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0, 260f);

        panelGo.AddComponent<Image>().color = new Color(0.15f, 0.18f, 0.22f, 0.95f);

        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(panelGo.transform, false);
        var contentRect = contentGo.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(8, 8);
        contentRect.offsetMax = new Vector2(-8, -8);

        return contentRect;
    }

    static void CreateFermerButton(Transform parent)
    {
        var go = new GameObject("FermerButton");
        Undo.RegisterCreatedObjectUndo(go, "Fermer");
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, ButtonHeight);
        rect.offsetMin = new Vector2(Padding, Padding);
        rect.offsetMax = new Vector2(-Padding, Padding + ButtonHeight);

        go.AddComponent<Image>().color = new Color(0.25f, 0.3f, 0.4f, 0.95f);
        go.AddComponent<Button>();

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var text = textGo.AddComponent<Text>();
        text.text = "Fermer";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }

    static void PositionAreas(Transform panelTransform, RectTransform searchRect,
        RectTransform patientsContent, RectTransform labelSessions, RectTransform sessionsContent,
        RectTransform labelAnalytics, RectTransform analyticsContent)
    {
        float top = -Padding - SearchHeight;
        float bottom = Padding + ButtonHeight + Padding;
        float contentHeight = PanelHeight - (Padding * 2 + SearchHeight + ButtonHeight + Padding);
        float rightTop = bottom + contentHeight * RightColumnSessionsRatio;
        float gap = 4f;

        var patientsScroll = patientsContent.parent.parent.GetComponent<RectTransform>();
        var sessionsScroll = sessionsContent.parent.parent.GetComponent<RectTransform>();
        var analyticsPanel = analyticsContent.parent.GetComponent<RectTransform>();

        // Левая колонка: список пациентов на всю высоту
        patientsScroll.anchorMin = new Vector2(0, 0);
        patientsScroll.anchorMax = new Vector2(0.48f, 1);
        patientsScroll.offsetMin = new Vector2(Padding, bottom);
        patientsScroll.offsetMax = new Vector2(-Padding / 2f, top);

        // Правая колонка: подпись "Séances" + список сессий
        labelSessions.anchorMin = new Vector2(0.52f, 1);
        labelSessions.anchorMax = new Vector2(1f, 1);
        labelSessions.offsetMin = new Vector2(Padding / 2f, top - LabelHeight);
        labelSessions.offsetMax = new Vector2(-Padding, top);

        sessionsScroll.anchorMin = new Vector2(0.52f, 0);
        sessionsScroll.anchorMax = new Vector2(1f, 1);
        sessionsScroll.offsetMin = new Vector2(Padding / 2f, rightTop + gap);
        sessionsScroll.offsetMax = new Vector2(-Padding, top - LabelHeight - gap);

        // Подпись "Analytique" + блок аналитики
        labelAnalytics.anchorMin = new Vector2(0.52f, 0);
        labelAnalytics.anchorMax = new Vector2(1f, 0);
        labelAnalytics.offsetMin = new Vector2(Padding / 2f, rightTop - LabelHeight - gap);
        labelAnalytics.offsetMax = new Vector2(-Padding, rightTop - gap);

        analyticsPanel.anchorMin = new Vector2(0.52f, 0);
        analyticsPanel.anchorMax = new Vector2(1f, 0);
        analyticsPanel.offsetMin = new Vector2(Padding / 2f, bottom);
        analyticsPanel.offsetMax = new Vector2(-Padding, rightTop - LabelHeight - gap);
    }
}

[CustomEditor(typeof(PatientsListUI))]
public class PatientsListUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(6);
        if (GUILayout.Button("Create Patient List Panel"))
        {
            var ui = (PatientsListUI)target;
            Transform parent = ui.transform.parent != null ? ui.transform.parent : Object.FindObjectOfType<Canvas>()?.transform;
            if (parent == null)
            {
                Debug.LogWarning("No parent or Canvas in scene. Create under a Canvas.");
                return;
            }
            CreatePatientsListPanel.Create(parent);
        }
    }
}
