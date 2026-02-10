using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hospital.Data.Models;
using Newtonsoft.Json;

namespace Hospital.Data.Storage
{
// Gestion simple du fichier Data/sessions.json (format enveloppé) avec validation optionnelle
// des références patients/superviseurs et verrouillage lecture/écriture.
public class SessionsManager
{
    private readonly string _path;
    private readonly PatientsManager? _patients;
    private readonly SuperviseursManager? _superviseurs;
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
    private List<SessionJson> _sessions = new();

    // path peut être redéfini pour tests ou autre emplacement.
    public SessionsManager(string path = "Data/sessions.json")
    {
        _path = path;
    }

    // Fournir les manageurs patients/superviseurs active la validation des IDs lors des écritures.
    public SessionsManager(PatientsManager patientsManager, SuperviseursManager superviseursManager, string path = "Data/sessions.json") : this(path)
    {
        _patients = patientsManager;
        _superviseurs = superviseursManager;
    }

    // Retourne une copie en lecture seule pour éviter les modifications concurrentes.
    public IReadOnlyList<SessionJson> GetAll()
    {
        EnsureLoaded();
        _lock.EnterReadLock();
        try { return _sessions.ToList(); }
        finally { _lock.ExitReadLock(); }
    }

    // Filtre les sessions par patient.
    public IReadOnlyList<SessionJson> GetByPatient(string idPatient)
    {
        EnsureLoaded();
        _lock.EnterReadLock();
        try { return _sessions.Where(s => string.Equals(s.IDpatient, idPatient, StringComparison.OrdinalIgnoreCase)).ToList(); }
        finally { _lock.ExitReadLock(); }
    }

    // Ajoute une session et vérifie les références si les manageurs sont fournis.
    public SessionJson Add(SessionJson session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            ValidateReferences(session);
            session.DateDebut = Normalize(session.DateDebut);
            if (session.duree == 0 && session.DateDebut != default) session.duree = 0;
            _sessions.Add(session);
            PersistUnsafe();
            return session;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Remplace une session identifiée par (IdPatient + DateDebut).
    public bool Update(SessionJson session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            var index = _sessions.FindIndex(s => string.Equals(s.IDpatient, session.IDpatient, StringComparison.OrdinalIgnoreCase) && s.DateDebut == session.DateDebut);
            if (index < 0) return false;

            ValidateReferences(session);
            session.DateDebut = Normalize(session.DateDebut);
            _sessions[index] = session;
            PersistUnsafe();
            return true;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Supprime une session identifiée par (IdPatient + DateDebut).
    public bool Remove(string idPatient, DateTime dateDebut)
    {
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            var removed = _sessions.RemoveAll(s => string.Equals(s.IDpatient, idPatient, StringComparison.OrdinalIgnoreCase) && s.DateDebut == dateDebut) > 0;
            if (removed) PersistUnsafe();
            return removed;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Supprime toutes les sessions d'un patient (par son ID). Retourne le nombre de sessions supprimées.
    public int RemoveAllByPatient(string idPatient)
    {
        if (string.IsNullOrWhiteSpace(idPatient)) return 0;
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            int count = _sessions.RemoveAll(s => string.Equals(s.IDpatient, idPatient, StringComparison.OrdinalIgnoreCase));
            if (count > 0) PersistUnsafe();
            return count;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Vérifie que patient et superviseur existent si les manageurs ont été fournis.
    private void ValidateReferences(SessionJson session)
    {
        if (_patients != null && !_patients.GetAll().Any(p => string.Equals(p.IDpatient, session.IDpatient, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Patient '{session.IDpatient}' introuvable.");
        if (_superviseurs != null && !string.IsNullOrWhiteSpace(session.IdSuperviseur) && !_superviseurs.GetAll().Any(s => string.Equals(s.IdSuperviseur, session.IdSuperviseur, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Superviseur '{session.IdSuperviseur}' introuvable.");
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
            _sessions = new List<SessionJson>();
            return;
        }

        var json = File.ReadAllText(_path);

        // Cas le plus courant : tableau avec un objet {"Sessions": [...]}
        var envelopeList = JsonConvert.DeserializeObject<List<SessionEnvelope>>(json, ReadSettings);
        if (envelopeList != null && envelopeList.Count > 0)
        {
            _sessions = envelopeList.Where(e => e?.Sessions != null).SelectMany(e => e!.Sessions!).ToList();
            return;
        }

        // Fallback : un seul objet {"Sessions": [...]}
        var envelope = JsonConvert.DeserializeObject<SessionEnvelope>(json, ReadSettings);
        _sessions = envelope?.Sessions ?? new List<SessionJson>();
    }

    private void PersistUnsafe()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var payload = new List<SessionEnvelope> { new() { Sessions = _sessions } };
        var json = JsonConvert.SerializeObject(payload, WriteSettings);
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
