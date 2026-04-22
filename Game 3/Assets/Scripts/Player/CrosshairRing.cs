using UnityEngine;

public class CrosshairRing : MonoBehaviour
{
    // MUST match radius field in CrosshairController
    public float radius = 1.75f;

    // More segments = smoother circle, but 64 is likely plenty for this
    public int segments = 64;

    void Start()
    {
        LineRenderer lr = GetComponent<LineRenderer>();

        lr.loop = true;
        lr.positionCount = segments;
        lr.useWorldSpace = false; // Uses local space instead so it follows player

        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
    }
}
