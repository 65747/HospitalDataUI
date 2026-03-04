using UnityEngine;

/// <summary>
/// Place sur un GameObject dans la scène d'environnement pour définir une position de téléportation.
/// L'ID doit correspondre à un IdPosition choisi dans le menu (ex: "assis", "debout").
/// Le joueur sera téléporté sur la position et rotation de ce Transform au chargement de la scène.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("ID de la position (doit correspondre à une entrée dans EnvironnementManager, ex: assis, debout)")]
    public string IdPosition = "assis";

    [Tooltip("Nom affiché dans le menu (optionnel)")]
    public string NomAffichage;
}
