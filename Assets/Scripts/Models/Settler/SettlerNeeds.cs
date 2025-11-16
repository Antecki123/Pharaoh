using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Models.Settler
{
    public class SettlerNeeds
    {
        public Need Rest = new Need { DefaultDecayTime = 10f, RestoreFactor = 5f };
        public Need Entertainment = new Need { DefaultDecayTime = 30f, RestoreFactor = 8f };
        public Need Health = new Need { DefaultDecayTime = 80f, RestoreFactor = 15f };
        public Need Pray = new Need { DefaultDecayTime = 50f, RestoreFactor = 5f };

        public SettlerNeeds()
        {
            Rest.Value = 1.0f;
            Entertainment.Value = 1.0f;
            Health.Value = 1.0f;
            Pray.Value = 1.0f;
        }

        public class Need
        {
            public float Value;
            public float DefaultDecayTime;
            public float RestoreFactor;
            public bool IsRestoring;

            public void Update()
            {
                float delta = Time.deltaTime / (IsRestoring ? RestoreFactor : DefaultDecayTime);
                Value += IsRestoring ? delta : -delta;
                Value = Mathf.Max(Value, 0f);
            }
        }
    }

    public struct SettlerNeedsData
    {
        public NeedData RestData;
        public NeedData EntertainmentData;
        public NeedData HealthData;
        public NeedData PrayData;

        public void Update(float deltaTime)
        {
            RestData.Update(deltaTime);
            EntertainmentData.Update(deltaTime);
            HealthData.Update(deltaTime);
            PrayData.Update(deltaTime);
        }

        public struct NeedData
        {
            public float Value;
            public float DefaultDecayTime;
            public float RestoreFactor;
            public bool IsRestoring;

            public NeedData(float Value, float DefaultDecayTime, float RestoreFactor, bool IsRestoring)
            {
                this.Value = Value;
                this.DefaultDecayTime = DefaultDecayTime;
                this.RestoreFactor = RestoreFactor;
                this.IsRestoring = IsRestoring;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(float deltaTime)
            {
                float delta = deltaTime / (IsRestoring ? RestoreFactor : DefaultDecayTime);
                Value = math.max(0f, Value + (IsRestoring ? delta : -delta));
            }
        }
    }

    [BurstCompile]
    public struct NeedsUpdateJob : IJobParallelFor
    {
        public NativeArray<SettlerNeedsData> NeedsDataArray;
        [ReadOnly] public float DeltaTime;

        public void Execute(int index)
        {
            var data = NeedsDataArray[index];
            data.Update(DeltaTime);
            NeedsDataArray[index] = data;
        }
    }
}