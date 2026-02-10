# Partie 3 — Exemples d’utilisation des manageurs

Cette partie donne des **exemples concrets** d’utilisation de l’API (HospitalDataService et manageurs). Chaque exemple indique un **but** précis et peut être réutilisé dans l’application (Unity UI, scripts de test, outils d’import, etc.).

---

## 3.1 Accès unique et premier contact

**But :** Obtenir une référence unique aux données pour tout le projet, sans recréer les manageurs à chaque fois.

```csharp
using Hospital.Data.Storage;
using Hospital.Data.Models;

// Une seule instance pour toute l’application (singleton).
var data = HospitalDataService.Instance;

// Les trois manageurs sont créés à la demande (lazy) et pointent vers les vrais fichiers JSON.
var patients    = data.Patients;     // → les_patients.json
var superviseurs = data.Superviseurs; // → les_superviseur.json
var sessions    = data.Sessions;     // → sessions.json
```

**Pourquoi :** Éviter d’ouvrir plusieurs fois les mêmes fichiers et garder un point d’entrée clair pour les autres développeurs.

---

## 3.2 Afficher la liste complète des patients (ex. écran liste)

**But :** Remplir une liste ou un tableau à l’écran avec tous les patients (nom, prénom, pathologie, etc.).

```csharp
var data = HospitalDataService.Instance;
IReadOnlyList<PatientJson> tous = data.Patients.GetAll();

foreach (var p in tous)
{
    // Exemple : alimenter une ligne de tableau ou une entrée de liste déroulante.
    string ligne = $"{p.Prenom} {p.Nom} — {p.Pathologie} ({p.CoteNeglige})";
    Debug.Log(ligne);
    // Ou : maListeUI.AddItem(ligne, p.IDpatient);
}
```

**Remarque :** `GetAll()` retourne une **copie** de la liste ; modifier les éléments ne modifie pas le fichier tant qu’on n’appelle pas `Update` ou `Add`.

---

## 3.3 Créer un nouveau patient (formulaire d’inscription)

**But :** Enregistrer un patient saisi dans un formulaire. L’ID peut rester vide : le manageur en génère un automatiquement.

```csharp
var data = HospitalDataService.Instance;

var nouveau = new PatientJson
{
    IDpatient = "",  // Laissé vide → génération auto du type "patient-xxxxxxxx"
    Nom = "Dupont",
    Prenom = "Marie",
    date_de_naissance = 1985,
    Sexe = "F",
    Pathologie = "Héminégligence",
    CoteNeglige = "gauche",
    SuiviPatient = "Premier contact — à revoir dans 2 semaines"
};

PatientJson enregistre = data.Patients.Add(nouveau);
// enregistre.IDpatient contient maintenant l’ID attribué (ex. "patient-a1b2c3d4...")
// Le fichier les_patients.json est mis à jour immédiatement.
```

**Pourquoi :** Centraliser la création et la persistance dans le manageur ; l’UI n’a pas à gérer le chemin du fichier ni le format JSON.

---

## 3.4 Mettre à jour uniquement le suivi d’un patient

**But :** Modifier seulement le champ « Suivi patient » (notes de suivi) sans toucher aux autres champs (nom, pathologie, etc.).

```csharp
var data = HospitalDataService.Instance;
string idPatient = "patient-001";
string nouveauSuivi = "Séance du 05/02 — bons progrès sur la partie gauche.";

bool ok = data.Patients.UpdateSuivi(idPatient, nouveauSuivi);
if (ok)
    Debug.Log("Suivi mis à jour.");
else
    Debug.LogWarning("Patient introuvable pour cet ID.");
```

**Pourquoi :** Éviter de charger tout le patient, modifier un champ et rappeler `Update` ; une seule méthode dédiée pour le suivi.

---

## 3.5 Remplacer entièrement un patient (édition complète)

**But :** Sauvegarder les modifications d’un formulaire d’édition qui modifie plusieurs champs (nom, pathologie, etc.). L’entrée est identifiée par son `IDpatient`.

```csharp
var data = HospitalDataService.Instance;

// On part d’un patient déjà chargé (ex. sélectionné dans la liste).
PatientJson patientModifie = new PatientJson
{
    IDpatient = "patient-001",  // Doit correspondre à une entrée existante.
    Nom = "Dupont",
    Prenom = "Marie",
    date_de_naissance = 1985,
    Sexe = "F",
    Pathologie = "Négligence spatiale",  // Champ modifié.
    CoteNeglige = "gauche",
    DateCreation = patientExistant.DateCreation,  // À conserver si déjà chargé.
    SuiviPatient = "Dernière séance : amélioration."
};

bool ok = data.Patients.Update(patientModifie);
if (!ok)
    Debug.LogWarning("Aucun patient avec cet ID — peut-être supprimé entre-temps.");
```

**Pourquoi :** Garantir que toute l’entrée en base est cohérente avec ce que l’utilisateur a validé à l’écran.

---

## 3.6 Supprimer un patient (et toutes ses sessions)

**But :** Retirer définitivement un patient des données (ex. demande de suppression confirmée par l’utilisateur). **Toutes les sessions de ce patient sont supprimées automatiquement** (par son ID) dans `sessions.json` — pas besoin de les supprimer à la main.

```csharp
var data = HospitalDataService.Instance;
string idASupprimer = "patient-001";

bool supprime = data.Patients.Remove(idASupprimer);
if (supprime)
    Debug.Log("Patient et toutes ses sessions retirés.");
else
    Debug.Log("Aucun patient avec cet ID.");
```

**Comportement :** Lors d’un `Patients.Remove(idPatient)`, le service déclenche en interne la suppression de toutes les sessions dont `IDpatient` correspond, puis met à jour `sessions.json`. Un seul appel suffit.

---

## 3.7 Lister tous les superviseurs (ex. menu déroulant)

**But :** Alimenter une liste déroulante ou une grille pour choisir un superviseur (ex. avant de créer une session).

```csharp
var data = HospitalDataService.Instance;

foreach (var s in data.Superviseurs.GetAll())
{
    string libelle = $"{s.Prenom} {s.Nom} — {s.fonction}";
    // Ajouter à un Dropdown / liste : (libelle, s.IdSuperviseur)
}
```

**Pourquoi :** Une seule source de vérité (les_superviseur.json) pour tous les écrans qui ont besoin de la liste des superviseurs.

---

## 3.8 Ajouter un nouveau superviseur

**But :** Enregistrer un nouveau membre de l’équipe (superviseur). L’ID peut être laissé vide pour génération automatique.

```csharp
var data = HospitalDataService.Instance;

var nouveau = new SuperviseurJson
{
    IdSuperviseur = "",
    Nom = "Martin",
    Prenom = "Luc",
    fonction = "Kinésithérapeute"
};

SuperviseurJson ajoute = data.Superviseurs.Add(nouveau);
// ajoute.IdSuperviseur contient l’ID généré (ex. "sup-xxxxxxxx").
```

**Pourquoi :** Même logique que pour les patients : création et persistance centralisées.

---

## 3.9 Afficher toutes les sessions (tableau de bord)

**But :** Afficher la liste de toutes les sessions (ex. tableau de bord admin) avec patient, durée, score, commentaire.

```csharp
var data = HospitalDataService.Instance;

foreach (var s in data.Sessions.GetAll())
{
    string resume = $"{s.IDpatient} | {s.duree}s | Score {s.ScoreTotal} | {s.Commentaire}";
    Debug.Log(resume);
}
```

**Pourquoi :** Vue globale sur l’activité sans filtrer par patient.

---

## 3.10 Sessions d’un patient donné (historique)

**But :** Afficher uniquement les sessions d’un patient sélectionné (historique des séances, graphiques, etc.).

```csharp
var data = HospitalDataService.Instance;
string idPatient = "patient-001";

IReadOnlyList<SessionJson> sessionsDuPatient = data.Sessions.GetByPatient(idPatient);

foreach (var s in sessionsDuPatient)
{
    Debug.Log($"{s.DateDebut:g} — {s.EnvironnementUtilise}, {s.duree}s, score {s.ScoreTotal}");
}
```

**Pourquoi :** Éviter de parcourir toutes les sessions côté appelant ; le manageur filtre par `IDpatient` (insensible à la casse).

---

## 3.11 Créer une nouvelle session (enregistrement d’une séance)

**But :** Enregistrer une séance terminée : patient, superviseur, durée, score, commentaire. Les ID patient et superviseur sont validés par le manageur s’il a été construit avec `Patients` et `Superviseurs` (cas de `HospitalDataService`).

```csharp
var data = HospitalDataService.Instance;

var session = new SessionJson
{
    IDpatient = "patient-001",
    IdSuperviseur = "sup-abc",
    EnvironnementUtilise = "VR",
    PositionDepart = "assis",
    DateDebut = DateTime.UtcNow,
    niveauDifficulte = "moyen",
    NiveauAssistance_moyen = 1,
    ObjectifsAtteints = "oui",
    ObjectifsManques = "non",
    duree = 300,
    ScoreTotal = 85,
    TempsReaction = 0.5,
    PrecisionPointage = 0.9,
    Commentaire = "Bonne séance — patient plus à l’aise sur la gauche."
};

data.Sessions.Add(session);
// Si patient ou superviseur inconnu, une InvalidOperationException est levée.
// Sinon, la session est ajoutée et sessions.json est mis à jour.
```

**Pourquoi :** Garantir l’intégrité des références (pas de session vers un patient ou superviseur inexistant) et persister en une seule opération.

---

## 3.12 Modifier une session existante

**But :** Corriger ou compléter une session déjà enregistrée (ex. ajouter un commentaire a posteriori). La session est identifiée par le couple (IDpatient, DateDebut).

```csharp
var data = HospitalDataService.Instance;

// On part d’une session déjà chargée (ex. via GetByPatient puis sélection).
SessionJson sessionModifiee = new SessionJson
{
    IDpatient = sessionExistante.IDpatient,
    DateDebut = sessionExistante.DateDebut,  // Clé : ne pas changer.
    IdSuperviseur = sessionExistante.IdSuperviseur,
    EnvironnementUtilise = sessionExistante.EnvironnementUtilise,
    PositionDepart = sessionExistante.PositionDepart,
    niveauDifficulte = sessionExistante.niveauDifficulte,
    NiveauAssistance_moyen = sessionExistante.NiveauAssistance_moyen,
    ObjectifsAtteints = sessionExistante.ObjectifsAtteints,
    ObjectifsManques = sessionExistante.ObjectifsManques,
    duree = sessionExistante.duree,
    ScoreTotal = sessionExistante.ScoreTotal,
    TempsReaction = sessionExistante.TempsReaction,
    PrecisionPointage = sessionExistante.PrecisionPointage,
    Commentaire = "Commentaire mis à jour après relecture."
};

bool ok = data.Sessions.Update(sessionModifiee);
```

**Pourquoi :** Mise à jour ciblée sans recréer toute la liste des sessions ; le manageur retrouve l’entrée par (IDpatient, DateDebut).

---

## 3.13 Supprimer une session

**But :** Retirer une session erronée ou annulée. Identifiée par l’ID du patient et la date de début.

```csharp
var data = HospitalDataService.Instance;
string idPatient = "patient-001";
DateTime dateDebut = new DateTime(2026, 2, 1, 14, 0, 0, DateTimeKind.Utc);

bool supprime = data.Sessions.Remove(idPatient, dateDebut);
```

**Pourquoi :** Nettoyage précis sans toucher aux autres sessions du même patient.

**Note :** Pour supprimer *toutes* les sessions d’un patient sans supprimer le patient : `data.Sessions.RemoveAllByPatient(idPatient)` (retourne le nombre de sessions supprimées). Lorsqu’on supprime un patient avec `Patients.Remove(idPatient)`, cette suppression est faite automatiquement en interne.

---

## 3.14 Filtrer les patients côté appelant (ex. par première lettre)

**But :** Réduire la liste affichée sans ajouter de méthode dans le manageur (ex. filtre par première lettre du nom ou du prénom).

```csharp
var data = HospitalDataService.Instance;
string premiereLettre = "D";  // Saisie utilisateur ou touche clavier.

var tous = data.Patients.GetAll();
var filtres = tous.Where(p =>
    (p.Nom?.StartsWith(premiereLettre, StringComparison.OrdinalIgnoreCase) ?? false) ||
    (p.Prenom?.StartsWith(premiereLettre, StringComparison.OrdinalIgnoreCase) ?? false)
).ToList();

// Afficher filtres dans la liste / dropdown.
```

**Pourquoi :** Garder le manageur simple (GetAll) et laisser la logique de présentation (filtres, tri) dans la couche UI ou service métier.

---

## 3.15 Vérifier le chemin des données (débogage)

**But :** Savoir où sont lus/écrits les fichiers JSON (utile en débogage ou pour des logs).

```csharp
var data = HospitalDataService.Instance;
Debug.Log($"Dossier des données : {data.BasePath}");
// Exemple de sortie : .../Assets/HospitalData/StreamingAssets
```

**Pourquoi :** Éviter les confusions (fichiers de test vs production) et faciliter le support.

---

## 3.16 Scénario complet : de l’affichage à la création d’une session

**But :** Montrer un enchaînement réaliste : charger les patients, en choisir un, charger les superviseurs, en choisir un, puis créer une session pour ce patient avec ce superviseur.

```csharp
var data = HospitalDataService.Instance;

// 1) Liste des patients pour un menu.
var patients = data.Patients.GetAll();
// L’utilisateur en choisit un → idPatientChoisi

// 2) Liste des superviseurs pour un second menu.
var superviseurs = data.Superviseurs.GetAll();
// L’utilisateur en choisit un → idSuperviseurChoisi

// 3) Création de la session avec les deux ID.
var session = new SessionJson
{
    IDpatient = idPatientChoisi,
    IdSuperviseur = idSuperviseurChoisi,
    EnvironnementUtilise = "VR",
    PositionDepart = "assis",
    DateDebut = DateTime.UtcNow,
    niveauDifficulte = "moyen",
    NiveauAssistance_moyen = 0,
    ObjectifsAtteints = "",
    ObjectifsManques = "",
    duree = 0,
    ScoreTotal = 0,
    Commentaire = ""
};

try
{
    data.Sessions.Add(session);
    Debug.Log("Session enregistrée.");
}
catch (InvalidOperationException ex)
{
    Debug.LogWarning($"Référence invalide : {ex.Message}");
}
```

**Pourquoi :** Illustrer comment tout le flux (lecture listes → choix → écriture) repose sur le même point d’entrée et les mêmes manageurs.

---

*Partie 3 — Exemples d’utilisation. Chaque exemple a un but précis (affichage, création, mise à jour, suppression, filtrage, débogage) et peut être copié ou adapté dans le projet.*
