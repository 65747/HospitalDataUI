using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Hospital.Data.Models;

/// <summary>
/// Interface de sélection et configuration d'environnement pour les sessions VR
/// Permet de choisir un environnement et configurer ses paramètres (position, difficulté, durée, etc.)
/// </summary>
public class EnvironnementSelectionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject FormPanel;
    public TMP_Dropdown EnvironnementDropdown;
    public TMP_Dropdown PositionDropdown;
    public TMP_Dropdown DifficulteDropdown;
    public TMP_InputField DureeInputField;
    public Slider NiveauAssistanceSlider;
    public TextMeshProUGUI NiveauAssistanceLabel;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI StatusText;

    [Header("Boutons")]
    public Button StartButton;
    public Button CancelButton;

    [Tooltip("Objet à réafficher quand le formulaire est fermé (ex: bouton Commencer).")]
    public GameObject ShowWhenFormClosed;

    private List<EnvironnementConfig> _environnementsDisponibles;
    private EnvironnementConfig _environnementSelectionne;
    private ConfigurationEnvironnement _configuration;

    void Awake()
    {
        if (FormPanel) FormPanel.SetActive(false);

        // Configurer le champ durée pour n'accepter que des nombres
        if (DureeInputField != null)
        {
            DureeInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        }

        // Configurer le slider d'assistance
        if (NiveauAssistanceSlider != null)
        {
            NiveauAssistanceSlider.minValue = 1;
            NiveauAssistanceSlider.maxValue = 10;
            NiveauAssistanceSlider.wholeNumbers = true;
            NiveauAssistanceSlider.onValueChanged.AddListener(OnAssistanceChanged);
        }

        // Configurer les dropdowns
        if (EnvironnementDropdown != null)
        {
            EnvironnementDropdown.onValueChanged.AddListener(OnEnvironnementChanged);
        }

        // Configurer les boutons
        if (StartButton != null)
        {
            StartButton.onClick.AddListener(OnStartSession);
        }
        if (CancelButton != null)
        {
            CancelButton.onClick.AddListener(HideForm);
        }
    }

    void Start()
    {
        // Si le panneau est déjà visible au chargement (ex: ouvert dans l'éditeur), remplir les listes
        if (FormPanel != null && FormPanel.activeInHierarchy)
            ChargerEnvironnements();
    }

    /// <summary>
    /// Affiche le formulaire de sélection d'environnement
    /// </summary>
    public void ShowForm()
    {
        if (FormPanel == null) return;

        ChargerEnvironnements();
        FormPanel.SetActive(true);
        ClearStatus();
        MainMenuButtonsController.Instance?.OnMenuOpened();
    }

    /// <summary>
    /// Masque le formulaire
    /// </summary>
    public void HideForm()
    {
        if (FormPanel == null) return;
        FormPanel.SetActive(false);
        if (ShowWhenFormClosed != null) ShowWhenFormClosed.SetActive(true);
        MainMenuButtonsController.Instance?.OnMenuClosed();
    }

    /// <summary>
    /// Charge les environnements disponibles depuis le manager
    /// </summary>
    private void ChargerEnvironnements()
    {
        // Toujours vider les dropdowns pour ne pas laisser "Option A, B, C" du template Unity
        if (EnvironnementDropdown != null) EnvironnementDropdown.ClearOptions();
        if (PositionDropdown != null) PositionDropdown.ClearOptions();
        if (DifficulteDropdown != null) DifficulteDropdown.ClearOptions();

        try
        {
            if (EnvironnementManager.Instance == null)
            {
                SetStatus("EnvironnementManager non trouvé !", true);
                Debug.LogError("EnvironnementManager n'est pas dans la scène !");
                if (EnvironnementDropdown != null)
                    EnvironnementDropdown.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("— EnvironnementManager manquant —") });
                return;
            }

            _environnementsDisponibles = EnvironnementManager.Instance.GetAllEnvironnements();

            if (EnvironnementDropdown != null)
            {
                var options = _environnementsDisponibles
                    .Select(e => new TMP_Dropdown.OptionData(e.NomEnvironnement ?? e.IdEnvironnement ?? "?"))
                    .ToList();
                EnvironnementDropdown.AddOptions(options);

                if (options.Count > 0)
                {
                    EnvironnementDropdown.value = 0;
                    OnEnvironnementChanged(0);
                }
            }
        }
        catch (System.Exception ex)
        {
            SetStatus($"Erreur lors du chargement des environnements: {ex.Message}", true);
            Debug.LogError($"Erreur ChargerEnvironnements: {ex}");
            if (EnvironnementDropdown != null)
                EnvironnementDropdown.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("— Erreur —") });
        }
    }

    /// <summary>
    /// Appelé quand l'environnement sélectionné change
    /// </summary>
    private void OnEnvironnementChanged(int index)
    {
        if (_environnementsDisponibles == null || index < 0 || index >= _environnementsDisponibles.Count)
            return;

        _environnementSelectionne = _environnementsDisponibles[index];

        // Mettre à jour la description
        if (DescriptionText != null)
        {
            DescriptionText.text = _environnementSelectionne.Description;
        }

        // Mettre à jour les positions disponibles
        if (PositionDropdown != null)
        {
            PositionDropdown.ClearOptions();
            var posOptions = _environnementSelectionne.PositionsDisponibles
                .Select(p => new TMP_Dropdown.OptionData(p.NomAffichage))
                .ToList();
            PositionDropdown.AddOptions(posOptions);
            if (posOptions.Count > 0) PositionDropdown.value = 0;
        }

        // Mettre à jour les niveaux de difficulté
        if (DifficulteDropdown != null)
        {
            DifficulteDropdown.ClearOptions();
            var diffOptions = _environnementSelectionne.NiveauxDifficulte
                .Select(d => new TMP_Dropdown.OptionData(FormatDifficulteName(d)))
                .ToList();
            DifficulteDropdown.AddOptions(diffOptions);
            if (diffOptions.Count > 0) DifficulteDropdown.value = 0;
        }

        // Mettre à jour la durée par défaut
        if (DureeInputField != null)
        {
            DureeInputField.text = _environnementSelectionne.DureeDefaut.ToString();
        }

        // Réinitialiser le niveau d'assistance
        if (NiveauAssistanceSlider != null)
        {
            NiveauAssistanceSlider.value = 1;
        }

        ClearStatus();
    }

    /// <summary>
    /// Appelé quand le slider d'assistance change
    /// </summary>
    private void OnAssistanceChanged(float value)
    {
        if (NiveauAssistanceLabel != null)
        {
            NiveauAssistanceLabel.text = $"Niveau d'assistance: {(int)value}";
        }
    }

    /// <summary>
    /// Appelé quand l'utilisateur clique sur "Démarrer"
    /// </summary>
    private void OnStartSession()
    {
        if (_environnementSelectionne == null)
        {
            SetStatus("Veuillez sélectionner un environnement", true);
            return;
        }

        // Validation de la durée
        if (!int.TryParse(DureeInputField?.text, out var duree) || duree <= 0)
        {
            SetStatus("Durée invalide (doit être un nombre positif)", true);
            return;
        }

        // Récupérer la position sélectionnée
        var positionSelectionnee = _environnementSelectionne.PositionsDisponibles[PositionDropdown.value];

        // Créer la configuration
        _configuration = new ConfigurationEnvironnement
        {
            IdEnvironnement = _environnementSelectionne.IdEnvironnement,
            PositionDepart = positionSelectionnee.IdPosition,
            NiveauDifficulte = _environnementSelectionne.NiveauxDifficulte[DifficulteDropdown.value],
            NiveauAssistance = (int)NiveauAssistanceSlider.value,
            Duree = duree
        };

        // Afficher un résumé de la configuration
        string resume = $"Environnement configuré:\n" +
                       $"- {_environnementSelectionne.NomEnvironnement}\n" +
                       $"- Position: {positionSelectionnee.NomAffichage}\n" +
                       $"- Difficulté: {FormatDifficulteName(_configuration.NiveauDifficulte)}\n" +
                       $"- Assistance: {_configuration.NiveauAssistance}\n" +
                       $"- Durée: {_configuration.Duree}s";

        SetStatus(resume, false);

        Debug.Log($"Configuration environnement créée: {_configuration.IdEnvironnement}");
        
        // Lancer la session via le EnvironnementManager
        if (EnvironnementManager.Instance != null)
        {
            EnvironnementManager.Instance.LancerSession(_configuration);
        }
        else
        {
            Debug.LogError("EnvironnementManager non trouvé !");
            SetStatus("Erreur: EnvironnementManager non trouvé", true);
        }
    }

    /// <summary>
    /// Récupère la configuration actuelle (utilisable par d'autres scripts)
    /// </summary>
    public ConfigurationEnvironnement GetConfiguration()
    {
        return _configuration;
    }

    /// <summary>
    /// Formate le nom d'une difficulté pour l'affichage
    /// </summary>
    private string FormatDifficulteName(string difficulte)
    {
        return difficulte switch
        {
            "facile" => "Facile",
            "moyen" => "Moyen",
            "difficile" => "Difficile",
            "expert" => "Expert",
            _ => difficulte
        };
    }

    private void SetStatus(string text, bool isError)
    {
        if (StatusText == null) return;
        StatusText.text = text;
        StatusText.color = isError ? Color.red : Color.green;
    }

    private void ClearStatus()
    {
        if (StatusText == null) return;
        StatusText.text = string.Empty;
    }
}
