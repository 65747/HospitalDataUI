using UnityEngine;
using Hospital.Data.Models;

/// <summary>
/// Gère le positionnement du joueur lors du chargement d'une scène d'environnement.
/// Cherche un objet avec le composant SpawnPoint dont l'IdPosition correspond au choix de session ;
/// sinon utilise les coordonnées de repli (PositionDepart) si définies.
/// </summary>
public class PlayerPositionManager : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Transform du joueur/caméra à positionner (laissez vide pour utiliser la caméra principale)")]
    public Transform PlayerTransform;

    void Start()
    {
        PositionnerJoueur();
    }

    /// <summary>
    /// Positionne le joueur selon la configuration de session : d'abord sur un SpawnPoint de la scène (même IdPosition), sinon sur les coordonnées de repli.
    /// </summary>
    public void PositionnerJoueur()
    {
        if (EnvironnementManager.Instance == null)
        {
            Debug.LogWarning("EnvironnementManager non trouvé. Le joueur ne sera pas positionné.");
            return;
        }

        var config = EnvironnementManager.Instance.GetSessionEnCours();
        if (config == null || string.IsNullOrEmpty(config.PositionDepart))
        {
            Debug.LogWarning("Pas de position de départ définie dans la session en cours.");
            return;
        }

        string positionId = config.PositionDepart;

        // Utiliser la caméra principale si aucun transform n'est assigné
        if (PlayerTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                PlayerTransform = mainCamera.transform;
            else
            {
                Debug.LogError("Aucune caméra principale trouvée et aucun PlayerTransform assigné !");
                return;
            }
        }

        // 1) Chercher un SpawnPoint dans la scène avec le même IdPosition
        var spawnPoints = FindObjectsOfType<SpawnPoint>();
        foreach (var sp in spawnPoints)
        {
            if (sp != null && string.Equals(sp.IdPosition, positionId, System.StringComparison.OrdinalIgnoreCase))
            {
                PlayerTransform.SetPositionAndRotation(sp.transform.position, sp.transform.rotation);
                Debug.Log($"Joueur positionné sur le SpawnPoint \"{sp.IdPosition}\" ({sp.transform.position})");
                LogSessionInfo(config);
                return;
            }
        }

        // 2) Repli : utiliser les coordonnées de PositionDepart (config menu)
        var position = EnvironnementManager.Instance.GetPositionDepartActuelle();
        if (position != null)
        {
            PlayerTransform.position = position.Position;
            PlayerTransform.eulerAngles = position.Rotation;
            Debug.Log($"Joueur positionné (repli) à {position.NomAffichage}: {position.Position}");
        }
        else
            Debug.LogWarning($"Aucun SpawnPoint avec IdPosition=\"{positionId}\" dans la scène et pas de coordonnées de repli.");

        LogSessionInfo(config);
    }

    static void LogSessionInfo(ConfigurationEnvironnement config)
    {
        if (config == null) return;
        Debug.Log($"Session - Difficulté: {config.NiveauDifficulte}, Assistance: {config.NiveauAssistance}, Durée: {config.Duree}s");
    }

    /// <summary>
    /// Positionne le joueur à une position spécifique (utile pour déplacements en cours de jeu)
    /// </summary>
    public void PositionnerJoueurCustom(Vector3 position, Vector3 rotation)
    {
        if (PlayerTransform == null)
        {
            Debug.LogError("PlayerTransform non assigné !");
            return;
        }

        PlayerTransform.position = position;
        PlayerTransform.eulerAngles = rotation;
    }
}
