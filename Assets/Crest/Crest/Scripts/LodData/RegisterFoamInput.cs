// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

namespace Crest
{
    /// <summary>
    /// Registers a custom input to the foam simulation. Attach this GameObjects that you want to influence the foam simulation, such as depositing foam on the surface.
    /// </summary>
    public class RegisterFoamInput : RegisterLodDataInput<LodDataMgrFoam>
    {
        public override float Wavelength => 0f;
    }
}
