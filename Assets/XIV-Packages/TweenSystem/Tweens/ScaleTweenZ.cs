using UnityEngine;
using XIV.Core.Extensions;

namespace XIV.TweenSystem
{
    internal sealed class ScaleTweenZ : TweenDriver<float, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.localScale = component.localScale.SetZ(Mathf.Lerp(startValue, endValue, normalizedEasedTime));
        }
    }
}