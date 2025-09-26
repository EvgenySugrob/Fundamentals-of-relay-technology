using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : InteractiveObject
{
    [Header("Switch part param")]
    [SerializeField] Transform switchTransform;
    [SerializeField] Vector3 eulerAngleSwitch;

    private Quaternion startRotate;
    private Quaternion endRotate;
    private bool isOn = false;

    public bool IsOn => isOn;

    void Start()
    {
        startRotate = switchTransform.rotation;
    }

    private void Toggle()
    {
        isOn = !isOn;

        if (isOn)
            switchTransform.eulerAngles = eulerAngleSwitch;
        else
            switchTransform.rotation = startRotate;

        Debug.Log(isOn ? "Выключатель включен" : "Выключатель выключен");
        //Вызывать логику проверки цепи

        CircuitManager.Instance?.RecalculateCircuits();
    }

    public override List<ContextMenuItem> GetContextMenuItems()
    {
        return new List<ContextMenuItem>
        {
            new ContextMenuItem {DynamicLabel = () => isOn ? "Выкл" : "Вкл", Action = Toggle,CloseOnClick = false},
            new ContextMenuItem {Label = "Переместить", Action = () => PlacementSystem.Instance.StartReposition(this) },
            new ContextMenuItem {Label = "Подключить", Action = () => CircuitManager.Instance.StartBuildFromContext(this)},
            new ContextMenuItem {Label = "Удалить", Action = () => Destroy(gameObject)}
        };
    }

    public override void OnInteract()
    {
        if (CircuitManager.Instance.Mode == CircuitBuildMode.Normal)
            Toggle();
        else
            base.OnInteract();
    }

    public override string GetObjectID()
    {
        return gameObject.name;
    }
}
