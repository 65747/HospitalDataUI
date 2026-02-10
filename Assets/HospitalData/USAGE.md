# Guide : utiliser les manageurs Hospital Data

Toutes les données (patients, superviseurs, sessions) sont stockées en JSON dans **Assets/HospitalData/StreamingAssets/** et ne sont accessibles **que via les manageurs**. Ne pas lire ni écrire les JSON directement — uniquement via l’API ci-dessous.

---

## 1. Accès dans le code

En tête de fichier :

```csharp
using Hospital.Data.Storage;
using Hospital.Data.Models;
```

Point d’entrée unique : **HospitalDataService.Instance**. On en déduit les trois manageurs :

```csharp
var data = HospitalDataService.Instance;

// Manageurs (initialisation paresseuse, une fois pour toute l’appli)
var patients    = data.Patients;     // les_patients.json
var superviseurs = data.Superviseurs; // les_superviseur.json
var sessions    = data.Sessions;     // sessions.json (vérif des ID patient/superviseur)
```

---

## 2. Patients (PatientsManager)

**Fichier :** `les_patients.json`

| Méthode | Description |
|--------|-------------|
| `GetAll()` | Liste de tous les patients |
| `Add(PatientJson patient)` | Ajouter un patient. Si `IDpatient` est vide, il est généré. Écriture immédiate dans le JSON. |
| `Update(PatientJson patient)` | Remplacer l’entrée par ID. Retourne `true` si trouvée. |
| `UpdateSuivi(string idPatient, string suiviPatient)` | Mettre à jour uniquement le champ SuiviPatient. |
| `Remove(string idPatient)` | Supprimer un patient par ID ; **toutes ses sessions sont supprimées automatiquement**. Retourne `true` si supprimé. |

Exemple : lire tous les patients et en ajouter un

```csharp
var data = HospitalDataService.Instance;

// Liste complète
foreach (var p in data.Patients.GetAll())
    Debug.Log($"{p.IDpatient}: {p.Prenom} {p.Nom}");

// Ajout (ID peut rester vide — il sera attribué)
var nouveau = new PatientJson
{
    IDpatient = "",  // optionnel
    Nom = "Dupont",
    Prenom = "Marie",
    date_de_naissance = 1985,
    Sexe = "F",
    Pathologie = "Héminégligence",
    CoteNeglige = "gauche",
    SuiviPatient = ""
};
var added = data.Patients.Add(nouveau);
Debug.Log($"Patient ajouté avec l’ID : {added.IDpatient}");
```

---

## 3. Superviseurs (SuperviseursManager)

**Fichier :** `les_superviseur.json`

| Méthode | Description |
|--------|-------------|
| `GetAll()` | Tous les superviseurs |
| `Add(SuperviseurJson superviseur)` | Ajouter. `IdSuperviseur` vide → génération auto. Sauvegarde dans le même JSON. |
| `Update(SuperviseurJson superviseur)` | Mettre à jour par ID |
| `Remove(string idSuperviseur)` | Supprimer par ID |

Exemple

```csharp
var data = HospitalDataService.Instance;

foreach (var s in data.Superviseurs.GetAll())
    Debug.Log($"{s.IdSuperviseur}: {s.Prenom} {s.Nom} ({s.fonction})");

var newSup = new SuperviseurJson
{
    IdSuperviseur = "",
    Nom = "Martin",
    Prenom = "Luc",
    fonction = "Kinésithérapeute"
};
data.Superviseurs.Add(newSup);
```

---

## 4. Sessions (SessionsManager)

**Fichier :** `sessions.json`

Via **HospitalDataService**, les sessions sont liées aux **Patients** et **Superviseurs** : à chaque `Add` et `Update`, on vérifie que `IDpatient` et, si renseigné, `IdSuperviseur` existent dans les manageurs correspondants.

| Méthode | Description |
|--------|-------------|
| `GetAll()` | Toutes les sessions |
| `GetByPatient(string idPatient)` | Sessions d’un patient par ID |
| `Add(SessionJson session)` | Ajouter une session. Vérification des ID patient et superviseur. Écriture dans le même JSON. |
| `Update(SessionJson session)` | Mettre à jour par (IdPatient + DateDebut) |
| `Remove(string idPatient, DateTime dateDebut)` | Supprimer une session par patient et date de début |
| `RemoveAllByPatient(string idPatient)` | Supprimer toutes les sessions d’un patient (retourne le nombre supprimé). Appelé automatiquement lors de `Patients.Remove(idPatient)`. |

Exemple

```csharp
var data = HospitalDataService.Instance;

// Toutes les sessions d’un patient
var list = data.Sessions.GetByPatient("patient-001");

// Ajouter une session (le patient et le superviseur doivent déjà exister dans leurs JSON)
var session = new SessionJson
{
    IDpatient = "patient-001",
    IdSuperviseur = "sup-abc",
    EnvironnementUtilise = "VR",
    PositionDepart = "assis",
    DateDebut = DateTime.UtcNow,
    niveauDifficulte = "moyen",
    NiveauAssistance_moyen = 1,
    duree = 300,
    ScoreTotal = 85,
    Commentaire = "Bonne séance"
};
data.Sessions.Add(session);
```

---

## 5. À retenir

- **Une seule source de vérité :** toute modification passe par les manageurs — ils lisent et écrivent eux-mêmes dans les mêmes fichiers JSON.
- **Accès concurrent :** les manageurs utilisent un verrou en interne ; on peut les appeler depuis plusieurs threads.
- **Chemin des données :** par défaut `Assets/HospitalData/StreamingAssets/`. Il est défini dans `HospitalDataService` (on peut le sortir en paramètre si besoin).
- **Modèles :** les types `PatientJson`, `SuperviseurJson`, `SessionJson` sont dans `Hospital.Data.Models` (fichier `Models/JsonRecords.cs`). Les noms des propriétés correspondent aux clés JSON (ex. `IDpatient`, `date_de_naissance`, `duree`).

Pour plus d’exemples : **HospitalDataTestRunner.cs** (lecture et tests Add).
