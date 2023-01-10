#region File Description
//-----------------------------------------------------------------------------
// SkinnedEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Stardew3D
{

    /// <summary>
    /// Track which effect parameters need to be recomputed during the next OnApply.
    /// </summary>
    [Flags]
    internal enum EffectDirtyFlags
    {
        WorldViewProj = 1,
        World = 2,
        EyePosition = 4,
        MaterialColor = 8,
        Fog = 16,
        FogEnable = 32,
        AlphaTest = 64,
        ShaderIndex = 128,
        All = -1
    }


    /// <summary>
    /// Helper code shared between the various built-in effects.
    /// </summary>
    internal static class EffectHelpers
    {
        /// <summary>
        /// Sets up the standard key/fill/back lighting rig.
        /// </summary>
        internal static Vector3 EnableDefaultLighting(DirectionalLight light0, DirectionalLight light1, DirectionalLight light2)
        {
            // Key light.
            light0.Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f);
            light0.DiffuseColor = new Vector3(1, 0.9607844f, 0.8078432f);
            light0.SpecularColor = new Vector3(1, 0.9607844f, 0.8078432f);
            light0.Enabled = true;

            // Fill light.
            light1.Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f);
            light1.DiffuseColor = new Vector3(0.9647059f, 0.7607844f, 0.4078432f);
            light1.SpecularColor = Vector3.Zero;
            light1.Enabled = true;

            // Back light.
            light2.Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f);
            light2.DiffuseColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
            light2.SpecularColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
            light2.Enabled = true;

            // Ambient light.
            return new Vector3(0.05333332f, 0.09882354f, 0.1819608f);
        }


        /// <summary>
        /// Lazily recomputes the world+view+projection matrix and
        /// fog vector based on the current effect parameter settings.
        /// </summary>
        internal static EffectDirtyFlags SetWorldViewProjAndFog(EffectDirtyFlags dirtyFlags,
                                                                ref Matrix world, ref Matrix view, ref Matrix projection, ref Matrix worldView,
                                                                bool fogEnabled, float fogStart, float fogEnd,
                                                                EffectParameter worldViewProjParam, EffectParameter fogVectorParam)
        {
            // Recompute the world+view+projection matrix?
            if ((dirtyFlags & EffectDirtyFlags.WorldViewProj) != 0)
            {
                Matrix worldViewProj;

                Matrix.Multiply(ref world, ref view, out worldView);
                Matrix.Multiply(ref worldView, ref projection, out worldViewProj);

                worldViewProjParam.SetValue(worldViewProj);

                dirtyFlags &= ~EffectDirtyFlags.WorldViewProj;
            }

            if (fogEnabled)
            {
                // Recompute the fog vector?
                if ((dirtyFlags & (EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable)) != 0)
                {
                    SetFogVector(ref worldView, fogStart, fogEnd, fogVectorParam);

                    dirtyFlags &= ~(EffectDirtyFlags.Fog | EffectDirtyFlags.FogEnable);
                }
            }
            else
            {
                // When fog is disabled, make sure the fog vector is reset to zero.
                if ((dirtyFlags & EffectDirtyFlags.FogEnable) != 0)
                {
                    fogVectorParam.SetValue(Vector4.Zero);

                    dirtyFlags &= ~EffectDirtyFlags.FogEnable;
                }
            }

            return dirtyFlags;
        }


        /// <summary>
        /// Sets a vector which can be dotted with the object space vertex position to compute fog amount.
        /// </summary>
        static void SetFogVector(ref Matrix worldView, float fogStart, float fogEnd, EffectParameter fogVectorParam)
        {
            if (fogStart == fogEnd)
            {
                // Degenerate case: force everything to 100% fogged if start and end are the same.
                fogVectorParam.SetValue(new Vector4(0, 0, 0, 1));
            }
            else
            {
                // We want to transform vertex positions into view space, take the resulting
                // Z value, then scale and offset according to the fog start/end distances.
                // Because we only care about the Z component, the shader can do all this
                // with a single dot product, using only the Z row of the world+view matrix.

                float scale = 1f / (fogStart - fogEnd);

                Vector4 fogVector = new Vector4();

                fogVector.X = worldView.M13 * scale;
                fogVector.Y = worldView.M23 * scale;
                fogVector.Z = worldView.M33 * scale;
                fogVector.W = (worldView.M43 + fogStart) * scale;

                fogVectorParam.SetValue(fogVector);
            }
        }


        /// <summary>
        /// Lazily recomputes the world inverse transpose matrix and
        /// eye position based on the current effect parameter settings.
        /// </summary>
        internal static EffectDirtyFlags SetLightingMatrices(EffectDirtyFlags dirtyFlags, ref Matrix world, ref Matrix view,
                                                             EffectParameter worldParam, EffectParameter worldInverseTransposeParam, EffectParameter eyePositionParam)
        {
            // Set the world and world inverse transpose matrices.
            if ((dirtyFlags & EffectDirtyFlags.World) != 0)
            {
                Matrix worldTranspose;
                Matrix worldInverseTranspose;

                Matrix.Invert(ref world, out worldTranspose);
                Matrix.Transpose(ref worldTranspose, out worldInverseTranspose);

                worldParam.SetValue(world);
                worldInverseTransposeParam.SetValue(worldInverseTranspose);

                dirtyFlags &= ~EffectDirtyFlags.World;
            }

            // Set the eye position.
            if ((dirtyFlags & EffectDirtyFlags.EyePosition) != 0)
            {
                Matrix viewInverse;

                Matrix.Invert(ref view, out viewInverse);

                eyePositionParam.SetValue(viewInverse.Translation);

                dirtyFlags &= ~EffectDirtyFlags.EyePosition;
            }

            return dirtyFlags;
        }


        /// <summary>
        /// Sets the diffuse/emissive/alpha material color parameters.
        /// </summary>
        internal static void SetMaterialColor(bool lightingEnabled, float alpha,
                                              ref Vector3 diffuseColor, ref Vector3 emissiveColor, ref Vector3 ambientLightColor,
                                              EffectParameter diffuseColorParam, EffectParameter emissiveColorParam)
        {
            // Desired lighting model:
            //
            //     ((AmbientLightColor + sum(diffuse directional light)) * DiffuseColor) + EmissiveColor
            //
            // When lighting is disabled, ambient and directional lights are ignored, leaving:
            //
            //     DiffuseColor + EmissiveColor
            //
            // For the lighting disabled case, we can save one shader instruction by precomputing
            // diffuse+emissive on the CPU, after which the shader can use DiffuseColor directly,
            // ignoring its emissive parameter.
            //
            // When lighting is enabled, we can merge the ambient and emissive settings. If we
            // set our emissive parameter to emissive+(ambient*diffuse), the shader no longer
            // needs to bother adding the ambient contribution, simplifying its computation to:
            //
            //     (sum(diffuse directional light) * DiffuseColor) + EmissiveColor
            //
            // For futher optimization goodness, we merge material alpha with the diffuse
            // color parameter, and premultiply all color values by this alpha.

            if (lightingEnabled)
            {
                Vector4 diffuse = new Vector4();
                Vector3 emissive = new Vector3();

                diffuse.X = diffuseColor.X * alpha;
                diffuse.Y = diffuseColor.Y * alpha;
                diffuse.Z = diffuseColor.Z * alpha;
                diffuse.W = alpha;

                emissive.X = (emissiveColor.X + ambientLightColor.X * diffuseColor.X) * alpha;
                emissive.Y = (emissiveColor.Y + ambientLightColor.Y * diffuseColor.Y) * alpha;
                emissive.Z = (emissiveColor.Z + ambientLightColor.Z * diffuseColor.Z) * alpha;

                diffuseColorParam.SetValue(diffuse);
                emissiveColorParam.SetValue(emissive);
            }
            else
            {
                Vector4 diffuse = new Vector4();

                diffuse.X = (diffuseColor.X + emissiveColor.X) * alpha;
                diffuse.Y = (diffuseColor.Y + emissiveColor.Y) * alpha;
                diffuse.Z = (diffuseColor.Z + emissiveColor.Z) * alpha;
                diffuse.W = alpha;

                diffuseColorParam.SetValue(diffuse);
            }
        }
    }

    /// <summary>
    /// Built-in effect for rendering skinned character models.
    /// </summary>
    public class SkinnedEffectBony : Effect, IEffectMatrices, IEffectLights, IEffectFog
    {
        public const int MaxBones = 216;

        #region Effect Parameters

        EffectParameter textureParam;
        EffectParameter diffuseColorParam;
        EffectParameter emissiveColorParam;
        EffectParameter specularColorParam;
        EffectParameter specularPowerParam;
        EffectParameter eyePositionParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;
        EffectParameter worldParam;
        EffectParameter worldInverseTransposeParam;
        EffectParameter worldViewProjParam;
        EffectParameter bonesParam;

        int _shaderIndex = -1;

        #endregion

        #region Fields

        bool preferPerPixelLighting;
        bool oneLight;
        bool fogEnabled;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        Vector3 diffuseColor = Vector3.One;
        Vector3 emissiveColor = Vector3.Zero;
        Vector3 ambientLightColor = Vector3.Zero;

        float alpha = 1;

        DirectionalLight light0;
        DirectionalLight light1;
        DirectionalLight light2;

        float fogStart = 0;
        float fogEnd = 1;

        int weightsPerVertex = 4;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

        public static byte[] Bytecode = null; /*LoadEffectResource(
#if DIRECTX
            "Microsoft.Xna.Framework.Graphics.Effect.Resources.SkinnedEffect.dx11.mgfxo"
#else
            "Microsoft.Xna.Framework.Graphics.Effect.Resources.SkinnedEffect.ogl.mgfxo"
#endif
        );*/

        #endregion

        #region Public Properties


        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }

            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get { return view; }

            set
            {
                view = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }

            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
            }
        }


        /// <summary>
        /// Gets or sets the material diffuse color (range 0 to 1).
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }

            set
            {
                diffuseColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material emissive color (range 0 to 1).
        /// </summary>
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }

            set
            {
                emissiveColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material specular color (range 0 to 1).
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return specularColorParam.GetValueVector3(); }
            set { specularColorParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the material specular power.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPowerParam.GetValueSingle(); }
            set { specularPowerParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the material alpha.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }

            set
            {
                alpha = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the per-pixel lighting prefer flag.
        /// </summary>
        public bool PreferPerPixelLighting
        {
            get { return preferPerPixelLighting; }

            set
            {
                if (preferPerPixelLighting != value)
                {
                    preferPerPixelLighting = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }


        /// <summary>
        /// Gets or sets the ambient light color (range 0 to 1).
        /// </summary>
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }

            set
            {
                ambientLightColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets the first directional light.
        /// </summary>
        public DirectionalLight DirectionalLight0 { get { return light0; } }


        /// <summary>
        /// Gets the second directional light.
        /// </summary>
        public DirectionalLight DirectionalLight1 { get { return light1; } }


        /// <summary>
        /// Gets the third directional light.
        /// </summary>
        public DirectionalLight DirectionalLight2 { get { return light2; } }


        /// <summary>
        /// Gets or sets the fog enable flag.
        /// </summary>
        public bool FogEnabled
        {
            get { return fogEnabled; }

            set
            {
                if (fogEnabled != value)
                {
                    fogEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.FogEnable;
                }
            }
        }


        /// <summary>
        /// Gets or sets the fog start distance.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }

            set
            {
                fogStart = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog end distance.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }

            set
            {
                fogEnd = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColorParam.GetValueVector3(); }
            set { fogColorParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the current texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return textureParam.GetValueTexture2D(); }
            set { textureParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the number of skinning weights to evaluate for each vertex (1, 2, or 4).
        /// </summary>
        public int WeightsPerVertex
        {
            get { return weightsPerVertex; }

            set
            {
                if ((value != 1) &&
                    (value != 2) &&
                    (value != 4))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                weightsPerVertex = value;
                dirtyFlags |= EffectDirtyFlags.ShaderIndex;
            }
        }


        /// <summary>
        /// Sets an array of skinning bone transform matrices.
        /// </summary>
        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            if ((boneTransforms == null) || (boneTransforms.Length == 0))
                throw new ArgumentNullException("boneTransforms");

            if (boneTransforms.Length > MaxBones)
                throw new ArgumentException();

            bonesParam.SetValue(boneTransforms);
        }


        /// <summary>
        /// Gets a copy of the current skinning bone transform matrices.
        /// </summary>
        public Matrix[] GetBoneTransforms(int count)
        {
            if (count <= 0 || count > MaxBones)
                throw new ArgumentOutOfRangeException("count");

            Matrix[] bones = bonesParam.GetValueMatrixArray(count);

            // Convert matrices from 43 to 44 format.
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].M44 = 1;
            }

            return bones;
        }


        /// <summary>
        /// This effect requires lighting, so we explicitly implement
        /// IEffectLights.LightingEnabled, and do not allow turning it off.
        /// </summary>
        bool IEffectLights.LightingEnabled
        {
            get { return true; }
            set { if (!value) throw new NotSupportedException("SkinnedEffect does not support setting LightingEnabled to false."); }
        }


        #endregion

        #region Methods


        /// <summary>
        /// Creates a new SkinnedEffect with default parameter settings.
        /// </summary>
        public SkinnedEffectBony(GraphicsDevice device)
            : base(device, Bytecode)
        {
            CacheEffectParameters(null);

            DirectionalLight0.Enabled = true;

            SpecularColor = Vector3.One;
            SpecularPower = 16;

            Matrix[] identityBones = new Matrix[MaxBones];

            for (int i = 0; i < MaxBones; i++)
            {
                identityBones[i] = Matrix.Identity;
            }

            SetBoneTransforms(identityBones);
        }


        /// <summary>
        /// Creates a new SkinnedEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected SkinnedEffectBony(SkinnedEffectBony cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);

            preferPerPixelLighting = cloneSource.preferPerPixelLighting;
            fogEnabled = cloneSource.fogEnabled;

            world = cloneSource.world;
            view = cloneSource.view;
            projection = cloneSource.projection;

            diffuseColor = cloneSource.diffuseColor;
            emissiveColor = cloneSource.emissiveColor;
            ambientLightColor = cloneSource.ambientLightColor;

            alpha = cloneSource.alpha;

            fogStart = cloneSource.fogStart;
            fogEnd = cloneSource.fogEnd;

            weightsPerVertex = cloneSource.weightsPerVertex;
        }


        /// <summary>
        /// Creates a clone of the current SkinnedEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new SkinnedEffectBony(this);
        }


        /// <summary>
        /// Sets up the standard key/fill/back lighting rig.
        /// </summary>
        public void EnableDefaultLighting()
        {
            AmbientLightColor = EffectHelpers.EnableDefaultLighting(light0, light1, light2);
        }


        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters(SkinnedEffectBony cloneSource)
        {
            textureParam = Parameters["Texture"];
            diffuseColorParam = Parameters["DiffuseColor"];
            emissiveColorParam = Parameters["EmissiveColor"];
            specularColorParam = Parameters["SpecularColor"];
            specularPowerParam = Parameters["SpecularPower"];
            eyePositionParam = Parameters["EyePosition"];
            fogColorParam = Parameters["FogColor"];
            fogVectorParam = Parameters["FogVector"];
            worldParam = Parameters["World"];
            worldInverseTransposeParam = Parameters["WorldInverseTranspose"];
            worldViewProjParam = Parameters["WorldViewProj"];
            bonesParam = Parameters["Bones"];

            light0 = new DirectionalLight(Parameters["DirLight0Direction"],
                                          Parameters["DirLight0DiffuseColor"],
                                          Parameters["DirLight0SpecularColor"],
                                          (cloneSource != null) ? cloneSource.DirectionalLight0 : null);

            light1 = new DirectionalLight(Parameters["DirLight1Direction"],
                                          Parameters["DirLight1DiffuseColor"],
                                          Parameters["DirLight1SpecularColor"],
                                          (cloneSource != null) ? cloneSource.DirectionalLight1 : null);

            light2 = new DirectionalLight(Parameters["DirLight2Direction"],
                                          Parameters["DirLight2DiffuseColor"],
                                          Parameters["DirLight2SpecularColor"],
                                          (cloneSource != null) ? cloneSource.DirectionalLight2 : null);
        }


        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        
        protected override void OnApply()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

            // Recompute the world inverse transpose and eye position?
            dirtyFlags = EffectHelpers.SetLightingMatrices(dirtyFlags, ref world, ref view, worldParam, worldInverseTransposeParam, eyePositionParam);

            // Recompute the diffuse/emissive/alpha material color parameters?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                EffectHelpers.SetMaterialColor(true, alpha, ref diffuseColor, ref emissiveColor, ref ambientLightColor, diffuseColorParam, emissiveColorParam);

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            // Check if we can use the only-bother-with-the-first-light shader optimization.
            bool newOneLight = !light1.Enabled && !light2.Enabled;

            if (oneLight != newOneLight)
            {
                oneLight = newOneLight;
                dirtyFlags |= EffectDirtyFlags.ShaderIndex;
            }

            // Recompute the shader index?
            if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                int shaderIndex = 0;

                if (!fogEnabled)
                    shaderIndex += 1;

                if (weightsPerVertex == 2)
                    shaderIndex += 2;
                else if (weightsPerVertex == 4)
                    shaderIndex += 4;

                if (preferPerPixelLighting)
                    shaderIndex += 12;
                else if (oneLight)
                    shaderIndex += 6;

                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

                if (_shaderIndex != shaderIndex)
                {
                    _shaderIndex = shaderIndex;
                    CurrentTechnique = Techniques[_shaderIndex];
                    //return true;
                }
            }

            //return false;
        }


        #endregion
    }
}
