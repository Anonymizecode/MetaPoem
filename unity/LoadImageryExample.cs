// SPDX-License-Identifier: MIT
using UnityEngine;

namespace MetaPoem
{
    [RequireComponent(typeof(ImageryModelLoader))]
    public class LoadImageryExample : MonoBehaviour
    {
        public string[] imagery = { "明月", "荷花", "小桥" };

        void Start()
        {
            var loader = GetComponent<ImageryModelLoader>();
            loader.MissingImagery += (name, why) => Debug.LogWarning("Import a model for '" + name + "' (" + why + ")");
            for (int i = 0; i < imagery.Length; i++)
                loader.Spawn(imagery[i], transform, new Vector3(i * 3f, 0f, 0f));
        }
    }
}
