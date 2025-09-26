using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Lamp : InteractiveObject
{
    [Header("Lamp Settings")]
    [SerializeField] private Renderer lampRenderer;
    [SerializeField] private Light bulbLight;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    private bool isOn = false;

    protected override void Awake()
    {
        base.Awake();
        UpdateVisual();
    }

    public override void SetPowered(bool state)
    {
        isOn = state;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (lampRenderer != null)
            lampRenderer.material = isOn ? onMaterial : offMaterial;

        if (bulbLight != null)
            bulbLight.enabled = isOn;
    }

    public override List<ContextMenuItem> GetContextMenuItems()
    {
        return new List<ContextMenuItem>
        {
            new ContextMenuItem { Label = "Переместить", Action = () => PlacementSystem.Instance.StartReposition(this) },
            new ContextMenuItem { Label = "Подключить", Action = () => CircuitManager.Instance.StartBuildFromContext(this)},
            new ContextMenuItem { Label = "Удалить", Action = () => RemoveObject() }
        };
    }

    public override string GetObjectID() => "Lamp";

}
