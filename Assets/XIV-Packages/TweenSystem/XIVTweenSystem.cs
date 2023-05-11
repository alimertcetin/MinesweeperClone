using System.Collections.Generic;
using UnityEngine;
using XIV.Core.Collections;
using XIV.PoolSystem;

namespace XIV.TweenSystem
{
    internal static class XIVTweenSystem
    {
        class TweenData : IPoolable
        {
            public int instanceID;
            public DynamicArray<TweenTimeline> timelines;
            IPool pool;

            public TweenData()
            {
                timelines = new DynamicArray<TweenTimeline>(2);
            }

            public void Return()
            {
                pool.Return(this);
            }

            void IPoolable.OnPoolCreate(IPool pool)
            {
                this.pool = pool;
            }

            void IPoolable.OnPoolReturn()
            {
                instanceID = -1;
                for (var i = 0; i < timelines.Count; i++)
                {
                    timelines[i].Return();
                }

                timelines.Clear();
            }
        }
        
        class TweenHelperMono : MonoBehaviour
        {
            internal List<TweenData> tweenDatas = new List<TweenData>();
        
            static TweenHelperMono instance;
            public static TweenHelperMono Instance => instance == null ? instance = new GameObject("XIV-TweenSystem-Helper").AddComponent<TweenHelperMono>() : instance;

            void Update()
            {
                int count = tweenDatas.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    TweenData tweenData = tweenDatas[i];
                    TweenTimeline timeline = tweenData.timelines[0];
                    timeline.Update();
                    if (timeline.IsDone())
                    {
                        tweenData.timelines.RemoveAt(0);
                        timeline.Return();
                    }

                    if (tweenData.timelines.Count == 0)
                    {
                        tweenDatas.RemoveAt(i);
                        tweenLookup.Remove(tweenData.instanceID);
                        tweenData.Return();
                    }
                }
            }
        }

        static HashSet<int> tweenLookup = new HashSet<int>();

        [UnityEngine.RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            tweenLookup.Clear();
        }

        internal static void AddTween(int instanceID, TweenTimeline tween)
        {
            var tweenData = GetTweenData(instanceID);
            tweenData.timelines.Add() = tween;
        }

        internal static void CancelTween(int instanceID)
        {
            if (tweenLookup.Contains(instanceID) == false) return;
            tweenLookup.Remove(instanceID);

            int index = IndexOfTweenData(instanceID);
            var tweenDatas = TweenHelperMono.Instance.tweenDatas;
            var tweenData = tweenDatas[index];
            tweenDatas.RemoveAt(index);
            tweenData.Return();

            int count = tweenData.timelines.Count;
            for (int i = 0; i < count; i++)
            {
                tweenData.timelines[i].Cancel();
            }
        }

        internal static bool HasTween(int instanceID)
        {
            return tweenLookup.Contains(instanceID);
        }

        static TweenData GetTweenData(int instanceID)
        {
            if (tweenLookup.Contains(instanceID))
            {
                return TweenHelperMono.Instance.tweenDatas[IndexOfTweenData(instanceID)];
            }

            var tweenData = XIVPoolSystem.HasPool<TweenData>() ? XIVPoolSystem.GetItem<TweenData>() : XIVPoolSystem.AddPool(new XIVPool<TweenData>(() => new TweenData())).GetItem();
            tweenData.instanceID = instanceID;
            tweenLookup.Add(instanceID);
            TweenHelperMono.Instance.tweenDatas.Add(tweenData);
            return tweenData;
        }

        static int IndexOfTweenData(int instanceID)
        {
            var tweenDatas = TweenHelperMono.Instance.tweenDatas;
            int count = tweenDatas.Count;
            for (int i = 0; i < count; i++)
            {
                if (tweenDatas[i].instanceID == instanceID) return i;
            }

            return -1;
        }
    }
}
