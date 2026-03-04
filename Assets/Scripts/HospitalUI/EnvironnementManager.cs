using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Hospital.Data.Models;

/// <summary>
/// Manager central pour gérer tous les environnements de jeu
/// Contient les configurations de scènes et positions de départ
/// </summary>
public class EnvironnementManager : MonoBehaviour
{
    private static EnvironnementManager _instance;
    
    public static EnvironnementManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EnvironnementManager>();
                if (_instance == null)
                {
                    Debug.LogError("EnvironnementManager n'est pas présent dans la scène !");
                }
            }
            return _instance;
        }
    }

    [Header("Environnements disponibles")]
    [SerializeField]
    private List<EnvironnementConfig> _environnements = new List<EnvironnementConfig>();

    [Header("Configuration de la session en cours")]
    private ConfigurationEnvironnement _sessionEnCours;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitialiserEnvironnements();
    }

    /// <summary>
    /// Initialise les environnements par défaut si la liste est vide
    /// </summary>
    private void InitialiserEnvironnements()
    {
        if (_environnements.Count > 0) return;

        // Forêt Virtuelle
        _environnements.Add(new EnvironnementConfig
        {
            IdEnvironnement = "env-forest",
            NomEnvironnement = "Forêt Virtuelle",
            Description = "Un environnement forestier calme avec sons naturels pour la rééducation visuelle",
            NomScene = "ForestScene",
            PositionsDisponibles = new List<PositionDepart>
            {
                new PositionDepart { IdPosition = "assis", NomAffichage = "Assis", Position = new Vector3(0, 0.5f, 0), Rotation = Vector3.zero },
                new PositionDepart { IdPosition = "debout", NomAffichage = "Debout", Position = new Vector3(0, 1.7f, 0), Rotation = Vector3.zero },
                new PositionDepart { IdPosition = "assise-centre", NomAffichage = "Assis (Centre)", Position = new Vector3(5, 0.5f, 5), Rotation = new Vector3(0, 45, 0) }
            },
            NiveauxDifficulte = new List<string> { "facile", "moyen", "difficile" },
            DureeDefaut = 600
        });

        // Hôpital Moderne
        _environnements.Add(new EnvironnementConfig
        {
            IdEnvironnement = "env-hospital",
            NomEnvironnement = "Hôpital Moderne",
            Description = "Environnement hospitalier réaliste pour simulation de vie quotidienne",
            NomScene = "HospitalScene",
            PositionsDisponibles = new List<PositionDepart>
            {
                new PositionDepart { IdPosition = "assis", NomAffichage = "Assis", Position = new Vector3(0, 0.5f, 0), Rotation = Vector3.zero },
                new PositionDepart { IdPosition = "debout", NomAffichage = "Debout", Position = new Vector3(0, 1.7f, 0), Rotation = Vector3.zero },
                new PositionDepart { IdPosition = "allonge", NomAffichage = "Allongé", Position = new Vector3(0, 0.2f, 0), Rotation = new Vector3(90, 0, 0) }
            },
            NiveauxDifficulte = new List<string> { "facile", "moyen", "difficile" },
            DureeDefaut = 900
        });

        // Centre Ville
        _environnements.Add(new EnvironnementConfig
        {
            IdEnvironnement = "env-city",
            NomEnvironnement = "Centre Ville",
            Description = "Simulation urbaine avec obstacles et distractions pour tester l'attention",
            NomScene = "CityScene",
            PositionsDisponibles = new List<PositionDepart>
            {
                new PositionDepart { IdPosition = "assis", NomAffichage = "Assis", Position = new Vector3(0, 0.5f, 0), Rotation = Vector3.zero },
                new PositionDepart { IdPosition = "debout", NomAffichage = "Debout", Position = new Vector3(0, 1.7f, 0), Rotation = Vector3.zero }
            },
            NiveauxDifficulte = new List<string> { "moyen", "difficile", "expert" },
            DureeDefaut = 1200
        });
    }

    /// <summary>
    /// Récupère tous les environnements disponibles
    /// </summary>
    public List<EnvironnementConfig> GetAllEnvironnements()
    {
        return _environnements;
    }

    /// <summary>
    /// Récupère un environnement par son ID
    /// </summary>
    public EnvironnementConfig GetEnvironnementById(string id)
    {
        return _environnements.FirstOrDefault(e => e.IdEnvironnement == id);
    }

    /// <summary>
    /// Lance une session avec la configuration donnée
    /// </summary>
    public void LancerSession(ConfigurationEnvironnement config)
    {
        var environnement = GetEnvironnementById(config.IdEnvironnement);
        if (environnement == null)
        {
            Debug.LogError($"Environnement {config.IdEnvironnement} introuvable !");
            return;
        }

        // Sauvegarder la configuration pour l'utiliser dans la scène chargée
        _sessionEnCours = config;

        Debug.Log($"=== Lancement de la session ===");
        Debug.Log($"Environnement: {environnement.NomEnvironnement}");
        Debug.Log($"Scène: {environnement.NomScene}");
        Debug.Log($"Position: {config.PositionDepart}");
        Debug.Log($"Difficulté: {config.NiveauDifficulte}");
        Debug.Log($"Assistance: {config.NiveauAssistance}");
        Debug.Log($"Durée: {config.Duree}s");
        Debug.Log($"==============================");

        // Charger la scène
        ChargerScene(environnement.NomScene);
    }

    /// <summary>
    /// Charge une scène Unity
    /// </summary>
    private void ChargerScene(string nomScene)
    {
        if (string.IsNullOrEmpty(nomScene))
        {
            Debug.LogError("Nom de scène vide !");
            return;
        }

        // Vérifier si la scène existe dans les Build Settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == nomScene)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogWarning($"La scène '{nomScene}' n'existe pas dans les Build Settings. Tentative de chargement quand même...");
        }

        try
        {
            SceneManager.LoadScene(nomScene);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erreur lors du chargement de la scène '{nomScene}': {ex.Message}");
        }
    }

    /// <summary>
    /// Récupère la configuration de la session en cours
    /// </summary>
    public ConfigurationEnvironnement GetSessionEnCours()
    {
        return _sessionEnCours;
    }

    /// <summary>
    /// Récupère la position de départ pour la configuration actuelle
    /// </summary>
    public PositionDepart GetPositionDepartActuelle()
    {
        if (_sessionEnCours == null) return null;

        var environnement = GetEnvironnementById(_sessionEnCours.IdEnvironnement);
        if (environnement == null) return null;

        return environnement.PositionsDisponibles
            .FirstOrDefault(p => p.IdPosition == _sessionEnCours.PositionDepart);
    }

    /// <summary>
    /// Retourne au menu principal
    /// </summary>
    public void RetourMenuPrincipal()
    {
        _sessionEnCours = null;
        SceneManager.LoadScene("SampleScene"); // Nom de votre scène de menu principal
    }
}
