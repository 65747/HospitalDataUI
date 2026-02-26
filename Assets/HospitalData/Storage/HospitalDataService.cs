using System.IO;
using UnityEngine;

namespace Hospital.Data.Storage
{
    // Point d'accès unique aux données hospitalières. Toute lecture/écriture se fait uniquement via les manageurs.
    // Exemple : HospitalDataService.Instance.Patients.GetAll(), .Patients.Add(...), etc.
    public class HospitalDataService
    {
        static HospitalDataService _instance;
        readonly string _basePath;

        PatientsManager _patients;
        SuperviseursManager _superviseurs;
        SessionsManager _sessions;
        EnvironnementsManager _environnements;

        HospitalDataService()
        {
            _basePath = Path.Combine(Application.dataPath, "HospitalData", "StreamingAssets");
        }

        public static HospitalDataService Instance => _instance ??= new HospitalDataService();

        // Chemin du dossier contenant les JSON (pour débogage ou chemin personnalisé).
        public string BasePath => _basePath;

        // Manageur des patients (les_patients.json). À la première utilisation, on s'abonne pour supprimer ses sessions quand un patient est supprimé.
        public PatientsManager Patients
        {
            get
            {
                if (_patients == null)
                {
                    _patients = new PatientsManager(Path.Combine(_basePath, "les_patients.json"));
                    _patients.AfterPatientRemoved += id => Sessions.RemoveAllByPatient(id);
                }
                return _patients;
            }
        }

        // Manageur des superviseurs (les_superviseur.json).
        public SuperviseursManager Superviseurs => _superviseurs ??= new SuperviseursManager(Path.Combine(_basePath, "les_superviseur.json"));

        // Manageur des sessions (sessions.json). Vérifie que l'ID patient et superviseur existent lors de Add/Update.
        public SessionsManager Sessions => _sessions ??= new SessionsManager(Patients, Superviseurs, Path.Combine(_basePath, "sessions.json"));

        // Manageur des environnements (environnements.json).
        public EnvironnementsManager Environnements => _environnements ??= new EnvironnementsManager(Path.Combine(_basePath, "environnements.json"));
    }
}
