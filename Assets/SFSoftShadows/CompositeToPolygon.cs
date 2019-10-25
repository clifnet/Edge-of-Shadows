using UnityEngine;
using System.Collections;

public class CompositeToPolygon : MonoBehaviour
{
    public CompositeCollider2D compositeCollider;

    [ContextMenu("Create")]
    public void Create()
    {
        if(compositeCollider != null)
        {
            for (int i = 0; i < compositeCollider.pathCount; i++)
            {
                GameObject go = new GameObject("Polygon");
                SFPolygon sfp = go.AddComponent<SFPolygon>();
                go.transform.parent = transform;

                sfp.looped = true;

                Vector2[] points = new Vector2[compositeCollider.GetPathPointCount(i)];
                compositeCollider.GetPath(i, points);
                sfp.verts = points;
                sfp._FlipInsideOut();          
            }
        }
    }

    private void Start()
    {
        Create();
    }
}
