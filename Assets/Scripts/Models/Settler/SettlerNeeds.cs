using UnityEngine;

namespace Models.Settler
{
    public class SettlerNeeds
    {
        public Need Rest = new Need { DefaultDecayTime = 60f, RestoreFactor = 10f };
        public Need Entertainment = new Need { DefaultDecayTime = 120f, RestoreFactor = 8f };
        public Need Health = new Need { DefaultDecayTime = 200f, RestoreFactor = 15f };
        public Need Pray = new Need { DefaultDecayTime = 150f, RestoreFactor = 12f };

        public SettlerNeeds()
        {
            Rest.Value = 0.0f;
            Entertainment.Value = 1.0f;
            Health.Value = 1.0f;
            Pray.Value = 1.0f;
        }

        public void UpdateNeeds()
        {
            Rest.Update();
            Entertainment.Update();
            Health.Update();
            Pray.Update();
        }
    }

    public class Need
    {
        public float Value;
        public float DefaultDecayTime;
        public bool IsRestoring;
        public float RestoreFactor;

        public void Update()
        {
            float delta = Time.deltaTime / (IsRestoring ? RestoreFactor : DefaultDecayTime);
            Value += IsRestoring ? delta : -delta;
            Value = Mathf.Max(Value, 0f);
        }
    }
}