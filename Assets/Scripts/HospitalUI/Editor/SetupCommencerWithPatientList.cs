using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Outil d'éditeur : intègre la liste patients (Séances + Analytique) dans la panneau Commencer
/// et ajoute le bouton « Ajouter patient » comme pour « Ajouter superviseur ».
/// Menu : Hospital UI / Setup Commencer — liste patient intégrée + bouton Add Patient
/// </summary>
public static class SetupCommencerWithPatientList
{
    [MenuItem("Hospital UI/Setup Commencer — liste patient intégrée + bouton Add Patient")]
    public static void Run()
    {
        var doctorLogin = Object.FindObjectOfType<DoctorLoginUI>();
        if (doctorLogin == null)
        {
            Debug.LogError("Setup Commencer: aucun DoctorLoginUI dans la scène. Ouvrez la scène avec le panneau Commencer.");
            return;
        }

        var patientsListUI = Object.FindObjectOfType<PatientsListUI>();
        Transform listPanelTransform = null;
        if (patientsListUI != null && patientsListUI.ListPanel != null)
            listPanelTransform = patientsListUI.ListPanel.transform;

        GameObject loginPanel = doctorLogin.LoginPanel;
        if (loginPanel == null)
        {
            Debug.LogError("Setup Commencer: DoctorLoginUI.LoginPanel n'est pas assigné.");
            return;
        }

        int done = 0;

        // 1) Déplacer le panneau liste (PatientsListPanel) dans le panneau Commencer
        if (listPanelTransform != null && listPanelTransform.parent != loginPanel.transform)
        {
            Undo.SetTransformParent(listPanelTransform, loginPanel.transform, "Reparent list into Commencer");
            var rect = listPanelTransform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.65f, 0.1f);
                rect.anchorMax = new Vector2(1f, 0.9f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            done++;
            Debug.Log("Setup Commencer: panneau liste patient déplacé dans Commencer.");
        }
        else if (listPanelTransform == null)
            Debug.LogWarning("Setup Commencer: PatientsListUI ou ListPanel introuvable — la liste ne sera pas déplacée.");

        // 2) Lier SessionsContent / AnalyticsContent de la liste au DoctorLoginUI (même contenu = séances du patient sélectionné)
        if (patientsListUI != null)
        {
            var so = new SerializedObject(doctorLogin);
            if (patientsListUI.SessionsContent != null)
            {
                var prop = so.FindProperty("SessionsContent");
                if (prop != null && prop.objectReferenceValue != patientsListUI.SessionsContent)
                {
                    prop.objectReferenceValue = patientsListUI.SessionsContent;
                    done++;
                }
            }
            if (patientsListUI.AnalyticsContent != null)
            {
                var prop = so.FindProperty("AnalyticsContent");
                if (prop != null && prop.objectReferenceValue != patientsListUI.AnalyticsContent)
                {
                    prop.objectReferenceValue = patientsListUI.AnalyticsContent;
                    done++;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // 3) Créer ou trouver le bouton « Ajouter patient » et l'assigner
        var addPatientUI = Object.FindObjectOfType<AddPatientUI>();
        Button addPatientBtn = doctorLogin.AddPatientButton;
        if (addPatientBtn == null)
        {
            addPatientBtn = CreateAddPatientButton(loginPanel.transform, doctorLogin.PatientsContent);
            if (addPatientBtn != null)
            {
                Undo.RegisterCreatedObjectUndo(addPatientBtn.gameObject, "Add Patient button");
                var so = new SerializedObject(doctorLogin);
                var prop = so.FindProperty("AddPatientButton");
                if (prop != null) prop.objectReferenceValue = addPatientBtn;
                so.ApplyModifiedPropertiesWithoutUndo();
                done++;
            }
        }
        if (addPatientUI != null)
        {
            var so = new SerializedObject(doctorLogin);
            var prop = so.FindProperty("AddPatientUI");
            if (prop != null && prop.objectReferenceValue != addPatientUI)
            {
                prop.objectReferenceValue = addPatientUI;
                so.ApplyModifiedPropertiesWithoutUndo();
                done++;
            }
        }
        else
            Debug.LogWarning("Setup Commencer: AddPatientUI introuvable dans la scène — assignez-le manuellement à DoctorLoginUI.");

        // 4) Cacher le bouton « Liste » pour que la liste ne s'ouvre que via Commencer
        if (patientsListUI != null && patientsListUI.ShowListButton != null)
        {
            patientsListUI.ShowListButton.gameObject.SetActive(false);
            done++;
        }

        Debug.Log($"Setup Commencer: terminé ({done} modification(s)). Vérifiez les références DoctorLoginUI (Add Patient, SessionsContent, AnalyticsContent).");
    }

    static Button CreateAddPatientButton(Transform parent, RectTransform nearPatientsContent)
    {
        var go = new GameObject("Button_AddPatient");
        go.layer = 5;
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 24f);
        rect.sizeDelta = new Vector2(180f, 28f);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.4f, 0.3f, 0.95f);

        var btn = go.AddComponent<Button>();

        var textGo = new GameObject("Text");
        textGo.layer = 5;
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(4, 2);
        textRect.offsetMax = new Vector2(-4, -2);
        var text = textGo.AddComponent<Text>();
        text.text = "Ajouter patient";
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btn;
    }
}
