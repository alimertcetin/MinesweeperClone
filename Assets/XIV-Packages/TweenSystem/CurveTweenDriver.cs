using System;
using UnityEngine;
using XIV.Core.Utils;

namespace XIV.TweenSystem
{
    internal abstract class CurveTweenDriver<TValueType, TComponentType> : TweenDriver<TValueType[], TComponentType> where TComponentType : Component
    {
        internal CurveTweenDriver<TValueType, TComponentType> Set(TComponentType component, TValueType[] values, float duration, EasingFunction.Function easing, bool isPingPong = false, int loopCount = 0)
        {
            int length = values.Length;
            var reversedValues = new TValueType[length];
            Array.Copy(values, reversedValues, length);
            Array.Reverse(reversedValues);
            base.Set(component, values, reversedValues, duration, easing, isPingPong, loopCount);
            return this;
        }
    }
}