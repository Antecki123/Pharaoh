using Controllers.Work;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Environment
{
    public class InfluenceMap
    {
        public event Action OnIrrigationInfluenceChanged;
        public event Action OnFireRiskInfluenceChanged;
        public event Action OnAestheticsInfluenceChanged;
        public event Action OnCriminalInfluenceChanged;

        public IReadOnlyDictionary<Vector2Int, float> IrrigationInfluence => irrigationInfluence;
        public IReadOnlyDictionary<Vector2Int, float> FireRiskInfluence => fireRiskInfluence;
        public IReadOnlyDictionary<Vector2Int, float> AestheticsInfluence => aestheticsInfluence;
        public IReadOnlyDictionary<Vector2Int, float> CriminalInfluence => criminalInfluence;

        private readonly Dictionary<Vector2Int, float> irrigationInfluence = new();
        private readonly Dictionary<Vector2Int, float> fireRiskInfluence = new();
        private readonly Dictionary<Vector2Int, float> aestheticsInfluence = new();
        private readonly Dictionary<Vector2Int, float> criminalInfluence = new();

        private readonly Dictionary<InfluenceType, Dictionary<Vector2Int, float>> influenceMaps;
        private readonly Dictionary<InfluenceType, Action> influenceEvents;

        public InfluenceMap()
        {
            influenceMaps = new Dictionary<InfluenceType, Dictionary<Vector2Int, float>>
            {
                { InfluenceType.Irrigation, irrigationInfluence },
                { InfluenceType.FireRisk,   fireRiskInfluence   },
                { InfluenceType.Aesthetics, aestheticsInfluence },
                { InfluenceType.Criminal,   criminalInfluence   },
            };

            influenceEvents = new Dictionary<InfluenceType, Action>
            {
                { InfluenceType.Irrigation, () => OnIrrigationInfluenceChanged?.Invoke() },
                { InfluenceType.FireRisk,   () => OnFireRiskInfluenceChanged?.Invoke()   },
                { InfluenceType.Aesthetics, () => OnAestheticsInfluenceChanged?.Invoke() },
                { InfluenceType.Criminal,   () => OnCriminalInfluenceChanged?.Invoke()   },
            };
        }

        public void RegisterInfluenceSource(Vector2 center, InfluenceData data)
            => ApplyInfluence(center, data, add: true);

        public void UnregisterInfluenceSource(Vector2 center, InfluenceData data)
            => ApplyInfluence(center, data, add: false);

        private void ApplyInfluence(Vector2 center, InfluenceData data, bool add)
        {
            if (data.InfluenceType == InfluenceType.None)
            {
                Debug.LogError($"[InfluenceMap] InfluenceType.None – typ nie zosta³ przypisany.");
                return;
            }

            if (!influenceMaps.TryGetValue(data.InfluenceType, out var map) ||
                !influenceEvents.TryGetValue(data.InfluenceType, out var fireEvent))
            {
                Debug.LogError($"[InfluenceMap] Nieobs³ugiwany InfluenceType: {data.InfluenceType}.");
                return;
            }

            ModifyInfluence(map, center, data.InfluenceRange, data.InfluenceValue, add);
            fireEvent.Invoke();
        }

        private static void ModifyInfluence(Dictionary<Vector2Int, float> influence,
            Vector2 center, float range, float power, bool add)
        {
            int minX = Mathf.FloorToInt(center.x - range);
            int maxX = Mathf.CeilToInt(center.x + range);
            int minY = Mathf.FloorToInt(center.y - range);
            int maxY = Mathf.CeilToInt(center.y + range);

            float rangeSq = range * range;
            float delta = add ? power : -power;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;

                    if (dx * dx + dy * dy > rangeSq)
                        continue;

                    var cell = new Vector2Int(x, y);

                    if (influence.TryGetValue(cell, out float current))
                    {
                        float next = current + delta;

                        if (next <= 0f)
                            influence.Remove(cell);
                        else
                            influence[cell] = next;
                    }
                    else if (add)
                    {
                        influence[cell] = power;
                    }
                }
            }
        }
    }

    public enum InfluenceType
    {
        None,
        Irrigation,
        FireRisk,
        Aesthetics,
        Criminal
    }
}