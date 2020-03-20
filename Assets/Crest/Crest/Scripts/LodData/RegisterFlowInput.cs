// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

namespace Crest
{
    /// <summary>
    /// Registers a custom input to the flow data. Attach this GameObjects that you want to influence the horizontal flow of the water volume.
    /// </summary>
    public class RegisterFlowInput : RegisterLodDataInput<LodDataMgrFlow>
    {
        public override float Wavelength => 0f;
    }
}
