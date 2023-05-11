using UnityEngine;
using XIV.Core.Extensions;

namespace XIV.TweenSystem
{
    internal sealed class MoveTweenX : TweenDriver<float, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.position = component.position.SetX(Mathf.Lerp(startValue, endValue, normalizedEasedTime));
        }
    }
}