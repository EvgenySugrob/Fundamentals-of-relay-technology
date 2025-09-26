using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class Battery : InteractiveObject
{
    [Header("Battery Setting")]
    [SerializeField] private float voltage = 12f;
    [SerializeField] private Transform positiveTerminal;
    [SerializeField] private Transform negativeTerminal;
    [SerializeField] private BatteryPin positivePin;
    [SerializeField] private BatteryPin negativePin;

    public float Voltage => voltage;
    public Transform PositiveTerminal => positiveTerminal;
    public Transform NegativeTerminal => negativeTerminal;
    public BatteryPin PositivePin => positivePin;
    public BatteryPin NegativePin => negativePin;

    protected override void Awake()
    {
        base.Awake();
        positivePin.Initialize(this, true);
        negativePin.Initialize(this, false);
    }

    public float GetVoltage(Transform terminal)
    {
        if (terminal == positiveTerminal)
            return voltage;
        if (terminal == negativeTerminal)
            return 0f;

        return 0f;
    }

    public override List<ContextMenuItem> GetContextMenuItems()
    {
        return new List<ContextMenuItem>
        {
            new ContextMenuItem {Label = "Переместить", Action = () => PlacementSystem.Instance.StartReposition(this)},
            new ContextMenuItem {Label = "Удалить", Action = () => Destroy(gameObject)}
        };
    }

    public override string GetObjectID()
    {
        return gameObject.name;
    }

    private void OnDrawGizmos()   
    {
        if (positiveTerminal != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(positiveTerminal.position, 0.005f);
        }

        if (negativeTerminal != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(negativeTerminal.position, 0.005f);
        }
    }
}
