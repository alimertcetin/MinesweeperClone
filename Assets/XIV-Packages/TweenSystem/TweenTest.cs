using System;
using System.Collections.Generic;
using UnityEngine;
using XIV.Core;
using XIV.Core.Utils;
using XIV.Core.XIVMath;

namespace XIV.TweenSystem
{
    public class TweenTest : MonoBehaviour
    {
        public Transform prefab;
        public Vector3 startScale;
        public Vector3 targetScale;
        public float duration;
        public EasingFunction.Ease easingFunc;

        public int testCount;
        
        List<GameObject> goTestList;

        void Awake()
        {
            goTestList = new List<GameObject>();
            var scale = prefab.transform.localScale;
            int horizontal = 5;
            var pos = transform.position;
            for (int i = 0; i < testCount; i++)
            {
                var newGo = Instantiate(prefab.gameObject);
                newGo.transform.position = pos;
                goTestList.Add(newGo);
                pos.x += scale.x;
                
                horizontal--;
                if (horizontal == 0)
                {
                    pos.y += scale.y;
                    pos.x = transform.position.x;
                    horizontal = 5;
                }
            }
        }

        void Update()
        {
            for (int i = 0; i < goTestList.Count; i++)
            {
                if (goTestList[i].transform.HasTween())
                {
                    return;
                }
            }

            var easing = EasingFunction.GetEasingFunction(easingFunc);
            
            for (int i = 0; i < goTestList.Count; i++)
            {
                GameObject go = goTestList[i];
                var testTransform = go.transform;
                if (testTransform.HasTween())
                {
                    testTransform.CancelTween();
                }

                // testTransform.position -= Vector3.up * 5f;
                
                Vector3[] points = BezierMath.CreateCurve(testTransform.position, testTransform.position + Vector3.up * 5f);
                testTransform.XIVTween()
                    .FollowCurve(points, duration, easing, true)
                    .And()
                    .Scale(testTransform.localScale, Vector3.one * 0.25f, duration * 0.5f, easing, true, loopCount:1)
                    .Start();
                
                XIVDebug.DrawSpline(points, Color.red, 10, duration);
                // XIVDebug.DrawBezier(points[0], points[1], points[2], points[3], Color.red, duration);

                // Test1(testTransform, easing);
                // Test2(testTransform, easing);
                // Test3(testTransform, easing);
                // Test4(testTransform, easing);
            }
        }

        void Test1(Transform testTransform, EasingFunction.Function easing)
        {
            testTransform.XIVTween()
                .ScaleZ(startScale.z, targetScale.z, duration, easing)
                .OnComplete(() => Debug.Log("ScaleZ finished"))
                .Start();
            testTransform.XIVTween()
                .Move(testTransform.position, testTransform.position + Vector3.up * 5f, duration * 2f, easing)
                .OnComplete(() => Debug.Log("MoveTo Finished"))
                .OnCanceled(Cancel)
                .Start();
        }

        void Test2(Transform testTransform, EasingFunction.Function easing)
        {
            testTransform.XIVTween()
                .ScaleZ(startScale.z, targetScale.z, duration, easing)
                .And()
                .Move(testTransform.position, testTransform.position + Vector3.up * 5f, duration * 2f, easing)
                .OnComplete(() => Debug.Log("Both ScaleZ and Move tweens are completed"))
                .OnCanceled(Cancel)
                .Start();
        }

        void Test3(Transform testTransform, EasingFunction.Function easing)
        {
            testTransform.XIVTween()
                .Scale(startScale, targetScale, duration, easing, true)
                .And()
                // .Move(testTransform.position, testTransform.position + Vector3.up * 5f, duration * 2f, easing)
                .AddTween(new MoveTowardsTween(testTransform, testTransform.position + Vector3.up * 5f, 1f))
                .And()
                .AddTween(new ChangeColorTween().Set(testTransform.GetComponent<Renderer>(), Color.white, Color.red, duration, easing))
                .OnComplete(() => Debug.Log("Both ScaleZ and Move tweens are completed"))
                .OnCanceled(Cancel)
                .Start();
            
            
        }

        void Test4(Transform testTransform, EasingFunction.Function easing)
        {
            testTransform.XIVTween()
                .ScaleZ(startScale.z, targetScale.z, duration / 4f, easing, true, 4)
                .And()
                .Move(testTransform.position, testTransform.position + Vector3.up * 5f, duration, easing, true, loopCount: 2)
                .And()
                .AddTween(new ChangeColorTween().Set(testTransform.GetComponent<Renderer>(), Color.white, Color.red, duration, easing))
                .OnComplete(() => Debug.Log("Both ScaleZ and Move tweens are completed"))
                .OnCanceled(Cancel)
                .Start();
        }

        void Cancel()
        {
            var scale = prefab.transform.localScale;
            int horizontal = 5;
            var pos = transform.position;
            for (int i = 0; i < testCount; i++)
            {
                goTestList[i].transform.position = pos;
                goTestList[i].transform.localScale = Vector3.one;
                pos.x += scale.x;

                horizontal--;
                if (horizontal == 0)
                {
                    pos.y += scale.y;
                    pos.x = transform.position.x;
                    horizontal = 5;
                }
            }
        }

        [ContextMenu(nameof(CancelTween))]
        void CancelTween()
        {
            foreach (GameObject go in goTestList)
            {
                go.transform.CancelTween();
            }
        }
    }

    // How to create custom tweens using ITween
    public class MoveTowardsTween : ITween
    {
        Transform transform;
        Vector3 target;
        float speed;
        
        public MoveTowardsTween(Transform transform, Vector3 target, float speed)
        {
            this.transform = transform;
            this.target = target;
            this.speed = speed;
        }
        
        void ITween.Update(float deltaTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * deltaTime);
        }

        bool ITween.IsDone()
        {
            return Vector3.Distance(transform.position, target) < Mathf.Epsilon;
        }

        void ITween.Complete()
        {
            transform = default;
        }

        void ITween.Cancel()
        {
            transform = default;
        }
    }

    // How to create custom tweens using TweenDriver<T, T1>
    public class ChangeColorTween : TweenDriver<Color, Renderer>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            var color = Color.Lerp(startValue, endValue, normalizedEasedTime);
            component.material.color = color;
        }
    }
}