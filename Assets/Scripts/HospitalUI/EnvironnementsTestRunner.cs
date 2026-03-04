using UnityEngine;
using Hospital.Data.Storage;
using Hospital.Data.Models;
using System.Collections.Generic;

/// <summary>
/// Script de test pour vérifier le système de gestion d'environnements
/// Attachez ce script à un GameObject et cliquez sur les boutons dans l'Inspector (en mode Play)
/// ou appelez les méthodes depuis la console
/// </summary>
public class EnvironnementsTestRunner : MonoBehaviour
{
    [Header("Tests disponibles")]
    [Tooltip("Exécute tous les tests au démarrage")]
    public bool RunAllTestsOnStart = false;

    void Start()
    {
        if (RunAllTestsOnStart)
        {
            RunAllTests();
        }
    }

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        Debug.Log("=== DÉBUT DES TESTS ENVIRONNEMENTS ===");
        
        Test_1_LoadEnvironnements();
        Test_2_GetEnvironnementById();
        Test_3_ListAllEnvironnements();
        Test_4_TestEnvironnementDetails();
        Test_5_CreateConfiguration();
        
        Debug.Log("=== FIN DES TESTS ===");
    }

    [ContextMenu("Test 1: Charger les environnements")]
    public void Test_1_LoadEnvironnements()
    {
        Debug.Log("\n--- Test 1: Chargement des environnements ---");
        
        try
        {
            var environnements = HospitalDataService.Instance.Environnements.GetAll();
            Debug.Log($"✓ {environnements.Count} environnements chargés");
            
            if (environnements.Count == 0)
            {
                Debug.LogWarning("⚠ Aucun environnement trouvé. Vérifiez environnements.json");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Erreur : {ex.Message}");
        }
    }

    [ContextMenu("Test 2: Récupérer un environnement par ID")]
    public void Test_2_GetEnvironnementById()
    {
        Debug.Log("\n--- Test 2: Récupération par ID ---");
        
        try
        {
            var forest = HospitalDataService.Instance.Environnements.GetById("env-forest");
            
            if (forest != null)
            {
                Debug.Log($"✓ Environnement trouvé : {forest.NomEnvironnement}");
                Debug.Log($"  Description : {forest.Description}");
                Debug.Log($"  Positions : {string.Join(", ", forest.PositionsDisponibles)}");
            }
            else
            {
                Debug.LogWarning("⚠ Environnement 'env-forest' non trouvé");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Erreur : {ex.Message}");
        }
    }

    [ContextMenu("Test 3: Lister tous les environnements")]
    public void Test_3_ListAllEnvironnements()
    {
        Debug.Log("\n--- Test 3: Liste complète des environnements ---");
        
        try
        {
            var environnements = HospitalDataService.Instance.Environnements.GetAll();
            
            foreach (var env in environnements)
            {
                Debug.Log($"\n📍 {env.NomEnvironnement} ({env.IdEnvironnement})");
                Debug.Log($"   Description: {env.Description}");
                Debug.Log($"   Positions: {string.Join(", ", env.PositionsDisponibles)}");
                Debug.Log($"   Difficultés: {string.Join(", ", env.NiveauxDifficulte)}");
                Debug.Log($"   Durée défaut: {env.DureeDefaut}s");
            }
            
            Debug.Log($"\n✓ Total: {environnements.Count} environnements");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Erreur : {ex.Message}");
        }
    }

    [ContextMenu("Test 4: Détails des environnements")]
    public void Test_4_TestEnvironnementDetails()
    {
        Debug.Log("\n--- Test 4: Vérification des détails ---");
        
        try
        {
            var environnements = HospitalDataService.Instance.Environnements.GetAll();
            int erreurs = 0;
            
            foreach (var env in environnements)
            {
                // Vérifier les champs obligatoires
                if (string.IsNullOrEmpty(env.IdEnvironnement))
                {
                    Debug.LogError($"✗ Environnement sans ID");
                    erreurs++;
                }
                
                if (string.IsNullOrEmpty(env.NomEnvironnement))
                {
                    Debug.LogError($"✗ {env.IdEnvironnement} : Pas de nom");
                    erreurs++;
                }
                
                if (env.PositionsDisponibles == null || env.PositionsDisponibles.Count == 0)
                {
                    Debug.LogWarning($"⚠ {env.IdEnvironnement} : Aucune position disponible");
                }
                
                if (env.NiveauxDifficulte == null || env.NiveauxDifficulte.Count == 0)
                {
                    Debug.LogWarning($"⚠ {env.IdEnvironnement} : Aucun niveau de difficulté");
                }
                
                if (env.DureeDefaut <= 0)
                {
                    Debug.LogWarning($"⚠ {env.IdEnvironnement} : Durée par défaut invalide ({env.DureeDefaut}s)");
                }
            }
            
            if (erreurs == 0)
            {
                Debug.Log($"✓ Tous les environnements sont valides");
            }
            else
            {
                Debug.LogWarning($"⚠ {erreurs} erreur(s) trouvée(s)");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Erreur : {ex.Message}");
        }
    }

    [ContextMenu("Test 5: Créer une configuration")]
    public void Test_5_CreateConfiguration()
    {
        Debug.Log("\n--- Test 5: Création d'une configuration ---");
        
        try
        {
            var forest = HospitalDataService.Instance.Environnements.GetById("env-forest");
            
            if (forest != null)
            {
                var config = new ConfigurationEnvironnement
                {
                    IdEnvironnement = forest.IdEnvironnement,
                    PositionDepart = forest.PositionsDisponibles[0],
                    NiveauDifficulte = forest.NiveauxDifficulte[1], // moyen
                    NiveauAssistance = 5,
                    Duree = forest.DureeDefaut
                };
                
                Debug.Log("✓ Configuration créée:");
                Debug.Log($"  Environnement: {config.IdEnvironnement}");
                Debug.Log($"  Position: {config.PositionDepart}");
                Debug.Log($"  Difficulté: {config.NiveauDifficulte}");
                Debug.Log($"  Assistance: {config.NiveauAssistance}");
                Debug.Log($"  Durée: {config.Duree}s");
            }
            else
            {
                Debug.LogWarning("⚠ Impossible de créer la configuration (environnement non trouvé)");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Erreur : {ex.Message}");
        }
    }

    [ContextMenu("Test 6: Ajouter un environnement")]
    public void Test_6_AddEnvironnement()
    {
        Debug.Log("\n--- Test 6: Ajout d'un environnement ---");
        
        try
        {
            var newEnv = new EnvironnementJson
            {
                IdEnvironnement = "env-custom-test",
                NomEnvironnement = "Environnement de Test",
                Description = "Créé par le test automatique",
                PositionsDisponibles = new List<string> { "assis", "debout" },
                NiveauxDifficulte = new List<string> { "facile" },
                DureeDefaut = 120,
                ImagePath = "Test/test.png"
            };
            
            var added = HospitalDataService.Instance.Environnements.Add(newEnv);
            Debug.Log($"✓ Environnement ajouté : {added.NomEnvironnement} ({added.IdEnvironnement})");
            
            // Vérifier qu'il a bien été ajouté
            var retrieved = HospitalDataService.Instance.Environnements.GetById(added.IdEnvironnement);
            if (retrieved != null)
            {
                Debug.Log("✓ Environnement vérifié dans la liste");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Erreur : {ex.Message}");
        }
    }

    [ContextMenu("Test 7: Simuler une sélection utilisateur")]
    public void Test_7_SimulateUserSelection()
    {
        Debug.Log("\n--- Test 7: Simulation sélection utilisateur ---");
        
        try
        {
            // 1. L'utilisateur charge les environnements
            var environnements = HospitalDataService.Instance.Environnements.GetAll();
            Debug.Log($"1. Chargement: {environnements.Count} environnements disponibles");
            
            // 2. L'utilisateur sélectionne le 2ème environnement (index 1)
            if (environnements.Count > 1)
            {
                var selected = environnements[1];
                Debug.Log($"2. Sélection: {selected.NomEnvironnement}");
                
                // 3. L'interface charge les positions disponibles
                Debug.Log($"3. Positions disponibles: {string.Join(", ", selected.PositionsDisponibles)}");
                
                // 4. L'interface charge les difficultés disponibles
                Debug.Log($"4. Difficultés disponibles: {string.Join(", ", selected.NiveauxDifficulte)}");
                
                // 5. L'utilisateur configure et démarre
                var config = new ConfigurationEnvironnement
                {
                    IdEnvironnement = selected.IdEnvironnement,
                    PositionDepart = selected.PositionsDisponibles[0],
                    NiveauDifficulte = selected.NiveauxDifficulte[0],
                    NiveauAssistance = 3,
                    Duree = selected.DureeDefaut
                };
                
                Debug.Log("5. Configuration finale:");
                Debug.Log($"   - Environnement: {selected.NomEnvironnement}");
                Debug.Log($"   - Position: {config.PositionDepart}");
                Debug.Log($"   - Difficulté: {config.NiveauDifficulte}");
                Debug.Log($"   - Assistance: {config.NiveauAssistance}");
                Debug.Log($"   - Durée: {config.Duree}s");
                Debug.Log("✓ Simulation réussie!");
            }
            else
            {
                Debug.LogWarning("⚠ Pas assez d'environnements pour la simulation");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"✗ Erreur : {ex.Message}");
        }
    }

    [ContextMenu("Afficher statistiques")]
    public void ShowStatistics()
    {
        Debug.Log("\n=== STATISTIQUES ENVIRONNEMENTS ===");
        
        try
        {
            var environnements = HospitalDataService.Instance.Environnements.GetAll();
            
            Debug.Log($"Total environnements: {environnements.Count}");
            
            // Compter les positions uniques
            var positionsUniques = new HashSet<string>();
            var difficultesUniques = new HashSet<string>();
            int dureeMin = int.MaxValue;
            int dureeMax = int.MinValue;
            
            foreach (var env in environnements)
            {
                foreach (var pos in env.PositionsDisponibles)
                    positionsUniques.Add(pos);
                    
                foreach (var diff in env.NiveauxDifficulte)
                    difficultesUniques.Add(diff);
                    
                if (env.DureeDefaut < dureeMin) dureeMin = env.DureeDefaut;
                if (env.DureeDefaut > dureeMax) dureeMax = env.DureeDefaut;
            }
            
            Debug.Log($"Positions disponibles: {string.Join(", ", positionsUniques)}");
            Debug.Log($"Difficultés disponibles: {string.Join(", ", difficultesUniques)}");
            Debug.Log($"Durée min: {dureeMin}s, max: {dureeMax}s");
            
            Debug.Log("===================================");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erreur : {ex.Message}");
        }
    }
}
