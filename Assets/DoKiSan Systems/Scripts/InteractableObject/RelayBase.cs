using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RelayParameter
{
    public string Name;
    public float Value;
    public float MinValue;
    public float MaxValue;
    public System.Action<float> OnValueChanged;
}

public abstract class RelayBase : InteractiveObject
{
    [SerializeField] protected bool hasSetting = false;
    protected bool isEnergizer = false;

    public virtual void Energize()
    {
        isEnergizer = true;
        OnEnergizer();
    }

    public virtual void DeEnergizer()
    {
        isEnergizer = false;
        OnDeEnergizer();
    }

    protected abstract void OnEnergizer();
    protected abstract void OnDeEnergizer();

    public virtual List<RelayParameter> GetParameters()
    {
        return new List<RelayParameter>();
    }

    public override void OnInteract()
    {
        if (CircuitManager.Instance.Mode == CircuitBuildMode.Normal && hasSetting)
        {
            RelaySettingsUI.Instance.Open(this);
        }
        else
        {
            base.OnInteract();
        }
    }

    public override List<ContextMenuItem> GetContextMenuItems()
    {
        var items = new List<ContextMenuItem>()
        {
            new ContextMenuItem {Label = "Переместить", Action = () => PlacementSystem.Instance.StartReposition(this)},
            new ContextMenuItem {Label = "Подключить", Action = () => CircuitManager.Instance.StartBuildFromContext(this)},
            new ContextMenuItem {Label = "Удалить", Action = () => Destroy(gameObject)}
        };

        if (hasSetting)
            items.Insert(0, new ContextMenuItem { Label = "Настройки", Action = () => RelaySettingsUI.Instance.Open(this) });

        return items;
    }
}
