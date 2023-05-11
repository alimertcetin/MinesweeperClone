using UnityEngine;
using XIV.Core.Extensions;

namespace XIV.TweenSystem
{
    internal sealed class MoveTweenZ : TweenDriver<float, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.position = component.position.SetZ(Mathf.Lerp(startValue, endValue, normalizedEasedTime));
        }
    }
}