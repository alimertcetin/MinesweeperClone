using UnityEngine;

namespace XIV.TweenSystem
{
    internal sealed class RotationTweenX : TweenDriver<float, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.rotation = Quaternion.AngleAxis(Mathf.Lerp(startValue, endValue, normalizedEasedTime), Vector3.right);
        }
    }
}