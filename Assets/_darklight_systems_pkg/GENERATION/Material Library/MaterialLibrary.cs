using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight.ThirdDimensional.Generation
{
    [CreateAssetMenu(fileName = "NewMaterialLibrary", menuName = "WorldGeneration/MaterialLibrary", order = 1)]
    public class MaterialLibrary : ScriptableObject
    {
        [SerializeField] private Material _defaultGroundMaterial; 

        public Material DefaultGroundMaterial => _defaultGroundMaterial;

    }
}

