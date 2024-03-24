using UnityEngine;

public class PositionWrapper : MonoBehaviour
{
    [SerializeField]
    Vector2 wrapRange = new Vector2(350f, 500f);

    private void FixedUpdate()
    {
        if (transform.position.x < wrapRange.x)
        {
            transform.SetPositionX(wrapRange.y - (wrapRange.x - transform.position.x));
        }

        if (transform.position.x > wrapRange.y)
        {
            transform.SetPositionX(wrapRange.x + (transform.position.x - wrapRange.y));
        }
    }
}
