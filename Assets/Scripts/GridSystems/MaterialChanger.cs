using System;
using UnityEngine;

namespace GridSystems
{
    public class MaterialChanger : MonoBehaviour
    {
        public event Action<GameObject> OnDestroyed;
        public float duration;
        public Material[] materials;
        float currentSeconds;
        int currentIndex;
        Renderer renderer;
        bool isInitialized;

        void Update()
        {
            if (isInitialized == false)
            {
                isInitialized = true;
                renderer = GetComponent<Renderer>();
                renderer.material = materials[currentIndex++];
                if (currentIndex == materials.Length)
                {
                    // TODO : Pool?
                    OnDestroyed?.Invoke(this.gameObject);
                    Destroy(this);
                    return;
                }
            }
            
            currentSeconds += Time.deltaTime;
            if (currentSeconds < duration) return;
            
            currentSeconds = 0f;
            renderer.material = materials[currentIndex++];
            if (currentIndex == materials.Length)
            {
                // TODO : Pool?
                OnDestroyed?.Invoke(this.gameObject);
                Destroy(this);
            }
        }
    }
}