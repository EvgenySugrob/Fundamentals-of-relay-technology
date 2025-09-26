using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryPin : InteractiveObject
{
    public Battery ParentBattery { get; private set; }
    public bool IsPositive { get; private set; }

    public void Initialize(Battery parent, bool isPositive)
    {
        ParentBattery = parent;
        IsPositive = isPositive;
    }

    public override List<ContextMenuItem> GetContextMenuItems()
    {
        return new List<ContextMenuItem>
        {
            new ContextMenuItem{Label = "Подключить", Action = () => CircuitManager.Instance.StartBuildFromContext(this) }
        };
    }

    public override string GetObjectID()
    {
        return gameObject.name;
    }
}
