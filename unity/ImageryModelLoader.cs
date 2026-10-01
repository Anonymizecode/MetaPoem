// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaPoem
{
    [Serializable]
    public class ModelVariant
    {
        public string prefabGuid;
        public Vector3 euler;
        public float height;        // metres; 0 keeps the imported scale
        public string[] tags;
    }

    [Serializable]
    public class ImageryEntry
    {
        public int id;
        public string nameZh;
        public string nameEn;
        public List<ModelVariant> variants = new List<ModelVariant>();
    }

    [Serializable]
    public class ImageryCatalog
    {
        public List<ImageryEntry> imagery = new List<ImageryEntry>();

        [NonSerialized] Dictionary<string, ImageryEntry> _lookup;

        public static ImageryCatalog FromJson(string json) { return JsonUtility.FromJson<ImageryCatalog>(json); }

        public bool TryFind(string key, out ImageryEntry entry)
        {
            entry = null;
            if (string.IsNullOrEmpty(key)) return false;
            if (_lookup == null)
            {
                _lookup = new Dictionary<string, ImageryEntry>(StringComparer.OrdinalIgnoreCase);
                foreach (ImageryEntry e in imagery)
                {
                    if (!string.IsNullOrEmpty(e.nameZh)) _lookup[e.nameZh] = e;
                    if (!string.IsNullOrEmpty(e.nameEn)) _lookup[e.nameEn] = e;
                    _lookup[e.id.ToString()] = e;
                }
            }
            return _lookup.TryGetValue(key.Trim(), out entry);
        }
    }

    public class ImageryModelLoader : MonoBehaviour
    {
        public TextAsset manifest;

        [Tooltip("0 = a different random choice on every run")]
        public int seed = 0;

        public bool alignToGround = true;

        [Tooltip("Editor only: if no listed form loads, look for a prefab named after the imagery")]
        public bool searchLocalProjectAsFallback = true;

        public event Action<string, string> MissingImagery;   // (imagery name, reason)

        public Func<ModelVariant, GameObject> customResolver;

        public ImageryCatalog Catalog { get; set; }

        System.Random _rng;

        void Awake() { EnsureInitialised(); }

        void EnsureInitialised()
        {
            if (_rng == null) _rng = seed == 0 ? new System.Random() : new System.Random(seed);
            if (Catalog == null && manifest != null) Catalog = ImageryCatalog.FromJson(manifest.text);
        }

        public GameObject Spawn(string imagery, Transform parent = null, Vector3? position = null,
                                Quaternion? rotation = null, string tag = null)
        {
            EnsureInitialised();

            ImageryEntry entry;
            if (Catalog == null || !Catalog.TryFind(imagery, out entry))
            {
                ReportMissing(imagery, "not in the model list");
                return null;
            }

            GameObject prefab = null;
            ModelVariant chosen = null;
            foreach (ModelVariant v in ShuffledCandidates(entry, tag))
            {
                GameObject g = Resolve(v);
                if (g != null) { prefab = g; chosen = v; break; }
            }

#if UNITY_EDITOR
            if (prefab == null && searchLocalProjectAsFallback)
            {
                prefab = FindPrefabInOpenProject(entry.nameEn);
                if (prefab == null) prefab = FindPrefabInOpenProject(entry.nameZh);
            }
#endif
            if (prefab == null)
            {
                ReportMissing(imagery, "no form could be loaded (models not imported?)");
                return null;
            }

            Vector3 pos = position ?? Vector3.zero;
            Quaternion rot = rotation ?? Quaternion.identity;
            Vector3 canonicalEuler = chosen != null ? chosen.euler : Vector3.zero;

            GameObject instance = Instantiate(prefab, pos, rot * Quaternion.Euler(canonicalEuler), parent);
            instance.name = prefab.name;

            Bounds b;
            if (chosen != null && chosen.height > 0f && TryGetBounds(instance, out b) && b.size.y > 1e-4f)
                instance.transform.localScale *= chosen.height / b.size.y;

            if (alignToGround && TryGetBounds(instance, out b))
                instance.transform.position += Vector3.up * (pos.y - b.min.y);

            return instance;
        }

        // GUIDs resolve through the AssetDatabase (Editor only); set customResolver for player builds.
        GameObject Resolve(ModelVariant v)
        {
            if (customResolver != null)
            {
                GameObject g = customResolver(v);
                if (g != null) return g;
            }
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(v.prefabGuid))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(v.prefabGuid);
                if (!string.IsNullOrEmpty(path)) return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
#endif
            return null;
        }

        List<ModelVariant> ShuffledCandidates(ImageryEntry entry, string tag)
        {
            var list = new List<ModelVariant>();
            if (!string.IsNullOrEmpty(tag))
            {
                foreach (ModelVariant v in entry.variants)
                    if (v.tags != null && Array.IndexOf(v.tags, tag) >= 0) list.Add(v);
            }
            if (list.Count == 0) list.AddRange(entry.variants);

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                ModelVariant t = list[i]; list[i] = list[j]; list[j] = t;
            }
            return list;
        }

        static bool TryGetBounds(GameObject go, out Bounds bounds)
        {
            bounds = default(Bounds);
            bool any = false;
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
            {
                if (!any) { bounds = r.bounds; any = true; }
                else bounds.Encapsulate(r.bounds);
            }
            return any;
        }

        void ReportMissing(string imagery, string reason)
        {
            Debug.LogWarning("[MetaPoem] '" + imagery + "': " + reason);
            Action<string, string> handler = MissingImagery;
            if (handler != null) handler(imagery, reason);
        }

#if UNITY_EDITOR
        static GameObject FindPrefabInOpenProject(string query)
        {
            if (string.IsNullOrEmpty(query)) return null;
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets(query + " t:Prefab"))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null) return go;
            }
            return null;
        }
#endif
    }
}
