using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hospital.Data.Storage;
using Hospital.Data.Models;

public class PatientsListUI : MonoBehaviour
{
    public GameObject ListPanel;
    public Button ShowListButton;
    public GameObject[] HideWhenListOpen;
    public TMP_InputField SearchFieldTMP;
    public InputField SearchFieldLegacy;
    public RectTransform PatientsContent;
    public RectTransform SessionsContent;
    public RectTransform AnalyticsContent;
    public Text AnalyticsText;
    public TMP_Text AnalyticsTextTMP;
    public float PatientRowHeight = 40f;

    const float SessionRowHeight = 32f;
    const float AnalyticsRowHeight = 28f;
    const float Padding = 8f;

    List<GameObject> _patientButtons = new List<GameObject>();
    List<GameObject> _sessionRows = new List<GameObject>();
    List<GameObject> _analyticsRows = new List<GameObject>();

    void Awake()
    {
        if (ListPanel != null) ListPanel.SetActive(false);
    }

    void Start()
    {
        if (ShowListButton != null)
            ShowListButton.onClick.AddListener(ShowPanel);

        if (SearchFieldTMP != null) SearchFieldTMP.onValueChanged.AddListener(_ => Refresh());
        if (SearchFieldLegacy != null) SearchFieldLegacy.onValueChanged.AddListener(_ => Refresh());

        if (ListPanel != null)
        {
            foreach (var btn in ListPanel.GetComponentsInChildren<Button>(true))
            {
                var t = btn.GetComponentInChildren<Text>(true);
                if (t != null && t.text?.Trim().Equals("Fermer", System.StringComparison.OrdinalIgnoreCase) == true)
                {
                    btn.onClick.AddListener(HidePanel);
                    break;
                }
            }
        }
    }

    public void ShowPanel()
    {
        if (ListPanel != null) ListPanel.SetActive(true);
        SetButtonsVisible(false);
        MainMenuButtonsController.Instance?.OnMenuOpened();
        Refresh();
    }

    public void HidePanel()
    {
        if (ListPanel != null) ListPanel.SetActive(false);
        SetButtonsVisible(true);
        MainMenuButtonsController.Instance?.OnMenuClosed();
    }

    void SetButtonsVisible(bool visible)
    {
        if (HideWhenListOpen == null) return;
        foreach (var go in HideWhenListOpen)
            if (go != null) go.SetActive(visible);
    }

    string GetSearchText()
    {
        return SearchFieldTMP?.text ?? SearchFieldLegacy?.text ?? "";
    }

    void Refresh()
    {
        if (PatientsContent == null) return;

        foreach (var go in _patientButtons)
            if (go != null) Destroy(go);
        _patientButtons.Clear();

        var all = HospitalDataService.Instance.Patients.GetAll();
        var prefix = GetSearchText().Trim();
        var list = string.IsNullOrEmpty(prefix)
            ? all
            : all.Where(p => (p.Nom ?? "").StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)
                          || (p.Prenom ?? "").StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)).ToList();

        float h = PatientRowHeight > 0 ? PatientRowHeight : 40f;
        float y = -Padding;
        foreach (var p in list)
        {
            var btn = CreatePatientButton(p, y, h);
            _patientButtons.Add(btn);
            y -= h + Padding;
        }

        PatientsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(-y + Padding, 100f));
    }

    GameObject CreatePatientButton(PatientJson patient, float y, float height)
    {
        var go = new GameObject("Patient_" + patient.IDpatient);
        go.transform.SetParent(PatientsContent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(0, height);

        go.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.35f, 0.95f);
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
        btn.onClick.AddListener(() => ShowSessions(id));
        return go;
    }

    void ShowSessions(string idPatient)
    {
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
        UpdateAnalytics(sessions);
    }

    void UpdateAnalytics(IReadOnlyList<SessionJson> sessions)
    {
        if (AnalyticsContent != null)
        {
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

        if (AnalyticsText != null || AnalyticsTextTMP != null)
        {
            string text;
            if (sessions == null || sessions.Count == 0)
                text = "Aucune séance enregistrée.";
            else
            {
                int n = sessions.Count;
                double avgScore = sessions.Average(s => s.ScoreTotal);
                double avgDuree = sessions.Average(s => s.duree);
                int totalDuree = sessions.Sum(s => s.duree);
                double avgReaction = sessions.Average(s => s.TempsReaction);
                double avgPrecision = sessions.Average(s => s.PrecisionPointage);
                double avgAssistance = sessions.Average(s => s.NiveauAssistance_moyen);
                text = $"Séances : {n}\nScore moyen : {avgScore:F0}\nDurée moy. : {FormatDuree((int)avgDuree)}\nTemps total : {FormatDuree(totalDuree)}\nRéaction moy. : {avgReaction:F1} s\nPrécision moy. : {avgPrecision:F0} %\nAssistance moy. : {avgAssistance:F0}";
            }
            if (AnalyticsText != null) AnalyticsText.text = text;
            if (AnalyticsTextTMP != null) AnalyticsTextTMP.text = text;
        }
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
}
