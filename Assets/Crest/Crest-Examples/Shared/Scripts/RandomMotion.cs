// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

using UnityEngine;

/// <summary>
/// Shoves the gameobject around random amounts, occasionally useful for debugging where some motion is required to reproduce an issue.
/// </summary>
public class RandomMotion : MonoBehaviour
{
    public Vector3 _axis = Vector3.up;
    Vector3 _orthoAxis;
    [Range( 0, 15 )]
    public float _amplitude = 1f;
    [Range( 0, 5 )]
    public float _freq = 1f;

    [Range(0, 1)]
    public float _orthogonalMotion = 0f;

    Vector3 _origin;

    void Start()
    {
        _origin = transform.position;

        _orthoAxis = Quaternion.AngleAxis(90f, Vector3.up) * _axis;
    }

    void Update()
    {
        // do circles in perlin noise
        float rnd = 2f * (Mathf.PerlinNoise(0.5f + 0.5f * Mathf.Cos(_freq * Time.time), 0.5f + 0.5f * Mathf.Sin(_freq * Time.time)) - 0.5f);

        float orthoPhaseOff = Mathf.PI / 2f;
        float rndOrtho = 2f * (Mathf.PerlinNoise(0.5f + 0.5f * Mathf.Cos(_freq * Time.time + orthoPhaseOff), 0.5f + 0.5f * Mathf.Sin(_freq * Time.time + orthoPhaseOff)) - 0.5f);

        transform.position = _origin + (_axis * rnd + _orthoAxis * rndOrtho * _orthogonalMotion) * _amplitude;
    }
}
