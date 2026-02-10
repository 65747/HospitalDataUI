using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Hospital.Data.Models;
using Hospital.Data.Storage;

/// <summary>
/// Simple controller to show/hide a patient form and add the patient to the HospitalDataService.
/// - Attach this to a GameObject (ex: "PatientFormController").
/// - Configure the `FormPanel` with a child Canvas Panel containing the InputFields and an "Add" Button.
/// - Wire your scene "Add patient" Button to call `ShowForm()` on this component.
/// </summary>
public class AddPatientUI : MonoBehaviour
{
    [Header("UI Elements (assign in Inspector)")]
    public GameObject FormPanel;
    // Use TextMeshPro input fields for modern UI
    public TMP_InputField Nom;
    public TMP_InputField Prenom;
    public TMP_InputField AnneeNaissance;
    public TMP_Dropdown Sexe;
    public TMP_InputField Pathologie;
    public TMP_InputField CoteNeglige;
    public TMP_InputField Suivi;
    public TextMeshProUGUI StatusText;

    [Header("Optional initial canvas buttons to hide")]
    public GameObject InitialAddButton; // bouton "add patient" présent sur le premier Canvas
    public GameObject InitialCancelButton; // si présent, bouton cancel visible par défaut

    void Awake()
    {
        if (FormPanel) FormPanel.SetActive(false);

        // Hide initial buttons on first canvas if assigned
        if (InitialAddButton) InitialAddButton.SetActive(false);
        if (InitialCancelButton) InitialCancelButton.SetActive(false);

        // Configure AnneeNaissance to accept only numbers (TMP)
        if (AnneeNaissance != null)
        {
            AnneeNaissance.contentType = TMP_InputField.ContentType.IntegerNumber;
            AnneeNaissance.characterLimit = 4;
        }
    }

    // Called by the top-level "Add patient" button to open the form.
    public void ShowForm()
    {
        if (FormPanel == null) return;
        FormPanel.SetActive(true);
        ClearStatus();
        // Show optional initial canvas buttons when form opens
        if (InitialAddButton) InitialAddButton.SetActive(true);
        if (InitialCancelButton) InitialCancelButton.SetActive(true);
    }

    // Called by a "Cancel" button on the form to close it.
    public void HideForm()
    {
        if (FormPanel == null) return;
        FormPanel.SetActive(false);
        // Hide optional initial canvas buttons when form is closed
        if (InitialAddButton) InitialAddButton.SetActive(false);
        if (InitialCancelButton) InitialCancelButton.SetActive(false);
    }

    // Called by the "Add" button on the form.
    public void OnAddPatient()
    {
        // Basic validation: Nom/Prenom
        if (string.IsNullOrWhiteSpace(Nom?.text) || string.IsNullOrWhiteSpace(Prenom?.text))
        {
            SetStatus("Nom et Prénom requis", true);
            return;
        }

        // Year validation
        if (!int.TryParse(AnneeNaissance?.text, out var annee))
        {
            SetStatus("Année de naissance invalide (chiffres seulement)", true);
            return;
        }
        var yearNow = DateTime.UtcNow.Year;
        if (annee < 1900 || annee > yearNow)
        {
            SetStatus($"Année doit être entre 1900 et {yearNow}", true);
            return;
        }

        // Sexe validation: expect selection from dropdown (Homme/Femme)
        if (Sexe == null)
        {
            SetStatus("Champ sexe non configuré", true);
            return;
        }
        var choix = Sexe.options.Count > 0 ? (Sexe.options[Sexe.value].text ?? string.Empty) : string.Empty;
        var sexeRaw = choix.Trim().ToLowerInvariant();
        string sexeNorm = string.Empty;
        if (sexeRaw.Contains("hom")) sexeNorm = "Homme";
        else if (sexeRaw.Contains("fem")) sexeNorm = "Femme";
        else
        {
            SetStatus("Sexe invalide — choisissez 'Homme' ou 'Femme'", true);
            return;
        }

        var patient = new PatientJson
        {
            Nom = Nom?.text ?? string.Empty,
            Prenom = Prenom?.text ?? string.Empty,
            Sexe = sexeNorm,
            Pathologie = Pathologie?.text ?? string.Empty,
            CoteNeglige = CoteNeglige?.text ?? string.Empty,
            SuiviPatient = Suivi?.text ?? string.Empty,
            date_de_naissance = annee
        };

        try
        {
            var added = HospitalDataService.Instance.Patients.Add(patient);
            SetStatus($"Patient ajouté : {added.IDpatient}", false);
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
        if (AnneeNaissance) AnneeNaissance.text = string.Empty;
        if (Sexe) Sexe.value = 0;
        if (Pathologie) Pathologie.text = string.Empty;
        if (CoteNeglige) CoteNeglige.text = string.Empty;
        if (Suivi) Suivi.text = string.Empty;
    }
}
