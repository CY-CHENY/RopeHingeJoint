using System.Collections.Generic;
using UnityEngine;

namespace BedRock.RunTime
{
    public class CircleOutline : ModifiedShadow
    {
        [SerializeField, Range(1, 2)] int m_circleCount = 2;
        [SerializeField, Range(1, 2)] int m_sampleIncrement = 2;
        int m_firstSample = 4;

        public int CircleCount
        {
            get
            {
                return m_circleCount;
            }

            set
            {
                m_circleCount = Mathf.Max(value, 1);
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        public int FirstSample
        {
            get
            {
                return m_firstSample;
            }

            set
            {
                m_firstSample = Mathf.Max(value, 2);
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        public int SampleIncrement
        {
            get
            {
                return m_sampleIncrement;
            }

            set
            {
                m_sampleIncrement = Mathf.Max(value, 1);
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        public override void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive())
                return;

            var total = (m_firstSample * 2 + m_sampleIncrement * (m_circleCount - 1)) * m_circleCount / 2;
            var neededCapacity = verts.Count * (total + 1);
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity;
            var original = verts.Count;
            var count = 0;
            var sampleCount = m_firstSample;
            var dx = effectDistance.x / CircleCount;
            var dy = effectDistance.y / CircleCount;
            for (int i = 1; i <= m_circleCount; i++)
            {
                var rx = dx * i;
                var ry = dy * i;
                var radStep = 2 * Mathf.PI / sampleCount;
                var rad = (i % 2) * radStep * 0.5f;
                for (int j = 0; j < sampleCount; j++)
                {
                    var next = count + original;
                    ApplyShadow(verts, effectColor, count, next, rx * Mathf.Cos(rad), ry * Mathf.Sin(rad));
                    count = next;
                    rad += radStep;
                }
                sampleCount += m_sampleIncrement;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            CircleCount = m_circleCount;
            FirstSample = m_firstSample;
            SampleIncrement = m_sampleIncrement;
        }

        protected override void Reset()
        {
            effectColor = Color.black;
            effectDistance = new Vector2(2, -2);
        }
#endif
    }
}

