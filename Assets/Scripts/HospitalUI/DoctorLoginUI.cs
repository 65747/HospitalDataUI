using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hospital.Data.Storage;
using Hospital.Data.Models;

/// <summary>
/// Page de connexion médecin : recherche + liste des superviseurs, bouton « Ajouter superviseur »,
/// recherche + liste des patients. Bouton « Commencer » (externe) ouvre cette page ;
/// « Fermer » la referme. « Démarrer la séance » → page Configuration. Tout en français.
/// </summary>
public class DoctorLoginUI : MonoBehaviour
{
    [Header("Panneaux")]
    public GameObject LoginPanel;
    public GameObject ConfigurationPanel;

    [Header("Bouton Commencer (ouvre cette page)")]
    public Button CommencerButton;

    [Header("Superviseur — recherche et liste")]
    public TMP_InputField SupervisorSearchTMP;
    public InputField SupervisorSearchLegacy;
    public RectTransform SupervisorsContent;
    public Button AddSupervisorButton;
    public float SupervisorRowHeight = 36f;

    [Header("Référence formulaire « Ajouter superviseur » (optionnel)")]
    public AddSupervisorUI AddSupervisorUI;

    [Header("Patient — recherche et liste")]
    public TMP_InputField PatientSearchTMP;
    public InputField PatientSearchLegacy;
    public RectTransform PatientsContent;
    public Button AddPatientButton;
    public float PatientRowHeight = 40f;

    [Header("Référence formulaire « Ajouter patient » (optionnel)")]
    public AddPatientUI AddPatientUI;

    [Header("Info patient (Séances + Analytique, optionnel — affiché dans Commencer)")]
    public RectTransform SessionsContent;
    public RectTransform AnalyticsContent;

    [Header("Affichage du choix (optionnel)")]
    public Text SelectedSupervisorText;
    public TMP_Text SelectedSupervisorTextTMP;
    public Text SelectedPatientText;
    public TMP_Text SelectedPatientTextTMP;

    [Header("Actions")]
    public Button StartSessionButton;
    public Button FermerButton;

    const float Padding = 8f;
    const float SessionRowHeight = 32f;
    const float AnalyticsRowHeight = 28f;
    static readonly Color RowColorNormal = new Color(0.25f, 0.25f, 0.35f, 0.95f);
    static readonly Color RowColorSelected = new Color(0.35f, 0.4f, 0.55f, 1f);

    List<GameObject> _supervisorRows = new List<GameObject>();
    List<GameObject> _patientRows = new List<GameObject>();
    List<GameObject> _sessionRows = new List<GameObject>();
    List<GameObject> _analyticsRows = new List<GameObject>();
    GameObject _selectedSupervisorRow;
    GameObject _selectedPatientRow;

    /// <summary>ID du superviseur actuellement sélectionné.</summary>
    public string SelectedSupervisorId { get; private set; }

    /// <summary>ID du patient actuellement sélectionné.</summary>
    public string SelectedPatientId { get; private set; }

    void Start()
    {
        if (CommencerButton != null)
            CommencerButton.onClick.AddListener(ShowLoginPanel);

        if (FermerButton != null)
            FermerButton.onClick.AddListener(HideLoginPanel);

        if (AddSupervisorButton != null)
            AddSupervisorButton.onClick.AddListener(OnAddSupervisorClick);
        if (AddPatientButton != null)
            AddPatientButton.onClick.AddListener(OnAddPatientClick);

        if (StartSessionButton != null)
            StartSessionButton.onClick.AddListener(OnStartSessionClick);

        if (SupervisorSearchTMP != null)
            SupervisorSearchTMP.onValueChanged.AddListener(_ => RefreshSupervisors());
        if (SupervisorSearchLegacy != null)
            SupervisorSearchLegacy.onValueChanged.AddListener(_ => RefreshSupervisors());

        if (PatientSearchTMP != null)
            PatientSearchTMP.onValueChanged.AddListener(_ => RefreshPatients());
        if (PatientSearchLegacy != null)
            PatientSearchLegacy.onValueChanged.AddListener(_ => RefreshPatients());

        if (ConfigurationPanel != null)
            ConfigurationPanel.SetActive(false);
        if (LoginPanel != null)
            LoginPanel.SetActive(false);
        if (CommencerButton != null)
            CommencerButton.gameObject.SetActive(true);

        EnsureSearchFieldsVisible();
    }

    static Font GetDefaultFont()
    {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f != null) return f;
        f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (f != null) return f;
        return null;
    }

    void EnsureSearchFieldsVisible()
    {
        var font = GetDefaultFont();

        void FixInputField(InputField input)
        {
            if (input == null) return;
            // Si l'InputField est sur un enfant, le parent Image bloque les clics : désactiver son raycast.
            var parent = input.transform.parent;
            if (parent != null)
            {
                var parentImg = parent.GetComponent<Image>();
                if (parentImg != null && parentImg.gameObject != input.gameObject)
                    parentImg.raycastTarget = false;
            }
            if (font == null) return;
            if (input.textComponent != null)
            {
                input.textComponent.font = font;
                input.textComponent.color = Color.black;
                input.textComponent.enabled = true;
            }
            if (input.placeholder != null)
            {
                var ph = input.placeholder as Text;
                if (ph != null)
                {
                    ph.font = font;
                    ph.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    ph.enabled = true;
                }
            }
        }

        FixInputField(SupervisorSearchLegacy);
        FixInputField(PatientSearchLegacy);
    }

    void OnEnable()
    {
        RefreshSupervisors();
        RefreshPatients();
    }

    /// <summary>Ouvre la page de connexion (appelé par le bouton Commencer).</summary>
    public void ShowLoginPanel()
    {
        if (LoginPanel != null)
        {
            LoginPanel.transform.localScale = Vector3.one;
            LoginPanel.SetActive(true);
        }
        if (CommencerButton != null) CommencerButton.gameObject.SetActive(false);
        MainMenuButtonsController.Instance?.OnMenuOpened();
        RefreshSupervisors();
        RefreshPatients();
    }

    /// <summary>Ferme la page de connexion (bouton Fermer).</summary>
    public void HideLoginPanel()
    {
        if (LoginPanel != null)
            LoginPanel.SetActive(false);
        if (CommencerButton != null) CommencerButton.gameObject.SetActive(true);
        MainMenuButtonsController.Instance?.OnMenuClosed();
    }

    void OnAddSupervisorClick()
    {
        if (AddSupervisorUI != null)
            AddSupervisorUI.ShowForm();
        else
            Debug.LogWarning("DoctorLoginUI: assignez AddSupervisorUI pour le bouton « Ajouter superviseur ».");
    }

    void OnAddPatientClick()
    {
        if (AddPatientUI != null)
            AddPatientUI.ShowForm();
        else
            Debug.LogWarning("DoctorLoginUI: assignez AddPatientUI pour le bouton « Ajouter patient ».");
    }

    string GetSupervisorSearchText()
    {
        return SupervisorSearchTMP != null ? SupervisorSearchTMP.text ?? "" : SupervisorSearchLegacy != null ? SupervisorSearchLegacy.text ?? "" : "";
    }

    string GetPatientSearchText()
    {
        return PatientSearchTMP != null ? PatientSearchTMP.text ?? "" : PatientSearchLegacy != null ? PatientSearchLegacy.text ?? "" : "";
    }

    public void RefreshSupervisors()
    {
        if (SupervisorsContent == null)
        {
            Debug.LogWarning("DoctorLoginUI: SupervisorsContent non assigné — assignez le contenu de la liste superviseurs dans l'Inspector.");
            return;
        }

        foreach (var go in _supervisorRows)
            if (go != null) Destroy(go);
        _supervisorRows.Clear();

        List<SuperviseurJson> all;
        try
        {
            all = new List<SuperviseurJson>(HospitalDataService.Instance.Superviseurs.GetAll());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"DoctorLoginUI RefreshSupervisors: {e.Message}");
            return;
        }

        var prefix = GetSupervisorSearchText().Trim();
        var list = string.IsNullOrEmpty(prefix)
            ? all
            : all.Where(s =>
                (s.Nom ?? "").StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase) ||
                (s.Prenom ?? "").StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase) ||
                (s.fonction ?? "").Contains(prefix)).ToList();

        float h = SupervisorRowHeight > 0 ? SupervisorRowHeight : 36f;
        float y = -Padding;
        if (list.Count == 0)
        {
            var path = System.IO.Path.Combine(HospitalDataService.Instance?.BasePath ?? "", "les_superviseur.json");
            bool exists = System.IO.File.Exists(path);
            string msg = exists ? "Aucun superviseur dans le fichier." : "Fichier introuvable : Assets/HospitalData/StreamingAssets/les_superviseur.json";
            _supervisorRows.Add(CreatePlaceholderRow(SupervisorsContent, msg, y, h));
            y -= h + Padding;
            if (!exists) Debug.LogWarning($"DoctorLoginUI: {path}");
        }
        else
        {
            foreach (var s in list)
            {
                var row = CreateSupervisorRow(s, y, h);
                _supervisorRows.Add(row);
                y -= h + Padding;
            }
        }

        SupervisorsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(-y + Padding, 80f));

        _selectedSupervisorRow = null;
        if (!string.IsNullOrEmpty(SelectedSupervisorId))
        {
            foreach (var row in _supervisorRows)
                if (row != null && row.name == "Supervisor_" + SelectedSupervisorId)
                {
                    _selectedSupervisorRow = row;
                    var img = row.GetComponent<Image>();
                    if (img != null) img.color = RowColorSelected;
                    var lbl = row.GetComponentInChildren<Text>();
                    if (lbl != null) SetSelectedSupervisorLabel(lbl.text);
                    break;
                }
        }
        if (_selectedSupervisorRow == null) SetSelectedSupervisorLabel("");
    }

    void SetSelectedSupervisorLabel(string text)
    {
        var display = string.IsNullOrEmpty(text) ? "Aucun superviseur sélectionné" : "Superviseur : " + text;
        if (SelectedSupervisorText != null) SelectedSupervisorText.text = display;
        if (SelectedSupervisorTextTMP != null) SelectedSupervisorTextTMP.text = display;
    }

    static GameObject CreatePlaceholderRow(RectTransform parent, string message, float y, float height)
    {
        var go = new GameObject("PlaceholderRow");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(0, height);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 0.9f);
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 2);
        textRect.offsetMax = new Vector2(-10, -2);
        var text = textGo.AddComponent<Text>();
        text.text = message;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.color = new Color(0.9f, 0.85f, 0.7f);
        return go;
    }

    GameObject CreateSupervisorRow(SuperviseurJson s, float y, float height)
    {
        var go = new GameObject("Supervisor_" + s.IdSuperviseur);
        go.transform.SetParent(SupervisorsContent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(0, height);

        var rowImg = go.AddComponent<Image>();
        rowImg.color = RowColorNormal;
        var btn = go.AddComponent<Button>();

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 2);
        textRect.offsetMax = new Vector2(-10, -2);
        var text = textGo.AddComponent<Text>();
        text.text = string.IsNullOrEmpty(s.fonction)
            ? $"{s.Prenom} {s.Nom}"
            : $"{s.Prenom} {s.Nom} — {s.fonction}";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 15;
        text.color = Color.white;

        var id = s.IdSuperviseur;
        btn.onClick.AddListener(() => SelectSupervisor(id));
        return go;
    }

    void SelectSupervisor(string id)
    {
        SelectedSupervisorId = id;
        foreach (var row in _supervisorRows)
        {
            if (row == null) continue;
            var img = row.GetComponent<Image>();
            if (img != null) img.color = RowColorNormal;
            if (row.name == "Supervisor_" + id)
            {
                _selectedSupervisorRow = row;
                if (img != null) img.color = RowColorSelected;
            }
        }
        var label = _selectedSupervisorRow != null ? _selectedSupervisorRow.GetComponentInChildren<Text>() : null;
        SetSelectedSupervisorLabel(label != null ? label.text : "");
    }

    public void RefreshPatients()
    {
        if (PatientsContent == null)
        {
            Debug.LogWarning("DoctorLoginUI: PatientsContent non assigné — assignez le contenu de la liste patients dans l'Inspector.");
            return;
        }

        foreach (var go in _patientRows)
            if (go != null) Destroy(go);
        _patientRows.Clear();

        List<PatientJson> all;
        try
        {
            all = new List<PatientJson>(HospitalDataService.Instance.Patients.GetAll());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"DoctorLoginUI RefreshPatients: {e.Message}");
            return;
        }

        var prefix = GetPatientSearchText().Trim();
        var list = string.IsNullOrEmpty(prefix)
            ? all
            : all.Where(p =>
                (p.Nom ?? "").StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase) ||
                (p.Prenom ?? "").StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase) ||
                (p.Pathologie ?? "").Contains(prefix)).ToList();

        float h = PatientRowHeight > 0 ? PatientRowHeight : 40f;
        float y = -Padding;
        if (list.Count == 0)
        {
            var path = System.IO.Path.Combine(HospitalDataService.Instance?.BasePath ?? "", "les_patients.json");
            bool exists = System.IO.File.Exists(path);
            string msg = exists ? "Aucun patient dans le fichier." : "Fichier introuvable : Assets/HospitalData/StreamingAssets/les_patients.json";
            _patientRows.Add(CreatePlaceholderRow(PatientsContent, msg, y, h));
            y -= h + Padding;
            if (!exists) Debug.LogWarning($"DoctorLoginUI: {path}");
        }
        else
        {
            foreach (var p in list)
            {
                var row = CreatePatientRow(p, y, h);
                _patientRows.Add(row);
                y -= h + Padding;
            }
        }

        PatientsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(-y + Padding, 100f));

        _selectedPatientRow = null;
        if (!string.IsNullOrEmpty(SelectedPatientId))
        {
            foreach (var row in _patientRows)
                if (row != null && row.name == "Patient_" + SelectedPatientId)
                {
                    _selectedPatientRow = row;
                    var img = row.GetComponent<Image>();
                    if (img != null) img.color = RowColorSelected;
                    var lbl = row.GetComponentInChildren<Text>();
                    if (lbl != null) SetSelectedPatientLabel(lbl.text);
                    break;
                }
        }
        if (_selectedPatientRow == null) SetSelectedPatientLabel("");
    }

    void SetSelectedPatientLabel(string text)
    {
        var display = string.IsNullOrEmpty(text) ? "Aucun patient sélectionné" : "Patient : " + text;
        if (SelectedPatientText != null) SelectedPatientText.text = display;
        if (SelectedPatientTextTMP != null) SelectedPatientTextTMP.text = display;
    }

    GameObject CreatePatientRow(PatientJson patient, float y, float height)
    {
        var go = new GameObject("Patient_" + patient.IDpatient);
        go.transform.SetParent(PatientsContent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(0, height);

        var rowImg = go.AddComponent<Image>();
        rowImg.color = RowColorNormal;
        var btn = go.AddComponent<Button>();

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 2);
        textRect.offsetMax = new Vector2(-10, -2);
        var text = textGo.AddComponent<Text>();
        text.text = $"{patient.Prenom} {patient.Nom} — {patient.Pathologie}";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = Color.white;

        var id = patient.IDpatient;
        btn.onClick.AddListener(() => SelectPatient(id));
        return go;
    }

    void SelectPatient(string idPatient)
    {
        SelectedPatientId = idPatient;
        foreach (var row in _patientRows)
        {
            if (row == null) continue;
            var img = row.GetComponent<Image>();
            if (img != null) img.color = RowColorNormal;
            if (row.name == "Patient_" + idPatient)
            {
                _selectedPatientRow = row;
                if (img != null) img.color = RowColorSelected;
            }
        }
        var label = _selectedPatientRow != null ? _selectedPatientRow.GetComponentInChildren<Text>() : null;
        SetSelectedPatientLabel(label != null ? label.text : "");
        RefreshSessionsAndAnalyticsForPatient(idPatient);
    }

    void RefreshSessionsAndAnalyticsForPatient(string idPatient)
    {
        if (string.IsNullOrEmpty(idPatient)) return;
        if (SessionsContent == null) return;

        foreach (var go in _sessionRows)
            if (go != null) Destroy(go);
        _sessionRows.Clear();

        var sessions = HospitalDataService.Instance.Sessions.GetByPatient(idPatient);
        float y = -Padding;
        foreach (var s in sessions)
        {
            _sessionRows.Add(CreateSessionRow(s, y));
            y -= SessionRowHeight + 4f;
        }
        SessionsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(-y + Padding, 50f));
        UpdateAnalyticsForPatient(sessions);
    }

    void UpdateAnalyticsForPatient(IReadOnlyList<SessionJson> sessions)
    {
        if (AnalyticsContent == null) return;
        foreach (var go in _analyticsRows)
            if (go != null) Destroy(go);
        _analyticsRows.Clear();

        float y = -Padding;
        if (sessions == null || sessions.Count == 0)
        {
            _analyticsRows.Add(CreateAnalyticsRow("Aucune séance enregistrée.", y));
            y -= AnalyticsRowHeight + 4f;
        }
        else
        {
            int n = sessions.Count;
            double avgScore = sessions.Average(s => s.ScoreTotal);
            double avgDuree = sessions.Average(s => s.duree);
            int totalDuree = sessions.Sum(s => s.duree);
            double avgReaction = sessions.Average(s => s.TempsReaction);
            double avgPrecision = sessions.Average(s => s.PrecisionPointage);
            double avgAssistance = sessions.Average(s => s.NiveauAssistance_moyen);
            _analyticsRows.Add(CreateAnalyticsRow($"Séances : {n}", y)); y -= AnalyticsRowHeight + 4f;
            _analyticsRows.Add(CreateAnalyticsRow($"Score moyen : {avgScore:F0}", y)); y -= AnalyticsRowHeight + 4f;
            _analyticsRows.Add(CreateAnalyticsRow($"Durée moy. : {FormatDuree((int)avgDuree)}", y)); y -= AnalyticsRowHeight + 4f;
            _analyticsRows.Add(CreateAnalyticsRow($"Temps total : {FormatDuree(totalDuree)}", y)); y -= AnalyticsRowHeight + 4f;
            _analyticsRows.Add(CreateAnalyticsRow($"Réaction moy. : {avgReaction:F1} s", y)); y -= AnalyticsRowHeight + 4f;
            _analyticsRows.Add(CreateAnalyticsRow($"Précision moy. : {avgPrecision:F0} %", y)); y -= AnalyticsRowHeight + 4f;
            _analyticsRows.Add(CreateAnalyticsRow($"Assistance moy. : {avgAssistance:F0}", y)); y -= AnalyticsRowHeight + 4f;
        }
        AnalyticsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(-y + Padding, 60f));
    }

    static string FormatDuree(int secondes)
    {
        if (secondes < 60) return $"{secondes} s";
        int min = secondes / 60;
        int s = secondes % 60;
        return s > 0 ? $"{min} min {s} s" : $"{min} min";
    }

    GameObject CreateSessionRow(SessionJson session, float y)
    {
        var go = new GameObject("Session");
        go.transform.SetParent(SessionsContent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(0, SessionRowHeight);
        go.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.25f, 0.9f);
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8, 2);
        textRect.offsetMax = new Vector2(-8, -2);
        var text = textGo.AddComponent<Text>();
        text.text = $"{session.DateDebut:g} — {session.duree}s — Score {session.ScoreTotal} — {session.Commentaire}";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 13;
        text.color = new Color(0.9f, 0.9f, 0.9f);
        return go;
    }

    GameObject CreateAnalyticsRow(string line, float y)
    {
        var go = new GameObject("AnalyticsRow");
        go.transform.SetParent(AnalyticsContent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(0, AnalyticsRowHeight);
        go.AddComponent<Image>().color = new Color(0.22f, 0.28f, 0.35f, 0.92f);
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 2);
        textRect.offsetMax = new Vector2(-10, -2);
        var text = textGo.AddComponent<Text>();
        text.text = line;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.color = new Color(0.95f, 0.95f, 0.95f);
        return go;
    }

    void OnStartSessionClick()
    {
        if (string.IsNullOrEmpty(SelectedSupervisorId))
        {
            Debug.LogWarning("Sélectionnez un superviseur.");
            return;
        }
        if (string.IsNullOrEmpty(SelectedPatientId))
        {
            Debug.LogWarning("Sélectionnez un patient.");
            return;
        }

        if (LoginPanel != null) LoginPanel.SetActive(false);
        if (CommencerButton != null) CommencerButton.gameObject.SetActive(true);
        if (ConfigurationPanel != null) ConfigurationPanel.SetActive(true);
    }
}
