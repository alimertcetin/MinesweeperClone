using System;
using UnityEngine;
using XIV.Core.Utils;
using XIV.PoolSystem;
using XIV.TweenSystem;

namespace Minesweeper.GridSystems
{
    public class Cell : MonoBehaviour
    {
        bool isRevealing;
        Vector3 initialScale;

        public void Initialize()
        {
            initialScale = transform.localScale;
        }

        public bool Select()
        {
            if (isRevealing) return false;
            transform.CancelTween();
            
            transform.XIVTween()
                .Scale(transform.localScale, initialScale * 0.75f, 0.1f, EasingFunction.EaseInOutBounce)
                .Start();
            return true;
        }

        public bool Deselect()
        {
            if (isRevealing) return false;
            transform.CancelTween();
            
            transform.XIVTween()
                .Scale(transform.localScale, initialScale, 0.1f, EasingFunction.Linear)
                .Start();
            return true;
        }

        public void Explore(Material[] materials, Action onExplored = null)
        {
            transform.CancelTween();
            isRevealing = true;

            float durationPerMaterial = 0.25f / materials.Length;
            var renderer = GetComponent<Renderer>();
            EasingFunction.Function easeFunc = EasingFunction.EaseInOutElastic;
            var tween = renderer.XIVTween();
            tween.Scale(transform.localScale, initialScale * 1.5f, 0.25f, easeFunc, true)
                .And();
            for (int i = 0; i < materials.Length - 1; i++)
            {
                tween.AddTween(ChangeMaterialTween.Create(renderer, materials[i], materials[i + 1], durationPerMaterial, easeFunc));
            }

            tween.OnComplete(() =>
            {
                isRevealing = false;
                onExplored?.Invoke();
                transform.XIVTween()
                    .Scale(transform.localScale, initialScale, 0.1f, EasingFunction.Linear)
                    .Start();
            }).Start();
        }
    }

    public class ChangeMaterialTween : TweenDriver<Material, Renderer>
    {
        public static ChangeMaterialTween Create(Renderer renderer, Material startValue, Material endValue, float duration, EasingFunction.Function easingFunc)
        {
            return XIVPoolSystem.HasPool<ChangeMaterialTween>() ? 
                (ChangeMaterialTween)XIVPoolSystem.GetItem<ChangeMaterialTween>().Set(renderer, startValue, endValue, duration, easingFunc) :
                (ChangeMaterialTween)XIVPoolSystem.AddPool(new XIVPool<ChangeMaterialTween>(() => new ChangeMaterialTween())).GetItem().Set(renderer, startValue, endValue,duration, easingFunc);
        }
        
        protected override void OnComplete()
        {
            component.material = endValue;
            base.OnComplete();
        }

        protected override void OnUpdate(float easedTime)
        {
        }
    }
}