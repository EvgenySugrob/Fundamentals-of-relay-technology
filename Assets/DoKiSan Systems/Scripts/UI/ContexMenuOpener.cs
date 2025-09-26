using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContexMenuOpener : MonoBehaviour
{
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private Camera rayCamera; // можно оставить пустым — будет использован main

    private void Awake()
    {
        if (rayCamera == null) rayCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = rayCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableMask))
            {
                var obj = hit.collider.GetComponentInParent<InteractiveObject>();
                if (obj != null)
                {
                    ContextMenuManager.Instance.Show(obj, mousePos);
                }
            }
        }
    }
}
