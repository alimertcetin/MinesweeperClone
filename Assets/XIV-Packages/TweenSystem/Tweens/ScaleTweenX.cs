using UnityEngine;
using XIV.Core.Extensions;

namespace XIV.TweenSystem
{
    internal sealed class ScaleTweenX : TweenDriver<float, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.localScale = component.localScale.SetX(Mathf.Lerp(startValue, endValue, normalizedEasedTime));
        }
    }
}