using RaceManager.Effects;
using System;
using UnityEngine;
using AudioType = RaceManager.Effects.AudioType;

namespace RaceManager.DamageSystem
{
    /// <summary>
    /// Attach this class to a glass or light object to make it damageable 
    /// </summary>
    public class DamageableGlass : DamageableObject, ISfxEventSource
    {
        [Tooltip("Material applied to the object after complete damage, if this field is null then the object will not be visible after destruction")]
        [SerializeField] private Material _brokenGlassMaterial;

        [Tooltip("Particle system, reproduced at the moment of destruction")]
        [SerializeField] private ParticleSystem _shardsParticles;

        [Tooltip("Material index if the mesh has multiple materials")]
        [SerializeField] protected int p_glassMaterialIndex;            
        
        protected Renderer p_renderer;
        protected Material[] p_materials;
        protected Material p_defaultGlassMaterial;

        private Action<AudioType> _onTakeDamageEvent;

        public ParticleSystem ShardsParticles => _shardsParticles;
        public Action<AudioType> SfxEvent => _onTakeDamageEvent;

        protected override void InitDamageObject()
        {
            if (!IsInited)
            {
                base.InitDamageObject();
                _shardsParticles.SetActive(false);
                p_renderer = GetComponent<Renderer>();
                if (p_renderer)
                {
                    p_materials = p_renderer.materials;
                    p_defaultGlassMaterial = p_materials[p_glassMaterialIndex];
                }
            }
        }

        public override void SetDamage(float damage)
        {
            base.SetDamage(damage);
            _onTakeDamageEvent?.Invoke(AudioType.SFX_GlassShards_Medium);
        }

        protected override void DoDeath()
        {
            base.DoDeath();
            if (p_renderer)
            {
                if (_brokenGlassMaterial)
                {
                    p_materials[p_glassMaterialIndex] = _brokenGlassMaterial;
                    p_renderer.materials = p_materials;
                }
                else
                {
                    p_renderer.enabled = false;
                }
            }

            if (_shardsParticles)
            {
                _shardsParticles.SetActive(true);
                _shardsParticles.Play();
            }

            _onTakeDamageEvent?.Invoke(AudioType.SFX_GlassShards_Heavy);
        }
    }
}

