# Structures de données & Managers

Pour utiliser cette partie dans Unity, il faut ajouter **Newtonsoft.Json** aux dépendances.

---

## Partie 1 — Données : stockage et structures

### 1.1 Comment les données sont stockées

Le stockage physique est constitué de **trois fichiers texte au format JSON**, placés dans un dossier dédié (ex. `Assets/HospitalData/StreamingAssets/` dans le projet Unity).

| Fichier | Contenu |
|---------|---------|
| `les_patients.json` | Liste de tous les patients |
| `les_superviseur.json` | Liste de tous les superviseurs |
| `sessions.json` | Liste des sessions (format enveloppé, voir ci-dessous) |

- **Sérialisation / désérialisation** : Newtonsoft.Json (Json.NET).
- **Encodage** : UTF-8.
- **Format d'écriture** : JSON indenté (`Formatting.Indented`) pour lisibilité et suivi en dépôt.

### 1.2 Structures de données pour le stockage physique

Les noms des propriétés en C# sont **alignés sur les clés JSON**. Voici les structures utilisées côté fichier (et reflétées en mémoire).

#### Fichier `les_patients.json`

```json
[
  {
    "IDpatient": "patient-001",
    "Nom": "Dupont",
    "Prenom": "Jean",
    "date_de_naissance": 1980,
    "Sexe": "M",
    "Pathologie": "Héminégligence",
    "CoteNeglige": "gauche",
    "DateCreation": "2026-01-30T10:00:00Z",
    "SuiviPatient": "Recommander exercices..."
  }
]
```

#### Fichier `les_superviseur.json`

```json
[
  {
    "IdSuperviseur": "sup-001",
    "Nom": "Martin",
    "Prenom": "Luc",
    "fonction": "Kinésithérapeute"
  }
]
```

#### Fichier `sessions.json`

```json
[
  {
    "Sessions": [
      {
        "IDpatient": "patient-001",
        "EnvironnementUtilise": "VR",
        "PositionDepart": "assis",
        "DateDebut": "2026-02-01T14:00:00Z",
        "niveauDifficulte": "moyen",
        "NiveauAssistance_moyen": 1,
        "ObjectifsAtteints": "oui",
        "ObjectifsManques": "non",
        "duree": 300,
        "ScoreTotal": 85,
        "IdSuperviseur": "sup-001",
        "TempsReaction": 0.5,
        "PrecisionPointage": 0.9,
        "Commentaire": "Bonne séance"
      }
    ]
  }
]
```

### 1.3 Chargement en mémoire — structures utilisées

- **Chargement** : à la demande au premier accès en lecture ou écriture. Les fichiers sont lus une fois puis gardés en mémoire.
- **Structures en mémoire** :
  - **Patients** : `List<PatientJson>` (copie privée dans le manageur).
  - **Superviseurs** : `List<SuperviseurJson>`.
  - **Sessions** : `List<SessionJson>` (désérialisation depuis l'enveloppe `SessionEnvelope`).
- **Synchronisation** : `ReaderWriterLockSlim` pour accès concurrent (plusieurs lectures possibles, écriture exclusive).
- **Persistance** : après chaque `Add`, `Update`, `Remove` (et `UpdateSuivi` pour les patients), la liste concernée est réécrite dans le fichier JSON correspondant.

---

## Partie 2 — Manager proposé

### 2.1 Données accessibles

Via un point d'entrée unique **`HospitalDataService.Instance`**, les données accessibles sont :

- **Patients** : liste des patients (lecture, création, mise à jour, suppression).
- **Superviseurs** : liste des superviseurs (lecture, création, mise à jour, suppression).
- **Sessions** : liste des sessions, avec filtrage par patient (lecture, création, mise à jour, suppression). Lors de l'ajout ou de la mise à jour d'une session, le manageur peut vérifier que l'ID patient et l'ID superviseur existent dans les manageurs respectifs.

Aucun accès direct aux fichiers JSON n'est exposé : tout passe par les manageurs.

### 2.2 Méthodes pour lire les données

| Manageur | Méthode | Description |
|----------|---------|-------------|
| PatientsManager | `GetAll()` | Retourne une copie en lecture seule de tous les patients. |
| SuperviseursManager | `GetAll()` | Retourne une copie en lecture seule de tous les superviseurs. |
| SessionsManager | `GetAll()` | Retourne une copie en lecture seule de toutes les sessions. |
| SessionsManager | `GetByPatient(string idPatient)` | Retourne les sessions dont le patient a l'ID donné (insensible à la casse). |

### 2.3 Méthodes pour créer ou mettre à jour les données

| Manageur | Méthode | Description |
|----------|---------|-------------|
| PatientsManager | `Add(PatientJson patient)` | Ajoute un patient ; génère un ID si `IDpatient` est vide ; persiste dans `les_patients.json`. |
| PatientsManager | `Update(PatientJson patient)` | Remplace l'entrée dont l'ID correspond ; retourne `true` si trouvée. |
| PatientsManager | `UpdateSuivi(string idPatient, string suiviPatient)` | Met à jour uniquement le champ `SuiviPatient` du patient donné. |
| PatientsManager | `Remove(string idPatient)` | Supprime le patient ayant cet ID ; retourne `true` si supprimé. |
| SuperviseursManager | `Add(SuperviseurJson superviseur)` | Ajoute un superviseur ; génère un ID si `IdSuperviseur` est vide ; persiste. |
| SuperviseursManager | `Update(SuperviseurJson superviseur)` | Remplace l'entrée dont l'ID correspond. |
| SuperviseursManager | `Remove(string idSuperviseur)` | Supprime le superviseur ayant cet ID. |
| SessionsManager | `Add(SessionJson session)` | Ajoute une session ; peut valider les références patient/superviseur ; persiste. |
| SessionsManager | `Update(SessionJson session)` | Remplace la session identifiée par (IDpatient, DateDebut) ; peut valider les références. |
| SessionsManager | `Remove(string idPatient, DateTime dateDebut)` | Supprime la session correspondante. |

### 2.4 C# — Signatures des méthodes et déclarations des variables

#### Modèles (`Hospital.Data.Models`)

```csharp
namespace Hospital.Data.Models
{
    public class PatientJson
    {
        public string IDpatient { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public int date_de_naissance { get; set; }
        public string Sexe { get; set; }
        public string Pathologie { get; set; }
        public string CoteNeglige { get; set; }
        public DateTime DateCreation { get; set; }
        public string SuiviPatient { get; set; }
    }

    public class SuperviseurJson
    {
        public string IdSuperviseur { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string fonction { get; set; }
    }

    public class SessionJson
    {
        public string IDpatient { get; set; }
        public string EnvironnementUtilise { get; set; }
        public string PositionDepart { get; set; }
        public DateTime DateDebut { get; set; }
        public string niveauDifficulte { get; set; }
        public int NiveauAssistance_moyen { get; set; }
        public string ObjectifsAtteints { get; set; }
        public string ObjectifsManques { get; set; }
        public int duree { get; set; }
        public int ScoreTotal { get; set; }
        public string IdSuperviseur { get; set; }
        public double TempsReaction { get; set; }
        public double PrecisionPointage { get; set; }
        public string Commentaire { get; set; }
    }

    public class SessionEnvelope
    {
        public List<SessionJson> Sessions { get; set; }
    }
}
```

#### PatientsManager (`Hospital.Data.Storage`)

```csharp
namespace Hospital.Data.Storage
{
    public class PatientsManager
    {
        // Variables d'instance (stockage et synchronisation)
        private readonly string _path;
        private readonly ReaderWriterLockSlim _lock;
        private static readonly JsonSerializerSettings ReadSettings;
        private static readonly JsonSerializerSettings WriteSettings;
        private bool _loaded;
        private List<PatientJson> _patients;

        public PatientsManager(string path = "Data/les_patients.json");

        // Lecture
        public IReadOnlyList<PatientJson> GetAll();

        // Création / mise à jour / suppression
        public PatientJson Add(PatientJson patient);
        public bool Update(PatientJson patient);
        public bool UpdateSuivi(string idPatient, string suiviPatient);
        public bool Remove(string idPatient);
    }
}
```

#### SuperviseursManager (`Hospital.Data.Storage`)

```csharp
namespace Hospital.Data.Storage
{
    public class SuperviseursManager
    {
        private readonly string _path;
        private readonly ReaderWriterLockSlim _lock;
        private static readonly JsonSerializerSettings ReadSettings;
        private static readonly JsonSerializerSettings WriteSettings;
        private bool _loaded;
        private List<SuperviseurJson> _superviseurs;

        public SuperviseursManager(string path = "Data/les_superviseur.json");

        public IReadOnlyList<SuperviseurJson> GetAll();
        public SuperviseurJson Add(SuperviseurJson superviseur);
        public bool Update(SuperviseurJson superviseur);
        public bool Remove(string idSuperviseur);
    }
}
```

#### SessionsManager (`Hospital.Data.Storage`)

```csharp
namespace Hospital.Data.Storage
{
    public class SessionsManager
    {
        private readonly string _path;
        private readonly ReaderWriterLockSlim _lock;
        private static readonly JsonSerializerSettings ReadSettings;
        private static readonly JsonSerializerSettings WriteSettings;
        private bool _loaded;
        private List<SessionJson> _sessions;

        public SessionsManager(string path = "Data/sessions.json");

        public IReadOnlyList<SessionJson> GetAll();
        public IReadOnlyList<SessionJson> GetByPatient(string idPatient);
        public SessionJson Add(SessionJson session);
        public bool Update(SessionJson session);
        public bool Remove(string idPatient, DateTime dateDebut);
    }
}
```

---

## Partie 3 — Exemples d'utilisation

### 3.1 Accéder au service

```csharp
var data = HospitalDataService.Instance;
```

### 3.2 Lister tous les patients (ex. pour un menu)

```csharp
var data = HospitalDataService.Instance;
foreach (var p in data.Patients.GetAll())
{
    string libelle = $"{p.Prenom} {p.Nom} — {p.Pathologie}";
    // Afficher dans une liste / dropdown : (libelle, p.IDpatient)
}
```

### 3.3 Ajouter un nouveau patient

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
// enregistre.IDpatient contient maintenant l'ID attribué (ex. "patient-a1b2c3d4...")
// Le fichier les_patients.json est mis à jour immédiatement.
```

*L'ID peut rester vide : le manageur en génère un automatiquement.*

### 3.4 Mettre à jour uniquement le suivi d'un patient

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

### 3.5 Remplacer entièrement un patient (édition complète)

```csharp
var data = HospitalDataService.Instance;

// On part d'un patient déjà chargé (ex. sélectionné dans la liste).
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

### 3.6 Supprimer un patient

```csharp
var data = HospitalDataService.Instance;
string idASupprimer = "patient-001";

bool supprime = data.Patients.Remove(idASupprimer);
if (supprime)
    Debug.Log("Patient retiré du fichier.");
else
    Debug.Log("Aucun patient avec cet ID.");
```

*Lors d'un `Patients.Remove(idASupprimer)`, le service déclenche en interne la suppression de toutes les sessions dont `IDpatient` correspond, puis met à jour `sessions.json`. Un seul appel suffit.*

### 3.7 Lister tous les superviseurs (ex. menu déroulant)

```csharp
var data = HospitalDataService.Instance;
foreach (var s in data.Superviseurs.GetAll())
{
    string libelle = $"{s.Prenom} {s.Nom} — {s.fonction}";
    // Ajouter à un Dropdown / liste : (libelle, s.IdSuperviseur)
}
```

### 3.8 Ajouter un nouveau superviseur

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
// ajoute.IdSuperviseur contient l'ID généré (ex. "sup-xxxxxxxx").
```

*L'ID peut être laissé vide pour génération automatique.*

### 3.9 Afficher toutes les sessions (tableau de bord)

```csharp
var data = HospitalDataService.Instance;
foreach (var s in data.Sessions.GetAll())
{
    string resume = $"{s.IDpatient} | {s.duree}s | Score {s.ScoreTotal} | {s.Commentaire}";
    Debug.Log(resume);
}
```

### 3.10 Sessions d'un patient donné (historique)

```csharp
var data = HospitalDataService.Instance;
string idPatient = "patient-001";

IReadOnlyList<SessionJson> sessionsDuPatient = data.Sessions.GetByPatient(idPatient);

foreach (var s in sessionsDuPatient)
{
    Debug.Log($"{s.DateDebut:g} — {s.EnvironnementUtilise}, {s.duree}s, score {s.ScoreTotal}");
}
```

### 3.11 Créer une nouvelle session (enregistrement d'une séance)

*Enregistrer une séance terminée : patient, superviseur, durée, score, commentaire. Les ID patient et superviseur sont validés par le manageur s'il a été construit avec Patients et Superviseurs (cas de HospitalDataService).*

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
    Commentaire = "Bonne séance — patient plus à l'aise sur la gauche."
};

data.Sessions.Add(session);
// Si patient ou superviseur inconnu, une InvalidOperationException est levée.
// Sinon, la session est ajoutée et sessions.json est mis à jour.
```

### 3.12 Modifier une session existante

```csharp
var data = HospitalDataService.Instance;

// On part d'une session déjà chargée (ex. via GetByPatient puis sélection).
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

*La session est identifiée par le couple (IDpatient, DateDebut).*

### 3.13 Supprimer une session

```csharp
var data = HospitalDataService.Instance;
string idPatient = "patient-001";
DateTime dateDebut = new DateTime(2026, 2, 1, 14, 0, 0, DateTimeKind.Utc);

bool supprime = data.Sessions.Remove(idPatient, dateDebut);
```

### 3.14 Filtrer les patients côté appelant (ex. par première lettre)

```csharp
using System.Linq;

var data = HospitalDataService.Instance;
string premiereLettre = "D";  // Saisie utilisateur ou touche clavier.

var tous = data.Patients.GetAll();
var filtres = tous.Where(p =>
    (p.Nom?.StartsWith(premiereLettre, StringComparison.OrdinalIgnoreCase) ?? false) ||
    (p.Prenom?.StartsWith(premiereLettre, StringComparison.OrdinalIgnoreCase) ?? false)
).ToList();

// Afficher filtres dans la liste / dropdown.
```

### 3.15 Vérifier le chemin des données (débogage)

```csharp
var data = HospitalDataService.Instance;
Debug.Log($"Dossier des données : {data.BasePath}");
// Exemple de sortie : .../Assets/HospitalData/StreamingAssets
```

### 3.16 Scénario complet : de l'affichage à la création d'une session

```csharp
var data = HospitalDataService.Instance;

// 1) Liste des patients pour un menu.
var patients = data.Patients.GetAll();
// L'utilisateur en choisit un → idPatientChoisi

// 2) Liste des superviseurs pour un second menu.
var superviseurs = data.Superviseurs.GetAll();
// L'utilisateur en choisit un → idSuperviseurChoisi

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
