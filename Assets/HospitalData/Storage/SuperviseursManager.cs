using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hospital.Data.Models;
using Newtonsoft.Json;

namespace Hospital.Data.Storage
{
// Gestion simple du fichier Data/les_superviseur.json avec verrouillage lecture/écriture.
public class SuperviseursManager
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
    private List<SuperviseurJson> _superviseurs = new();

    // path peut être redéfini pour tests ou autre emplacement.
    public SuperviseursManager(string path = "Data/les_superviseur.json")
    {
        _path = path;
    }

    // Retourne une copie en lecture seule pour éviter les modifications concurrentes.
    public IReadOnlyList<SuperviseurJson> GetAll()
    {
        EnsureLoaded();
        _lock.EnterReadLock();
        try { return _superviseurs.ToList(); }
        finally { _lock.ExitReadLock(); }
    }

    // Ajoute un superviseur et génère un ID si manquant.
    public SuperviseurJson Add(SuperviseurJson superviseur)
    {
        if (superviseur == null) throw new ArgumentNullException(nameof(superviseur));
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            if (string.IsNullOrWhiteSpace(superviseur.IdSuperviseur)) superviseur.IdSuperviseur = $"sup-{Guid.NewGuid():N}";
            if (_superviseurs.Any(s => string.Equals(s.IdSuperviseur, superviseur.IdSuperviseur, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Un superviseur avec l'ID '{superviseur.IdSuperviseur}' existe déjà.");

            _superviseurs.Add(superviseur);
            PersistUnsafe();
            return superviseur;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Remplace entièrement l'entrée existante (même ID).
    public bool Update(SuperviseurJson superviseur)
    {
        if (superviseur == null) throw new ArgumentNullException(nameof(superviseur));
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            var existing = _superviseurs.FindIndex(s => string.Equals(s.IdSuperviseur, superviseur.IdSuperviseur, StringComparison.OrdinalIgnoreCase));
            if (existing < 0) return false;

            _superviseurs[existing] = superviseur;
            PersistUnsafe();
            return true;
        }
        finally { _lock.ExitWriteLock(); }
    }

    // Supprime un superviseur par ID.
    public bool Remove(string idSuperviseur)
    {
        _lock.EnterWriteLock();
        try
        {
            EnsureLoadedUnsafe();
            var removed = _superviseurs.RemoveAll(s => string.Equals(s.IdSuperviseur, idSuperviseur, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed) PersistUnsafe();
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
            _superviseurs = new List<SuperviseurJson>();
            return;
        }

        var json = File.ReadAllText(_path);
        _superviseurs = JsonConvert.DeserializeObject<List<SuperviseurJson>>(json, ReadSettings) ?? new List<SuperviseurJson>();
    }

    private void PersistUnsafe()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var json = JsonConvert.SerializeObject(_superviseurs, WriteSettings);
        File.WriteAllText(_path, json);
    }
}
}
