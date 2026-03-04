using Sackrany.Utils.Pool.Abstracts;
using Sackrany.Utils.Pool.Extensions;

using UnityEngine;

namespace Sackrany.Utils.Pool.Components
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticlesPoolComponent : MonoBehaviour, IPoolable
    {
        private new ParticleSystem particleSystem;
        private float _time;
        
        private void Awake()
        {
            particleSystem = GetComponent<ParticleSystem>();
            _time = particleSystem.main.duration;
        }

        private bool isActive;
        private float timer;
        private void Update()
        {
            if (!isActive) return;
            timer += Time.deltaTime;
            if (timer >= _time)
            {
                gameObject.RELEASE();
                isActive = false;
            }
        }
        
        public void OnPooled()
        {
            isActive = true;
            timer = 0;
            gameObject.SetActive(true);
            particleSystem.Play();
        }
        public void OnReleased()
        {
            particleSystem.Stop();
            gameObject.SetActive(false);
        }
    }
}