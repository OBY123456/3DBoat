// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Default time provider - sets the ocean time to Unity's game time.
    /// </summary>
    public class TimeProviderDefault : TimeProviderBase
    {
        public override float CurrentTime => Time.time;
        public override float DeltaTime => Time.deltaTime;
    }
}
