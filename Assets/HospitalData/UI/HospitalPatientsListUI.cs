using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Hospital.Data.Storage;
using Hospital.Data.Models;

namespace Hospital.Data.Unity
{
    /// <summary>
    /// Меню: список пациентов с поиском по первым буквам имени/фамилии.
    /// При клике на пациента показываются все его сессии.
    /// 
    /// КАК НАСТРОИТЬ (см. UI_SETUP.md):
    /// 1. Создай Canvas → внутри Panel.
    /// 2. На Panel: InputField (поиск), ScrollView для пациентов (внутри Viewport → Content), блок для сессий (ScrollView с Content).
    /// 3. Этот скрипт повесь на Panel (или на любой объект в сцене).
    /// 4. В Inspector перетащи ссылки: SearchField, PatientsContent, SessionsContent.
    /// 5. Запусти сцену — список подтянется из HospitalDataService.
    /// </summary>
    public class HospitalPatientsListUI : MonoBehaviour
    {
        [Header("Références UI (glisser depuis la hiérarchie)")]
        [Tooltip("Champ de recherche : premières lettres du nom ou du prénom")]
        public InputField SearchField;

        [Tooltip("Content du ScrollView où apparaissent les boutons patients (parent des boutons)")]
        public RectTransform PatientsContent;

        [Tooltip("Content du ScrollView où apparaissent les sessions du patient sélectionné")]
        public RectTransform SessionsContent;

        [Header("Optionnel")]
        [Tooltip("Hauteur d'une ligne patient. Si 0, utilise 40.")]
        public float PatientRowHeight = 40f;

        const float SessionRowHeight = 32f;
        const float Padding = 8f;

        List<GameObject> _patientButtons = new List<GameObject>();
        List<GameObject> _sessionRows = new List<GameObject>();

        void Start()
        {
            if (SearchField != null)
                SearchField.onValueChanged.AddListener(OnSearchChanged);

            RefreshPatientsList("");
        }

        void OnSearchChanged(string text)
        {
            RefreshPatientsList(text ?? "");
        }

        void RefreshPatientsList(string searchPrefix)
        {
            if (PatientsContent == null) return;

            foreach (var go in _patientButtons)
            {
                if (go != null) Destroy(go);
            }
            _patientButtons.Clear();

            var data = HospitalDataService.Instance;
            var all = data.Patients.GetAll();

            string prefix = (searchPrefix ?? "").Trim();
            var filtered = string.IsNullOrEmpty(prefix)
                ? all
                : all.Where(p => MatchPrefix(p, prefix)).ToList();

            float rowHeight = PatientRowHeight > 0 ? PatientRowHeight : 40f;
            float y = -Padding;
            foreach (var p in filtered)
            {
                var btn = CreatePatientButton(p, y, rowHeight);
                if (btn != null)
                {
                    _patientButtons.Add(btn);
                    y -= rowHeight + Padding;
                }
            }

            float totalHeight = Mathf.Max(-y + Padding, 100f);
            PatientsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }

        static bool MatchPrefix(PatientJson p, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return true;
            string nom = p.Nom ?? "";
            string prenom = p.Prenom ?? "";
            string combined = $"{prenom} {nom}".Trim();
            string combined2 = $"{nom} {prenom}".Trim();
            return combined.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)
                   || combined2.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)
                   || nom.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)
                   || prenom.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase);
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

            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.35f, 0.95f);

            var btn = go.AddComponent<Button>();
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

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

            string idPatient = patient.IDpatient;
            btn.onClick.AddListener(() => ShowSessionsForPatient(idPatient));

            return go;
        }

        void ShowSessionsForPatient(string idPatient)
        {
            if (SessionsContent == null) return;

            foreach (var go in _sessionRows)
            {
                if (go != null) Destroy(go);
            }
            _sessionRows.Clear();

            var data = HospitalDataService.Instance;
            var sessions = data.Sessions.GetByPatient(idPatient);

            float y = -Padding;
            foreach (var s in sessions)
            {
                var row = CreateSessionRow(s, y);
                if (row != null)
                {
                    _sessionRows.Add(row);
                    y -= SessionRowHeight + 4f;
                }
            }

            float totalHeight = Mathf.Max(-y + Padding, 50f);
            SessionsContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
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

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.3f, 0.25f, 0.9f);

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
}
