using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Menu : Hospital UI ▸ Create Commencer menu (full)
/// Генерирует всё меню «Commencer» с нуля:
///   – Колонка «Superviseur» (заголовок, поиск, список, кнопка «Ajouter superviseur»)
///   – Колонка «Patient»     (заголовок, поиск, список, кнопка «Ajouter patient»)
///   – Колонка «Info»        (Séances + Analytique, два скролла)
///   – Нижние кнопки «Fermer» и «Démarrer la séance»
///   – Панель «Configuration» с кнопкой «Retour»
/// Все ссылки автоматически назначаются на DoctorLoginUI и ConfigurationPanelUI.
/// </summary>
public static class CreateCommencerMenuFull
{
    const int UILayer = 5;

    static readonly Color PanelBg   = new Color(0.18f, 0.18f, 0.22f, 0.95f);
    static readonly Color ColBg     = new Color(0.22f, 0.22f, 0.28f, 0.6f);
    static readonly Color ScrollBg  = new Color(0.14f, 0.14f, 0.18f, 0.95f);
    static readonly Color SearchBg  = new Color(0.28f, 0.28f, 0.32f, 0.95f);
    static readonly Color BtnGreen  = new Color(0.18f, 0.55f, 0.28f, 1f);
    static readonly Color BtnRed    = new Color(0.6f, 0.18f, 0.18f, 1f);
    static readonly Color BtnTeal   = new Color(0.2f, 0.42f, 0.38f, 1f);

    [MenuItem("Hospital UI/Create Commencer menu (full)")]
    public static void Run()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("CreateCommencer: aucun Canvas trouvé dans la scène. Ajoutez un Canvas d'abord.");
            return;
        }

        // ─── Root ───────────────────────────────────────────────
        var root = CreateGO("CommencerRoot", canvas.transform);
        Undo.RegisterCreatedObjectUndo(root, "Create Commencer menu");
        Stretch(root);
        var doctorLogin = root.AddComponent<DoctorLoginUI>();

        // ─── Bouton Commencer (sous Canvas, pas sous root) ──────
        var commencerBtn = CreateButton(canvas.transform, "CommencerButton", "Commencer", BtnGreen, 180, 40);
        var commencerRect = commencerBtn.GetComponent<RectTransform>();
        commencerRect.anchorMin = commencerRect.anchorMax = new Vector2(0.5f, 0.5f);
        commencerRect.anchoredPosition = Vector2.zero;
        commencerBtn.transform.SetAsFirstSibling();

        // ─── LoginMedecinPanel ──────────────────────────────────
        var loginPanel = CreateGO("LoginMedecinPanel", root.transform);
        Stretch(loginPanel);
        loginPanel.AddComponent<Image>().color = PanelBg;

        // ─── Column 1 : Superviseur ────────────────────────────
        var colSup = CreateColumn(loginPanel.transform, "Col_Superviseur", 0f, 1f / 3f);

        var lblSup = CreateLabel(colSup.transform, "LblSuperviseur", "Aucun superviseur sélectionné", 14);
        SetAnchors(lblSup, 0.02f, 0.93f, 0.98f, 1f);

        var searchSup = CreateSearchField(colSup.transform, "SearchSuperviseur", "Rechercher");
        SetAnchors(searchSup, 0.02f, 0.84f, 0.98f, 0.92f);

        var scrollSup = CreateScrollView(colSup.transform, "SupervisorsScroll", out var contentSup);
        SetAnchors(scrollSup, 0.02f, 0.14f, 0.98f, 0.82f);

        var btnAddSup = CreateButton(colSup.transform, "Btn_AddSupervisor", "Ajouter superviseur", BtnTeal, 0, 0);
        SetAnchors(btnAddSup.gameObject, 0.05f, 0.02f, 0.95f, 0.12f);

        // ─── Column 2 : Patient ─────────────────────────────────
        var colPat = CreateColumn(loginPanel.transform, "Col_Patient", 1f / 3f, 2f / 3f);

        var lblPat = CreateLabel(colPat.transform, "LblPatient", "Aucun patient sélectionné", 14);
        SetAnchors(lblPat, 0.02f, 0.93f, 0.98f, 1f);

        var searchPat = CreateSearchField(colPat.transform, "SearchPatient", "Rechercher");
        SetAnchors(searchPat, 0.02f, 0.84f, 0.98f, 0.92f);

        var scrollPat = CreateScrollView(colPat.transform, "PatientsScroll", out var contentPat);
        SetAnchors(scrollPat, 0.02f, 0.14f, 0.98f, 0.82f);

        var btnAddPat = CreateButton(colPat.transform, "Btn_AddPatient", "Ajouter patient", BtnTeal, 0, 0);
        SetAnchors(btnAddPat.gameObject, 0.05f, 0.02f, 0.95f, 0.12f);

        // ─── Column 3 : Séances + Analytique ────────────────────
        var colInfo = CreateColumn(loginPanel.transform, "Col_Info", 2f / 3f, 1f);

        var lblSessions = CreateLabel(colInfo.transform, "LblSeances", "Séances", 13);
        SetAnchors(lblSessions, 0.02f, 0.90f, 0.98f, 0.98f);

        var scrollSess = CreateScrollView(colInfo.transform, "SessionsScroll", out var contentSess);
        SetAnchors(scrollSess, 0.02f, 0.54f, 0.98f, 0.88f);

        var lblAnalytics = CreateLabel(colInfo.transform, "LblAnalytique", "Analytique", 13);
        SetAnchors(lblAnalytics, 0.02f, 0.46f, 0.98f, 0.52f);

        var scrollAna = CreateScrollView(colInfo.transform, "AnalyticsScroll", out var contentAna);
        SetAnchors(scrollAna, 0.02f, 0.06f, 0.98f, 0.44f);

        // ─── Bottom buttons ─────────────────────────────────────
        var btnFermer = CreateButton(loginPanel.transform, "Btn_Fermer", "Fermer", BtnRed, 0, 0);
        SetAnchors(btnFermer.gameObject, 0.02f, 0.005f, 0.22f, 0.06f);

        var btnStart = CreateButton(loginPanel.transform, "Btn_StartSession", "Démarrer la séance", BtnGreen, 0, 0);
        SetAnchors(btnStart.gameObject, 0.72f, 0.005f, 0.98f, 0.06f);

        // ─── Configuration panel ────────────────────────────────
        var configPanel = CreateGO("ConfigurationPanel", root.transform);
        Stretch(configPanel);
        configPanel.AddComponent<Image>().color = PanelBg;

        var cfgLabel = CreateLabel(configPanel.transform, "LblConfig", "Configuration", 20);
        SetAnchors(cfgLabel, 0.2f, 0.45f, 0.8f, 0.6f);

        var btnRetour = CreateButton(configPanel.transform, "Btn_Retour", "Retour", BtnTeal, 0, 0);
        SetAnchors(btnRetour.gameObject, 0.35f, 0.2f, 0.65f, 0.3f);
        configPanel.SetActive(false);

        // ─── Wire ConfigurationPanelUI ──────────────────────────
        var cfgUI = configPanel.AddComponent<ConfigurationPanelUI>();
        var soCfg = new SerializedObject(cfgUI);
        SetRef(soCfg, "RetourButton",        btnRetour);
        SetRef(soCfg, "ConfigurationPanel",   configPanel);
        SetRef(soCfg, "LoginPanel",           loginPanel);
        soCfg.ApplyModifiedPropertiesWithoutUndo();

        // ─── Wire DoctorLoginUI ─────────────────────────────────
        var so = new SerializedObject(doctorLogin);
        SetRef(so, "LoginPanel",             loginPanel);
        SetRef(so, "ConfigurationPanel",     configPanel);
        SetRef(so, "CommencerButton",        commencerBtn);
        SetRef(so, "SupervisorSearchLegacy", searchSup.GetComponent<InputField>());
        SetRef(so, "SupervisorsContent",     contentSup);
        SetRef(so, "AddSupervisorButton",    btnAddSup);
        SetRef(so, "PatientSearchLegacy",    searchPat.GetComponent<InputField>());
        SetRef(so, "PatientsContent",        contentPat);
        SetRef(so, "AddPatientButton",       btnAddPat);
        SetRef(so, "SessionsContent",        contentSess);
        SetRef(so, "AnalyticsContent",       contentAna);
        SetRef(so, "SelectedSupervisorText", lblSup.GetComponent<Text>());
        SetRef(so, "SelectedPatientText",    lblPat.GetComponent<Text>());
        SetRef(so, "StartSessionButton",     btnStart);
        SetRef(so, "FermerButton",           btnFermer);
        so.ApplyModifiedPropertiesWithoutUndo();

        loginPanel.SetActive(false);

        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
        Debug.Log("✓ Commencer menu créé avec succès.\n" +
                  "  → Assignez AddSupervisorUI et AddPatientUI manuellement si nécessaire.\n" +
                  "  → Les listes se rempliront au runtime depuis les fichiers JSON.");
    }

    // ═══════════════════════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════════════════════

    static void SetRef(SerializedObject so, string prop, Object value)
    {
        var p = so.FindProperty(prop);
        if (p != null)
            p.objectReferenceValue = value;
        else
            Debug.LogWarning($"CreateCommencer: propriété '{prop}' introuvable sur {so.targetObject.GetType().Name}.");
    }

    static GameObject CreateGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.layer = UILayer;
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void Stretch(GameObject go)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(xMin, yMin);
        r.anchorMax = new Vector2(xMax, yMax);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    static GameObject CreateColumn(Transform parent, string name, float xMin, float xMax)
    {
        var go = CreateGO(name, parent);
        go.AddComponent<Image>().color = ColBg;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(xMin + 0.005f, 0.07f);
        r.anchorMax = new Vector2(xMax - 0.005f, 0.98f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject CreateLabel(Transform parent, string name, string text, int fontSize)
    {
        var go = CreateGO(name, parent);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleLeft;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    static GameObject CreateSearchField(Transform parent, string name, string placeholder)
    {
        var go = CreateGO(name, parent);
        var img = go.AddComponent<Image>();
        img.color = SearchBg;
        img.raycastTarget = true;
        var input = go.AddComponent<InputField>();

        var textGo = CreateGO("Text", go.transform);
        var textR = textGo.GetComponent<RectTransform>();
        textR.anchorMin = Vector2.zero;
        textR.anchorMax = Vector2.one;
        textR.offsetMin = new Vector2(10, 2);
        textR.offsetMax = new Vector2(-10, -2);
        var txt = textGo.AddComponent<Text>();
        txt.text = "";
        txt.fontSize = 14;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        input.textComponent = txt;

        var phGo = CreateGO("Placeholder", go.transform);
        var phR = phGo.GetComponent<RectTransform>();
        phR.anchorMin = Vector2.zero;
        phR.anchorMax = Vector2.one;
        phR.offsetMin = new Vector2(10, 2);
        phR.offsetMax = new Vector2(-10, -2);
        var phTxt = phGo.AddComponent<Text>();
        phTxt.text = placeholder;
        phTxt.fontSize = 14;
        phTxt.fontStyle = FontStyle.Italic;
        phTxt.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        phTxt.alignment = TextAnchor.MiddleLeft;
        phTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        input.placeholder = phTxt;

        return go;
    }

    /// <summary>
    /// ScrollView без VerticalLayoutGroup — DoctorLoginUI вручную позиционирует строки.
    /// </summary>
    static GameObject CreateScrollView(Transform parent, string name, out RectTransform content)
    {
        var go = CreateGO(name, parent);
        var bgImg = go.AddComponent<Image>();
        bgImg.color = ScrollBg;
        bgImg.raycastTarget = true;
        var scroll = go.AddComponent<ScrollRect>();

        var viewport = CreateGO("Viewport", go.transform);
        Stretch(viewport);
        var vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(1, 1, 1, 0.005f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        var contentGo = CreateGO("Content", viewport.transform);
        var cRect = contentGo.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0, 1);
        cRect.anchorMax = new Vector2(1, 1);
        cRect.pivot = new Vector2(0.5f, 1);
        cRect.anchoredPosition = Vector2.zero;
        cRect.sizeDelta = new Vector2(0, 0);

        scroll.content = cRect;
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        content = cRect;
        return go;
    }

    static Button CreateButton(Transform parent, string name, string label, Color color, float w, float h)
    {
        var go = CreateGO(name, parent);
        go.AddComponent<Image>().color = color;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(color.r + 0.1f, color.g + 0.1f, color.b + 0.1f, 1f);
        colors.pressedColor = new Color(color.r - 0.05f, color.g - 0.05f, color.b - 0.05f, 1f);
        btn.colors = colors;

        if (w > 0 && h > 0)
        {
            var r = go.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(w, h);
        }

        var textGo = CreateGO("Text", go.transform);
        Stretch(textGo);
        var textR = textGo.GetComponent<RectTransform>();
        textR.offsetMin = new Vector2(4, 2);
        textR.offsetMax = new Vector2(-4, -2);
        var txt = textGo.AddComponent<Text>();
        txt.text = label;
        txt.fontSize = 14;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btn;
    }
}
