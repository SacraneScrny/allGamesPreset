using Sackrany.Utils.Pool.Abstracts;
using Sackrany.Utils.Pool.Extensions;

using UnityEngine;

namespace Sackrany.Utils.Pool.Components
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPoolComponent : MonoBehaviour, IPoolable
    {
        private AudioSource audioSource;
        private float _clipTime;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            _clipTime = audioSource.clip.length;
        }

        private bool isActive;
        private float timer;
        private void Update()
        {
            if (!isActive) return;
            timer += Time.deltaTime;
            if (timer >= _clipTime)
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
            audioSource.Play();
        }
        public void OnReleased()
        {
            audioSource.Stop();
            gameObject.SetActive(false);
        }
    }
}