public class CircuitConnection
{
 public InteractiveObject A { get; private set; }
    public InteractiveObject B { get; private set; }
    public Wire Wire { get; private set; }

    public CircuitConnection(InteractiveObject a, InteractiveObject b, Wire wire)
    {
        A = a;
        B = b;
        Wire = wire;
    }

    public bool Contains(InteractiveObject a, InteractiveObject b)
    {
        return (A == a && B == b) || (A == b && B == a);
    }

    public bool Contains(InteractiveObject obj)
    {
        return A == obj || B == obj;
    }

    public InteractiveObject GetOther(InteractiveObject obj)
    {
        if (obj == A) return B;
        if (obj == B) return A;
        return obj;
    }

    public void Destroy()
    {
        if (Wire != null)
        {
            Wire.DestroySelf();
            Wire = null;
        }
    }
}
