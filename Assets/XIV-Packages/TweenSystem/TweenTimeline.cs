using UnityEngine;
using XIV.Core.Collections;
using XIV.PoolSystem;

namespace XIV.TweenSystem
{
    public sealed class TweenTimeline : IPoolable
    {
        public DynamicArray<ITween> tweens;
        IPool pool;

        public static TweenTimeline GetTimeline(params ITween[] tweens)
        {
            var timeline = XIVPoolSystem.HasPool<TweenTimeline>() ? XIVPoolSystem.GetItem<TweenTimeline>() : XIVPoolSystem.AddPool(new XIVPool<TweenTimeline>(() => new TweenTimeline())).GetItem();

            int length = tweens.Length;
            for (int i = 0; i < length; i++)
            {
                timeline.tweens.Add() = tweens[i];
            }
            
            return timeline;
        }

        TweenTimeline()
        {
            tweens = new DynamicArray<ITween>(2);
        }

        public void Update()
        {
            int count = tweens.Count;
            for (int i = 0; i < count; i++)
            {
                ref var tween = ref tweens[i];
                tween.Update(Time.deltaTime);
                
                if (tween.IsDone() == false) continue;
                tween.Complete();
                tweens.RemoveAt(i);
                i -= 1;
                count -= 1;
            }
        }

        public bool IsDone()
        {
            for (var i = 0; i < tweens.Count; i++)
            {
                if (tweens[i].IsDone() == false) return false;
            }

            return true;
        }

        public void Cancel()
        {
            int count = tweens.Count;
            for (var i = 0; i < count; i++)
            {
                tweens[i].Cancel();
            }
        }

        public void Return()
        {
            pool.Return(this);
        }

        void IPoolable.OnPoolCreate(IPool pool)
        {
            this.pool = pool;
        }

        void IPoolable.OnPoolReturn()
        {
            tweens.Clear();
        }
    }
}