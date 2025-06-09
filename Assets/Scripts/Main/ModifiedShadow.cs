using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BedRock.RunTime
{
    public abstract class ModifiedShadow : Shadow
    {
        List<UIVertex> list = new List<UIVertex>();
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            list.Clear();
            vh.GetUIVertexStream(list);

            ModifyVertices(list);

            vh.Clear();

            vh.AddUIVertexTriangleStream(list);
        }

        public abstract void ModifyVertices(List<UIVertex> verts);
    }
}

