﻿// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles & Tom Read Cutting

using UnityEngine;
using UnityEngine.Rendering;

namespace Crest
{
    /// <summary>
    /// Stores shadowing data to use during ocean shading. Shadowing is persistent and supports sampling across
    /// many frames and jittered sampling for (very) soft shadows.
    /// </summary>
    public class LodDataMgrShadow : LodDataMgr
    {
        public override string SimName { get { return "Shadow"; } }
        public override RenderTextureFormat TextureFormat { get { return RenderTextureFormat.RG16; } }
        protected override bool NeedToReadWriteTextureData { get { return true; } }

        public static bool s_processData = true;

        Light _mainLight;
        Camera _cameraMain;

        // LWRP version needs access to this externally, hence public get
        public CommandBuffer BufCopyShadowMap { get; private set; }

        RenderTexture _sources;
        PropertyWrapperMaterial[] _renderMaterial;

        static int sp_CenterPos = Shader.PropertyToID("_CenterPos");
        static int sp_Scale = Shader.PropertyToID("_Scale");
        static int sp_CamPos = Shader.PropertyToID("_CamPos");
        static int sp_CamForward = Shader.PropertyToID("_CamForward");
        static int sp_JitterDiameters_CurrentFrameWeights = Shader.PropertyToID("_JitterDiameters_CurrentFrameWeights");
        static int sp_MainCameraProjectionMatrix = Shader.PropertyToID("_MainCameraProjectionMatrix");
        static int sp_SimDeltaTime = Shader.PropertyToID("_SimDeltaTime");
        static int sp_LD_SliceIndex_Source = Shader.PropertyToID("_LD_SliceIndex_Source");

        SimSettingsShadow Settings { get { return OceanRenderer.Instance._simSettingsShadow; } }
        public override void UseSettings(SimSettingsBase settings) { OceanRenderer.Instance._simSettingsShadow = settings as SimSettingsShadow; }
        public override SimSettingsBase CreateDefaultSettings()
        {
            var settings = ScriptableObject.CreateInstance<SimSettingsShadow>();
            settings.name = SimName + " Auto-generated Settings";
            return settings;
        }

        protected override void Start()
        {
            base.Start();

#if UNITY_2018
            Debug.LogError("Shadowing not enabled on preview versions of LWRP. Upgrade to 2019 is required.", this);
#endif
            {
                _renderMaterial = new PropertyWrapperMaterial[OceanRenderer.Instance.CurrentLodCount];
                var shader = Shader.Find("Hidden/Crest/Simulation/Update Shadow");
                for (int i = 0; i < _renderMaterial.Length; i++)
                {
                    _renderMaterial[i] = new PropertyWrapperMaterial(shader);
                }
            }

            if (!SampleShadows.Created)
            {
                Debug.LogError("To support shadowing, a Custom renderer must be configured on the pipeline asset, and this custom renderer data must have the Sample Shadows feature added.", GraphicsSettings.renderPipelineAsset);
            }

            _cameraMain = Camera.main;
            if (_cameraMain == null)
            {
                var viewpoint = OceanRenderer.Instance.Viewpoint;
                _cameraMain = viewpoint != null ? viewpoint.GetComponent<Camera>() : null;

                if (_cameraMain == null)
                {
                    Debug.LogError("Could not find main camera, disabling shadow data", this);
                    enabled = false;
                    return;
                }
            }

#if UNITY_EDITOR
            if (!OceanRenderer.Instance.OceanMaterial.IsKeywordEnabled("_SHADOWS_ON"))
            {
                Debug.LogWarning("Shadowing is not enabled on the current ocean material and will not be visible.", this);
            }
#endif
        }

        protected override void InitData()
        {
            base.InitData();

            int resolution = OceanRenderer.Instance.LodDataResolution;
            var desc = new RenderTextureDescriptor(resolution, resolution, TextureFormat, 0);
            _sources = CreateLodDataTextures(desc, SimName + "_1", NeedToReadWriteTextureData);

            TextureArrayHelpers.ClearToBlack(_sources);
            TextureArrayHelpers.ClearToBlack(_targets);
        }

        bool StartInitLight()
        {
            if (_mainLight == null)
            {
                _mainLight = OceanRenderer.Instance._primaryLight;

                if (_mainLight == null)
                {
                    if (!Settings._allowNullLight)
                    {
                        Debug.LogWarning("Primary light must be specified on OceanRenderer script to enable shadows.", this);
                    }
                    return false;
                }

                if (_mainLight.type != LightType.Directional)
                {
                    Debug.LogError("Primary light must be of type Directional.", this);
                    return false;
                }

                if (_mainLight.shadows == LightShadows.None)
                {
                    Debug.LogError("Shadows must be enabled on primary light to enable ocean shadowing (types Hard and Soft are equivalent for the ocean system).", this);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// May happen if scenes change etc
        /// </summary>
        void ClearBufferIfLightChanged()
        {
            if (_mainLight != OceanRenderer.Instance._primaryLight)
            {
                if (_mainLight)
                {
                    BufCopyShadowMap = null;
                    TextureArrayHelpers.ClearToBlack(_sources);
                    TextureArrayHelpers.ClearToBlack(_targets);
                }
                _mainLight = null;
            }
        }

        public override void UpdateLodData()
        {
            if (!enabled)
            {
                return;
            }

            base.UpdateLodData();

            ClearBufferIfLightChanged();

            if (!StartInitLight())
            {
                enabled = false;
                return;
            }

            if (!s_processData)
            {
                return;
            }

            if (BufCopyShadowMap == null)
            {
                BufCopyShadowMap = new CommandBuffer();
                BufCopyShadowMap.name = "Shadow data";
            }

            if (!s_processData)
            {
                return;
            }

            Swap(ref _sources, ref _targets);

            BufCopyShadowMap.Clear();

            ValidateSourceData();

            // clear the shadow collection. it will be overwritten with shadow values IF the shadows render,
            // which only happens if there are (nontransparent) shadow receivers around
            TextureArrayHelpers.ClearToBlack(_targets);

            using (new ProfilingSample(BufCopyShadowMap, "CrestSampleShadows"))
            {
                var lt = OceanRenderer.Instance._lodTransform;
                for (var lodIdx = lt.LodCount - 1; lodIdx >= 0; lodIdx--)
                {
                    lt._renderData[lodIdx].Validate(0, this);
                    _renderMaterial[lodIdx].SetVector(sp_CenterPos, lt._renderData[lodIdx]._posSnapped);
                    var scale = OceanRenderer.Instance.CalcLodScale(lodIdx);
                    _renderMaterial[lodIdx].SetVector(sp_Scale, new Vector3(scale, 1f, scale));
                    _renderMaterial[lodIdx].SetVector(sp_JitterDiameters_CurrentFrameWeights, new Vector4(Settings._jitterDiameterSoft, Settings._jitterDiameterHard, Settings._currentFrameWeightSoft, Settings._currentFrameWeightHard));
                    _renderMaterial[lodIdx].SetMatrix(sp_MainCameraProjectionMatrix, _cameraMain.projectionMatrix * _cameraMain.worldToCameraMatrix);
                    _renderMaterial[lodIdx].SetFloat(sp_SimDeltaTime, Time.deltaTime);

                    // compute which lod data we are sampling previous frame shadows from. if a scale change has happened this can be any lod up or down the chain.
                    var srcDataIdx = lodIdx + ScaleDifferencePow2;
                    srcDataIdx = Mathf.Clamp(srcDataIdx, 0, lt.LodCount - 1);
                    _renderMaterial[lodIdx].SetInt(sp_LD_SliceIndex, lodIdx);
                    _renderMaterial[lodIdx].SetInt(sp_LD_SliceIndex_Source, srcDataIdx);
                    BindSourceData(_renderMaterial[lodIdx], false);
                    BufCopyShadowMap.Blit(Texture2D.blackTexture, _targets, _renderMaterial[lodIdx].material, -1, lodIdx);
                }
            }
        }

        public void ValidateSourceData()
        {
            foreach (var renderData in OceanRenderer.Instance._lodTransform._renderDataSource)
            {
                renderData.Validate(BuildCommandBufferBase._lastUpdateFrame - Time.frameCount, this);
            }
        }

        public void BindSourceData(IPropertyWrapper simMaterial, bool paramsOnly)
        {
            var rd = OceanRenderer.Instance._lodTransform._renderDataSource;
            BindData(simMaterial, paramsOnly ? Texture2D.blackTexture : _sources as Texture, true, ref rd, true);
        }

        public static string TextureArrayName = "_LD_TexArray_Shadow";
        private static TextureArrayParamIds textureArrayParamIds = new TextureArrayParamIds(TextureArrayName);
        public static int ParamIdSampler(bool sourceLod = false) { return textureArrayParamIds.GetId(sourceLod); }
        protected override int GetParamIdSampler(bool sourceLod = false)
        {
            return ParamIdSampler(sourceLod);
        }
        public static void BindNull(IPropertyWrapper properties, bool sourceLod = false)
        {
            properties.SetTexture(ParamIdSampler(sourceLod), TextureArrayHelpers.BlackTextureArray);
        }
    }
}
