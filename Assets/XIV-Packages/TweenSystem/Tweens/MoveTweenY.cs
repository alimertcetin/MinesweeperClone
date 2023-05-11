using UnityEngine;
using XIV.Core.Extensions;

namespace XIV.TweenSystem
{
    internal sealed class MoveTweenY : TweenDriver<float, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.position = component.position.SetY(Mathf.Lerp(startValue, endValue, normalizedEasedTime));
        }
    }
}