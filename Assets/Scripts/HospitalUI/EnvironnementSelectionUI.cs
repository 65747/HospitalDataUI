using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Hospital.Data.Models;
using Hospital.Data.Storage;

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

    private List<EnvironnementJson> _environnementsDisponibles;
    private EnvironnementJson _environnementSelectionne;
    private ConfigurationEnvironnement _configuration;

    void Awake()
    {
        if (FormPanel) FormPanel.SetActive(false);

        // Configurer le champ durée pour n'accepter que des nombres
        if (DureeInputField != null)
        {
            DureeInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            EnsureDureeInputClickable();
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

    /// <summary>
    /// Affiche le formulaire de sélection d'environnement. Cache le bouton Commencer.
    /// </summary>
    public void ShowForm()
    {
        if (FormPanel == null) return;

        FormPanel.SetActive(true);
        ClearStatus();
        MainMenuButtonsController.Instance?.OnMenuOpened();
        var doctorLogin = Object.FindObjectOfType<DoctorLoginUI>();
        if (doctorLogin != null && doctorLogin.CommencerButton != null)
            doctorLogin.CommencerButton.gameObject.SetActive(false);
        ChargerEnvironnements();
    }

    /// <summary>
    /// Masque le formulaire. Si Cancel — retour à la menue Commencer (pas à l'écran initial).
    /// </summary>
    public void HideForm()
    {
        if (FormPanel == null) return;
        FormPanel.SetActive(false);
        MainMenuButtonsController.Instance?.OnMenuClosed();
        var doctorLogin = Object.FindObjectOfType<DoctorLoginUI>();
        if (doctorLogin != null)
            doctorLogin.ShowLoginPanel();
    }

    /// <summary>
    /// Charge les environnements disponibles depuis le service
    /// </summary>
    private void ChargerEnvironnements()
    {
        try
        {
            var basePath = HospitalDataService.Instance?.BasePath ?? "(null)";
            Debug.Log($"[EnvironnementSelectionUI] Chargement depuis: {basePath}");

            _environnementsDisponibles = HospitalDataService.Instance.Environnements.GetAll().ToList();
            if (_environnementsDisponibles == null) _environnementsDisponibles = new List<EnvironnementJson>();

            if (EnvironnementDropdown != null)
            {
                EnvironnementDropdown.ClearOptions();
                var options = _environnementsDisponibles
                    .Select(e => new TMP_Dropdown.OptionData(e.NomEnvironnement))
                    .ToList();

                if (options.Count == 0)
                {
                    options.Add(new TMP_Dropdown.OptionData("— Aucun environnement —"));
                    SetStatus("Aucun environnement chargé. Vérifiez que le fichier environnements.json existe dans Assets/HospitalData/StreamingAssets/", true);
                    Debug.LogWarning("[EnvironnementSelectionUI] Liste vide. Chemin attendu: " + System.IO.Path.Combine(basePath, "environnements.json"));
                }

                EnvironnementDropdown.AddOptions(options);
                EnvironnementDropdown.value = 0;
                EnvironnementDropdown.RefreshShownValue();
                if (_environnementsDisponibles.Count > 0)
                    OnEnvironnementChanged(0);
            }
        }
        catch (System.Exception ex)
        {
            SetStatus($"Erreur lors du chargement des environnements: {ex.Message}", true);
            Debug.LogError($"Erreur ChargerEnvironnements: {ex}");
            if (EnvironnementDropdown != null)
            {
                EnvironnementDropdown.ClearOptions();
                EnvironnementDropdown.AddOptions(new System.Collections.Generic.List<string> { "— Erreur de chargement —" });
                EnvironnementDropdown.RefreshShownValue();
            }
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
        if (PositionDropdown != null && _environnementSelectionne.PositionsDisponibles != null)
        {
            PositionDropdown.ClearOptions();
            var posOptions = _environnementSelectionne.PositionsDisponibles
                .Select(p => new TMP_Dropdown.OptionData(FormatPositionName(p)))
                .ToList();
            PositionDropdown.AddOptions(posOptions);
            if (posOptions.Count > 0)
            {
                PositionDropdown.value = 0;
                PositionDropdown.RefreshShownValue();
            }
        }

        // Mettre à jour les niveaux de difficulté
        if (DifficulteDropdown != null && _environnementSelectionne.NiveauxDifficulte != null)
        {
            DifficulteDropdown.ClearOptions();
            var diffOptions = _environnementSelectionne.NiveauxDifficulte
                .Select(d => new TMP_Dropdown.OptionData(FormatDifficulteName(d)))
                .ToList();
            DifficulteDropdown.AddOptions(diffOptions);
            if (diffOptions.Count > 0)
            {
                DifficulteDropdown.value = 0;
                DifficulteDropdown.RefreshShownValue();
            }
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

        // Créer la configuration
        _configuration = new ConfigurationEnvironnement
        {
            IdEnvironnement = _environnementSelectionne.IdEnvironnement,
            PositionDepart = _environnementSelectionne.PositionsDisponibles[PositionDropdown.value],
            NiveauDifficulte = _environnementSelectionne.NiveauxDifficulte[DifficulteDropdown.value],
            NiveauAssistance = (int)NiveauAssistanceSlider.value,
            Duree = duree
        };

        // Afficher un résumé de la configuration
        string resume = $"Environnement configuré:\n" +
                       $"- {_environnementSelectionne.NomEnvironnement}\n" +
                       $"- Position: {FormatPositionName(_configuration.PositionDepart)}\n" +
                       $"- Difficulté: {FormatDifficulteName(_configuration.NiveauDifficulte)}\n" +
                       $"- Assistance: {_configuration.NiveauAssistance}\n" +
                       $"- Durée: {_configuration.Duree}s";

        SetStatus(resume, false);

        Debug.Log($"Configuration environnement créée: {_configuration.IdEnvironnement}");
        
        // TODO: Ici vous pouvez déclencher le lancement de la session VR
        // Par exemple: StartVRSession(_configuration);
    }

    /// <summary>
    /// Récupère la configuration actuelle (utilisable par d'autres scripts)
    /// </summary>
    public ConfigurationEnvironnement GetConfiguration()
    {
        return _configuration;
    }

    /// <summary>
    /// Formate le nom d'une position pour l'affichage
    /// </summary>
    private string FormatPositionName(string position)
    {
        return position switch
        {
            "assis" => "Assis",
            "debout" => "Debout",
            "allonge" => "Allongé",
            "assise-centre" => "Assis (Centre)",
            _ => position
        };
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

    /// <summary>
    /// S'assure que le champ Durée est cliquable (parent Image ne bloque pas les raycasts)
    /// et que le texte/placeholder sont visibles.
    /// </summary>
    private void EnsureDureeInputClickable()
    {
        if (DureeInputField == null) return;

        var rt = DureeInputField.GetComponent<RectTransform>();
        if (rt != null && rt.parent != null)
        {
            var parentImage = rt.parent.GetComponent<UnityEngine.UI.Image>();
            if (parentImage != null)
            {
                parentImage.raycastTarget = false;
            }
        }

        if (DureeInputField.textComponent != null)
        {
            DureeInputField.textComponent.color = Color.black;
        }
        if (DureeInputField.placeholder != null)
        {
            var ph = DureeInputField.placeholder as TextMeshProUGUI;
            if (ph != null)
            {
                ph.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
            }
        }
    }
}
