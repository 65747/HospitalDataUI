using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Utilisé par l'outil éditeur Create Doctor Login Panel pour passer les références au controller.
/// Détruit après création.
/// </summary>
public class DoctorLoginUIRefs : MonoBehaviour
{
    public Button CommencerButton;

    public RectTransform SupervisorsContent;
    public TMP_InputField SupervisorSearchTMP;
    public InputField SupervisorSearchLegacy;
    public Button AddSupervisorButton;

    public RectTransform PatientsContent;
    public TMP_InputField PatientSearchTMP;
    public InputField PatientSearchLegacy;

    public Button StartSessionButton;
    public Button FermerButton;

    public Text SelectedSupervisorText;
    public Text SelectedPatientText;
}

/// <summary>
/// Utilisé par l'outil éditeur pour la page Configuration.
/// </summary>
public class ConfigurationPanelUIRefs : MonoBehaviour
{
    public Button RetourButton;
}
