using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hospital.Data.Models;
using Newtonsoft.Json;

namespace Hospital.Data.Storage
{
// Gestion simple du fichier Data/les_patients.json avec verrouillage lecture/écriture.
public class PatientsManager
{
    private readonly string _path;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private static readonly JsonSerializerSettings ReadSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };
    private static readonly JsonSerializerSettings WriteSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    private bool _loaded;
    private List<PatientJson> _patients = new();

    // Déclenché après suppression d'un patient (pour supprimer ses sessions côté service).
    public event Action<string> AfterPatientRemoved;

    // path peut être redéfini pour les tests ou un autre dossier.
    public PatientsManager(string path = "Data/les_patients.json")
    {
        _path = path;
    }

    // Retourne une copie en lecture seule pour éviter les modifications concurrentes.
    public IReadOnlyList<PatientJson> GetAll()
    {
        EnsureLoaded();
        _lock.EnterReadLock();
        try { return _patients.ToList(); }
        finally { _lock.ExitReadLock(); }
    }

    // Ajoute un patient et génère un ID si manquant.
    public PatientJson Add(PatientJson patient)
    {
        if (patient == null) throw new ArgumentNullException(nameof(patient));
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            if (string.IsNullOrWhiteSpace(patient.IDpatient)) patient.IDpatient = $"patient-{Guid.NewGuid():N}";
            if (_patients.Any(p => string.Equals(p.IDpatient, patient.IDpatient, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Un patient avec l'ID '{patient.IDpatient}' existe déjà.");

            patient.DateCreation = Normalize(patient.DateCreation);
            _patients.Add(patient);
            PersistUnsafe();
            return patient;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Remplace entièrement l'entrée existante (même ID).
    public bool Update(PatientJson patient)
    {
        if (patient == null) throw new ArgumentNullException(nameof(patient));
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            var existing = _patients.FindIndex(p => string.Equals(p.IDpatient, patient.IDpatient, StringComparison.OrdinalIgnoreCase));
            if (existing < 0) return false;

            patient.DateCreation = Normalize(patient.DateCreation);
            _patients[existing] = patient;
            PersistUnsafe();
            return true;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Mise à jour ciblée du suivi sans toucher aux autres champs.
    public bool UpdateSuivi(string idPatient, string suiviPatient)
    {
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            var patient = _patients.FirstOrDefault(p => string.Equals(p.IDpatient, idPatient, StringComparison.OrdinalIgnoreCase));
            if (patient == null) return false;
            patient.SuiviPatient = suiviPatient ?? string.Empty;
            PersistUnsafe();
            return true;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Supprime un patient par ID.
    public bool Remove(string idPatient)
    {
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            var removed = _patients.RemoveAll(p => string.Equals(p.IDpatient, idPatient, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed)
            {
                PersistUnsafe();
                AfterPatientRemoved?.Invoke(idPatient);
            }
            return removed;
        }
        finally { _lock.ExitWriteLock(); }
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_loaded) return;
            _lock.EnterWriteLock();
            try { LoadFromDisk(); _loaded = true; }
            finally { _lock.ExitWriteLock(); }
        }
        finally { _lock.ExitUpgradeableReadLock(); }
    }

    private void EnsureLoadedUnsafe()
    {
        if (_loaded) return;
        LoadFromDisk();
        _loaded = true;
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_path))
        {
            _patients = new List<PatientJson>();
            return;
        }

        var json = File.ReadAllText(_path);
        _patients = JsonConvert.DeserializeObject<List<PatientJson>>(json, ReadSettings) ?? new List<PatientJson>();
    }

    private void PersistUnsafe()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var json = JsonConvert.SerializeObject(_patients, WriteSettings);
        File.WriteAllText(_path, json);
    }

    private static DateTime Normalize(DateTime value)
    {
        if (value == default) return DateTime.UtcNow;
        if (value.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return value.ToUniversalTime();
    }
}
}
