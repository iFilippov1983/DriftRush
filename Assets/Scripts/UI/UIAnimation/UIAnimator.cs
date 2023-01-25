﻿using RaceManager.Root;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    public class UIAnimator : MonoBehaviour, IAgentsHelper
    {
        [SerializeField] private bool useAnimation;
        [SerializeField]
        [ShowIf("useAnimation")]
        private List<GUIAnimFREE> _animations = new List<GUIAnimFREE>();

        [SerializeField] private bool useParticles;
        [ShowIf("useParticles")]
        [SerializeField] private ParticleSystem _particles;


        public void Activate()
        {
            if (_animations.Count > 0)
                foreach (var anim in _animations)
                    anim.enabled = true;

            if (_particles != null)
                _particles.Play();
        }

        public void Deactivate()
        {
            if (_animations.Count > 0)
                foreach (var anim in _animations)
                    anim.enabled = false;

            if (_particles != null)
                _particles.Stop();
        }

        public void Destroy()
        {
            if (_animations.Count > 0)
                foreach (var anim in _animations)
                    anim.enabled = false;

            if (_particles != null)
                _particles.Stop();
        }
    }
}
