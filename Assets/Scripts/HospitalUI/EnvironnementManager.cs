using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Hospital.Data.Models;

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
                    Debug.LogError("EnvironnementManager n'est pas présent dans la scène !");
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
    }

    public List<EnvironnementConfig> GetAllEnvironnements()
    {
        return _environnements;
    }

    public EnvironnementConfig GetEnvironnementById(string id)
    {
        return _environnements.FirstOrDefault(e => e.IdEnvironnement == id);
    }

    public void LancerSession(ConfigurationEnvironnement config)
    {
        var environnement = GetEnvironnementById(config.IdEnvironnement);
        if (environnement == null)
        {
            Debug.LogError($"Environnement {config.IdEnvironnement} introuvable !");
            return;
        }
        _sessionEnCours = config;
        ChargerScene(environnement.NomScene);
    }

    private void ChargerScene(string nomScene)
    {
        if (string.IsNullOrEmpty(nomScene))
        {
            Debug.LogError("Nom de scène vide !");
            return;
        }
        SceneManager.LoadScene(nomScene);
    }

    public ConfigurationEnvironnement GetSessionEnCours()
    {
        return _sessionEnCours;
    }

    public PositionDepart GetPositionDepartActuelle()
    {
        if (_sessionEnCours == null) return null;
        var environnement = GetEnvironnementById(_sessionEnCours.IdEnvironnement);
        if (environnement == null) return null;
        return environnement.PositionsDisponibles
            .FirstOrDefault(p => p.IdPosition == _sessionEnCours.PositionDepart);
    }

    public void RetourMenuPrincipal()
    {
        _sessionEnCours = null;
        SceneManager.LoadScene("SampleScene");
    }
}
