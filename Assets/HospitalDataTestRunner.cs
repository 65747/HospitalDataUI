using UnityEngine;
using Hospital.Data.Storage;
using Hospital.Data.Models;

namespace Hospital.Data.Unity
{
    // Attacher ce script à un GameObject dans la scène (ex. objet vide "DatabaseTester").
    // Au Play, charge les données via les manageurs et affiche tout dans la console Unity.
    public class HospitalDataTestRunner : MonoBehaviour
    {
        [Tooltip("Sous-dossier dans Assets/HospitalData/. Vide = StreamingAssets. On peut indiquer Data.")]
        [SerializeField] string dataFolder = "";

        [Tooltip("Afficher le détail de chaque champ.")]
        [SerializeField] bool verbose = true;

        void Start()
        {
            RunAllTests();
        }

        [ContextMenu("Lancer les tests base de données")]
        public void RunAllTests()
        {
            var data = HospitalDataService.Instance;
            Debug.Log("[HospitalData] ========== TEST START ==========");
            Debug.Log($"[HospitalData] Base path: {data.BasePath}");

            TestPatients(data);
            TestSuperviseurs(data);
            TestSessions(data);
            TestAddPatients(data);
            TestAddSuperviseurs(data);
            TestAddSessions(data);

            Debug.Log("[HospitalData] ========== TEST END ==========");
        }

        void TestPatients(HospitalDataService data)
        {
            var manager = data.Patients;

            var all = manager.GetAll();
            Debug.Log($"[HospitalData] Patients: loaded {all.Count}");

            foreach (var p in all)
            {
                if (verbose)
                    Debug.Log($"[HospitalData]   Patient: Id={p.IDpatient}, Nom={p.Nom}, Prenom={p.Prenom}, Naissance={p.date_de_naissance}, Pathologie={p.Pathologie}, Cote={p.CoteNeglige}, Suivi={p.SuiviPatient}");
                else
                    Debug.Log($"[HospitalData]   Patient: {p.IDpatient} - {p.Prenom} {p.Nom}");
            }
        }

        void TestSuperviseurs(HospitalDataService data)
        {
            var manager = data.Superviseurs;

            var all = manager.GetAll();
            Debug.Log($"[HospitalData] Superviseurs: loaded {all.Count}");

            foreach (var s in all)
            {
                if (verbose)
                    Debug.Log($"[HospitalData]   Superviseur: Id={s.IdSuperviseur}, Nom={s.Nom}, Prenom={s.Prenom}, Fonction={s.fonction}");
                else
                    Debug.Log($"[HospitalData]   Superviseur: {s.IdSuperviseur} - {s.Prenom} {s.Nom} ({s.fonction})");
            }
        }

        void TestSessions(HospitalDataService data)
        {
            var manager = data.Sessions;

            var all = manager.GetAll();
            Debug.Log($"[HospitalData] Sessions: loaded {all.Count}");

            foreach (var s in all)
            {
                if (verbose)
                    Debug.Log($"[HospitalData]   Session: Patient={s.IDpatient}, Env={s.EnvironnementUtilise}, Pos={s.PositionDepart}, Duree={s.duree}s, Score={s.ScoreTotal}, Objectifs={s.ObjectifsAtteints}/{s.ObjectifsManques}, Commentaire={s.Commentaire}");
                else
                    Debug.Log($"[HospitalData]   Session: {s.IDpatient} | Score={s.ScoreTotal} | {s.Commentaire}");
            }

            if (all.Count > 0)
            {
                var firstPatient = all[0].IDpatient;
                var byPatient = manager.GetByPatient(firstPatient);
                Debug.Log($"[HospitalData] GetByPatient('{firstPatient}') => {byPatient.Count} session(s)");
            }
        }

        void TestAddPatients(HospitalDataService data)
        {
            var manager = data.Patients;
            int countBefore = manager.GetAll().Count;

            var added = new PatientJson
            {
                IDpatient = "",
                Nom = "TestAdd",
                Prenom = "Patient",
                date_de_naissance = 1990,
                Sexe = "M",
                Pathologie = "Test",
                CoteNeglige = "droit",
                SuiviPatient = "Test Add"
            };
            var result = manager.Add(added);
            int countAfter = manager.GetAll().Count;

            bool ok = countAfter == countBefore + 1 && !string.IsNullOrEmpty(result.IDpatient);
            Debug.Log(ok
                ? $"[HospitalData] Add Patient OK: id={result.IDpatient}, count {countBefore} -> {countAfter} (ajouté dans les_patients.json)"
                : $"[HospitalData] Add Patient FAIL: count {countBefore} -> {countAfter}");
        }

        void TestAddSuperviseurs(HospitalDataService data)
        {
            var manager = data.Superviseurs;
            int countBefore = manager.GetAll().Count;

            var added = new SuperviseurJson
            {
                IdSuperviseur = "",
                Nom = "TestAdd",
                Prenom = "Superviseur",
                fonction = "Test"
            };
            var result = manager.Add(added);
            int countAfter = manager.GetAll().Count;

            bool ok = countAfter == countBefore + 1 && !string.IsNullOrEmpty(result.IdSuperviseur);
            Debug.Log(ok
                ? $"[HospitalData] Add Superviseur OK: id={result.IdSuperviseur}, count {countBefore} -> {countAfter} (ajouté dans les_superviseur.json)"
                : $"[HospitalData] Add Superviseur FAIL: count {countBefore} -> {countAfter}");
        }

        void TestAddSessions(HospitalDataService data)
        {
            var manager = data.Sessions;
            var patients = data.Patients;
            var patient = patients.GetAll().Count > 0 ? patients.GetAll()[0] : null;
            if (patient == null)
            {
                Debug.Log("[HospitalData] Add Session SKIP: aucun patient");
                return;
            }

            int countBefore = manager.GetAll().Count;
            var added = new SessionJson
            {
                IDpatient = patient.IDpatient,
                EnvironnementUtilise = "Test",
                PositionDepart = "assis",
                DateDebut = System.DateTime.UtcNow,
                niveauDifficulte = "facile",
                NiveauAssistance_moyen = 0,
                ObjectifsAtteints = "oui",
                ObjectifsManques = "non",
                duree = 60,
                ScoreTotal = 100,
                IdSuperviseur = "",
                Commentaire = "Test Add Session"
            };
            var result = manager.Add(added);
            int countAfter = manager.GetAll().Count;

            bool ok = countAfter == countBefore + 1 && manager.GetByPatient(added.IDpatient).Count >= 1;
            Debug.Log(ok
                ? $"[HospitalData] Add Session OK: patient={result.IDpatient}, count {countBefore} -> {countAfter} (ajouté dans sessions.json)"
                : $"[HospitalData] Add Session FAIL: count {countBefore} -> {countAfter}");
        }
    }
}
