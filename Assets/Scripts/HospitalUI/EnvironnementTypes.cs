using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Position de départ possible dans un environnement.
/// Dans l'Inspector : ne définir que IdPosition et NomAffichage ; la position réelle vient des objets SpawnPoint dans la scène chargée.
/// Position/Rotation restent en repli si aucun SpawnPoint n'est trouvé (cachés dans l'Inspector).
/// </summary>
[System.Serializable]
public class PositionDepart
{
    [Tooltip("ID correspondant au SpawnPoint dans la scène d'environnement (ex: assis, debout)")]
    public string IdPosition;
    [Tooltip("Nom affiché dans le menu (ex: Assis, Debout)")]
    public string NomAffichage;

    [HideInInspector]
    public Vector3 Position;
    [HideInInspector]
    public Vector3 Rotation;
}

/// <summary>
/// Configuration d'un environnement (scène, positions, difficultés, durée par défaut).
/// </summary>
[System.Serializable]
public class EnvironnementConfig
{
    public string IdEnvironnement;
    public string NomEnvironnement;
    public string Description;
    public string NomScene;
    public List<PositionDepart> PositionsDisponibles = new List<PositionDepart>();
    public List<string> NiveauxDifficulte = new List<string>();
    public int DureeDefaut = 60;
}
