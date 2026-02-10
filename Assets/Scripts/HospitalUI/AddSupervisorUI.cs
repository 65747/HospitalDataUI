using UnityEngine;
using TMPro;
using Hospital.Data.Models;
using Hospital.Data.Storage;

/// <summary>
/// Small UI controller to add a superviseur using TMP fields.
/// Attach to a GameObject and assign the TMP_InputField fields and StatusText.
/// </summary>
public class AddSupervisorUI : MonoBehaviour
{
    public GameObject FormPanel;
    public TMP_InputField Nom;
    public TMP_InputField Prenom;
    public TMP_InputField Fonction;
    public TextMeshProUGUI StatusText;

    [Header("Optional initial canvas buttons to hide")]
    public GameObject InitialAddButton;
    public GameObject InitialCancelButton;

    void Awake()
    {
        if (FormPanel) FormPanel.SetActive(false);

        // Hide initial buttons at startup so the panel stays closed until ShowForm() is called
        if (InitialAddButton) InitialAddButton.SetActive(false);
        if (InitialCancelButton) InitialCancelButton.SetActive(false);
    }

    // Extra safeguard in Start to ensure the panel is closed even if inspector or other scripts activated it earlier
    void Start()
    {
        if (FormPanel) FormPanel.SetActive(false);
    }

    public void ShowForm()
    {
        if (FormPanel == null) return;
        FormPanel.SetActive(true);
        ClearStatus();
        // Show optional initial canvas buttons when form opens
        if (InitialAddButton) InitialAddButton.SetActive(true);
        if (InitialCancelButton) InitialCancelButton.SetActive(true);
        MainMenuButtonsController.Instance?.OnMenuOpened();
    }

    public void HideForm()
    {
        if (FormPanel == null) return;
        FormPanel.SetActive(false);
        // Hide optional initial canvas buttons when form is closed
        if (InitialAddButton) InitialAddButton.SetActive(false);
        if (InitialCancelButton) InitialCancelButton.SetActive(false);
        MainMenuButtonsController.Instance?.OnMenuClosed();
    }

    public void OnAddSupervisor()
    {
        var s = new SuperviseurJson
        {
            Nom = Nom?.text ?? string.Empty,
            Prenom = Prenom?.text ?? string.Empty,
            fonction = Fonction?.text ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(s.Nom) || string.IsNullOrWhiteSpace(s.Prenom))
        {
            SetStatus("Nom et Prénom requis", true);
            return;
        }

        try
        {
            var added = HospitalDataService.Instance.Superviseurs.Add(s);
            SetStatus($"Superviseur ajouté : {added.IdSuperviseur}", false);
            ClearFormExceptStatus();
        }
        catch (System.Exception ex)
        {
            SetStatus($"Erreur: {ex.Message}", true);
        }
    }

    void SetStatus(string text, bool isError)
    {
        if (StatusText == null) return;
        StatusText.text = text;
        StatusText.color = isError ? Color.red : Color.green;
    }

    void ClearStatus()
    {
        if (StatusText == null) return;
        StatusText.text = string.Empty;
    }

    void ClearFormExceptStatus()
    {
        if (Nom) Nom.text = string.Empty;
        if (Prenom) Prenom.text = string.Empty;
        if (Fonction) Fonction.text = string.Empty;
    }
}
