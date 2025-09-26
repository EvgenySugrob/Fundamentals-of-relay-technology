using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Wire : MonoBehaviour
{
private InteractiveObject a;
    private InteractiveObject b;
    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    public void Initialize(InteractiveObject objA, InteractiveObject objB)
    {
        a = objA;
        b = objB;
        lr = GetComponent<LineRenderer>();

        // если нужно — можно установить material/width здесь
        // lr.material = someMaterial;
        // lr.startWidth = lr.endWidth = 0.02f;
    }

    private void Update()
    {
        if (lr == null) return;

        if (a == null || b == null)
        {
            // один из объектов удалён — самоуничтожаемся
            DestroySelf();
            return;
        }

        lr.SetPosition(0, a.transform.position);
        lr.SetPosition(1, b.transform.position);
    }

    public void DestroySelf()
    {
        if (gameObject != null) Destroy(gameObject);
    }
}
