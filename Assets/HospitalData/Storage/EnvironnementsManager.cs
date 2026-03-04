using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Hospital.Data.Models;
using Newtonsoft.Json;

namespace Hospital.Data.Storage
{
    /// <summary>
    /// Gestion des environnements disponibles à partir du fichier environnements.json
    /// Permet de lire les environnements et leurs configurations disponibles
    /// </summary>
    public class EnvironnementsManager
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
        private List<EnvironnementJson> _environnements = new();

        public EnvironnementsManager(string path = "Data/environnements.json")
        {
            _path = path;
        }

        /// <summary>
        /// Retourne tous les environnements disponibles
        /// </summary>
        public IReadOnlyList<EnvironnementJson> GetAll()
        {
            EnsureLoaded();
            _lock.EnterReadLock();
            try { return _environnements.ToList(); }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Récupère un environnement par son ID
        /// </summary>
        public EnvironnementJson GetById(string idEnvironnement)
        {
            EnsureLoaded();
            _lock.EnterReadLock();
            try
            {
                return _environnements.FirstOrDefault(e =>
                    string.Equals(e.IdEnvironnement, idEnvironnement, StringComparison.OrdinalIgnoreCase));
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Ajoute un nouvel environnement
        /// </summary>
        public EnvironnementJson Add(EnvironnementJson environnement)
        {
            if (environnement == null) throw new ArgumentNullException(nameof(environnement));
            _lock.EnterWriteLock();
            try
            {
                EnsureLoadedUnsafe();
                if (string.IsNullOrWhiteSpace(environnement.IdEnvironnement))
                    environnement.IdEnvironnement = $"env-{Guid.NewGuid():N}";

                if (_environnements.Any(e => string.Equals(e.IdEnvironnement, environnement.IdEnvironnement, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Un environnement avec l'ID '{environnement.IdEnvironnement}' existe déjà.");

                _environnements.Add(environnement);
                PersistUnsafe();
                return environnement;
            }
            finally { _lock.ExitWriteLock(); }
        }

        /// <summary>
        /// Met à jour un environnement existant
        /// </summary>
        public bool Update(EnvironnementJson environnement)
        {
            if (environnement == null) throw new ArgumentNullException(nameof(environnement));
            _lock.EnterWriteLock();
            try
            {
                EnsureLoadedUnsafe();
                var existing = _environnements.FindIndex(e =>
                    string.Equals(e.IdEnvironnement, environnement.IdEnvironnement, StringComparison.OrdinalIgnoreCase));
                if (existing < 0) return false;

                _environnements[existing] = environnement;
                PersistUnsafe();
                return true;
            }
            finally { _lock.ExitWriteLock(); }
        }

        /// <summary>
        /// Supprime un environnement par son ID
        /// </summary>
        public bool Remove(string idEnvironnement)
        {
            _lock.EnterWriteLock();
            try
            {
                EnsureLoadedUnsafe();
                var removed = _environnements.RemoveAll(e =>
                    string.Equals(e.IdEnvironnement, idEnvironnement, StringComparison.OrdinalIgnoreCase));
                if (removed > 0) PersistUnsafe();
                return removed > 0;
            }
            finally { _lock.ExitWriteLock(); }
        }

        /// <summary>
        /// Recharge les environnements depuis le fichier
        /// </summary>
        public void Reload()
        {
            _lock.EnterWriteLock();
            try
            {
                _loaded = false;
                EnsureLoadedUnsafe();
            }
            finally { _lock.ExitWriteLock(); }
        }

        private void EnsureLoaded()
        {
            if (_loaded) return;
            _lock.EnterWriteLock();
            try { EnsureLoadedUnsafe(); }
            finally { _lock.ExitWriteLock(); }
        }

        private void EnsureLoadedUnsafe()
        {
            if (_loaded) return;

            var fullPath = ToFullPath(_path);
            if (!File.Exists(fullPath))
            {
                UnityEngine.Debug.LogWarning($"[Environnements] Fichier introuvable: {fullPath}");
                _environnements = new List<EnvironnementJson>();
                _loaded = true;
                return;
            }

            try
            {
                var json = File.ReadAllText(fullPath);
                var envelopes = JsonConvert.DeserializeObject<List<EnvironnementEnvelope>>(json, ReadSettings) ?? new List<EnvironnementEnvelope>();
                _environnements = envelopes.SelectMany(e => e.Environnements ?? new List<EnvironnementJson>()).ToList();
                UnityEngine.Debug.Log($"[Environnements] Chargé: {fullPath}, {_environnements.Count} environnement(s)");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Environnements] Erreur lors du chargement de {fullPath} : {ex.Message}\n{ex.StackTrace}");
                _environnements = new List<EnvironnementJson>();
            }

            _loaded = true;
        }

        private void PersistUnsafe()
        {
            var fullPath = ToFullPath(_path);
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var envelope = new List<EnvironnementEnvelope>
            {
                new EnvironnementEnvelope { Environnements = _environnements }
            };

            var json = JsonConvert.SerializeObject(envelope, WriteSettings);
            File.WriteAllText(fullPath, json);
        }

        private static string ToFullPath(string path)
        {
            if (string.IsNullOrEmpty(path)) path = "environnements.json";
            var fileName = Path.GetFileName(path);
#if UNITY_EDITOR
            var combined = Path.Combine(UnityEngine.Application.dataPath, "HospitalData", "StreamingAssets", fileName);
            if (Path.IsPathRooted(path) && File.Exists(path))
                return path;
            return Path.GetFullPath(combined);
#else
            return Path.Combine(UnityEngine.Application.streamingAssetsPath, "HospitalData", "StreamingAssets", fileName);
#endif
        }
    }
}
