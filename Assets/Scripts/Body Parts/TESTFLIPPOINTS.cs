using UnityEngine;
using WeaverCore.Utilities;

[ExecuteAlways]
public class TESTFLIPPOINTS : MonoBehaviour
{
    bool flipped = false;

    [SerializeField]
    PolygonCollider2D c;

    private void Update()
    {
        if (!flipped && c != null)
        {
            var points = c.points;

            for (int i = points.Length - 1; i >= 0; i--)
            {
                points[i] = points[i].With(x: -points[i].x);
            }
            c.points = points;
            flipped = true;
        }
    }
}
