// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Assign this to depth masks - objects that will occlude the water. This ensures that the mask will render before any of the ocean surface.
    /// </summary>
    public class RegisterMaskInput : MonoBehaviour
    {
        void Start()
        {
            // Render before the surface mesh
            GetComponent<Renderer>().sortingOrder = -LodDataMgr.MAX_LOD_COUNT - 1;
        }
    }
}
