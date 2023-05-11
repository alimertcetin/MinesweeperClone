using System;
using UnityEngine;
using XIV.Core.Utils;
using XIV.PoolSystem;

namespace XIV.TweenSystem
{
    public abstract class TweenDriver<TValueType, TComponentType> : TweenDriver<TValueType> where TComponentType : Component
    {
        internal TComponentType component { get; private set; }

        public TweenDriver<TValueType, TComponentType> Set(TComponentType component, TValueType startValue, TValueType endValue, float duration, EasingFunction.Function easingFunction, bool isPingPong = false, int loopCount = 0)
        {
            base.Set(startValue, endValue, duration, easingFunction, isPingPong, loopCount);
            this.component = component;
            return this;
        }

        protected override void OnComplete()
        {
            component = default;
        }

        protected override void OnCancel()
        {
            component = default;
        }
    }

    public abstract class TweenDriver<TValueType> : ITween, IPoolable
    {
        protected TValueType startValue;
        protected TValueType endValue;
        protected EasingFunction.Function easingFunction;
        protected Timer timer;
        IPool pool;
        bool hasPool;
        bool isPingPong;
        bool reversed;
        int loopCount;

        public TweenDriver<TValueType> Set(TValueType startValue, TValueType endValue, float duration, EasingFunction.Function easingFunction, bool isPingPong = false, int loopCount = 0)
        {
            Clear();
            this.startValue = startValue;
            this.endValue = endValue;
            this.easingFunction = easingFunction;
            this.timer = new Timer(duration);
            this.isPingPong = isPingPong;
            this.loopCount = loopCount;
            return this;
        }

        protected abstract void OnUpdate(float easedTime);
        protected abstract void OnComplete();
        protected abstract void OnCancel();
        
        void Clear()
        {
            startValue = default;
            endValue = default;
            timer = default;
            reversed = false;
        }

        void ITween.Update(float deltaTime)
        {
            timer.Update(deltaTime);
            if (timer.NormalizedTime > 0.5f && reversed == false && isPingPong)
            {
                Reverse();
            }

            var easedTime = easingFunction.Invoke(0f, 1f, isPingPong ? timer.NormalizedTimePingPong : timer.NormalizedTime);
            OnUpdate(easedTime);
            if (timer.IsDone == false) return;
            
            if (loopCount > 0)
            {
                timer.Restart();
            }
            loopCount--;
            if (isPingPong)
            {
                Reverse();
            }
        }

        void Reverse()
        {
            (startValue, endValue) = (endValue, startValue);
            reversed = !reversed;
        }

        bool ITween.IsDone() => timer.IsDone && loopCount <= 0;

        void ITween.Complete()
        {
            OnComplete();
            if (hasPool) pool.Return(this);
        }

        void ITween.Cancel()
        {
            OnCancel();
            if (hasPool) pool.Return(this);
        }

        void IPoolable.OnPoolCreate(IPool pool)
        {
            this.pool = pool;
            this.hasPool = pool != default;
        }

        void IPoolable.OnPoolReturn()
        {
            Clear();
        }
    }
}