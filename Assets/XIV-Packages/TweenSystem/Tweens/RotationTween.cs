using UnityEngine;

namespace XIV.TweenSystem
{
    internal sealed class RotationTween : TweenDriver<Quaternion, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.rotation = Quaternion.Lerp(startValue, endValue, normalizedEasedTime);
        }
    }
}