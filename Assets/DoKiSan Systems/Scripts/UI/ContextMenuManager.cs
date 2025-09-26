using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ContextMenuManager : MonoBehaviour
{
    public static ContextMenuManager Instance;

    [Header("Prefab & Canvas")]
    [SerializeField] private GameObject menuPrefab; // PrefabContextMenu
    [SerializeField] private Canvas parentCanvas;   // если не задан — будет найден автоматически

    private GameObject currentMenuInstance;
    private ContextMenuInstance currentInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
    }

    private void Update()
    {
        if (currentInstance == null) return;

        // Клик вне меню -> закрыть
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            var cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
            if (!RectTransformUtility.RectangleContainsScreenPoint(currentInstance.BasePanel, mousePos, cam))
            {
                Hide();
            }
        }
    }

    /// <summary>Показать меню для target в экранной позиции screenPos (в пикселях)</summary>
    public void Show(InteractiveObject target, Vector2 screenPos)
    {
        Hide();

        if (menuPrefab == null || parentCanvas == null)
        {
            Debug.LogWarning("ContextMenuManager: menuPrefab или parentCanvas не назначены.");
            return;
        }

        currentMenuInstance = Instantiate(menuPrefab, parentCanvas.transform);
        currentInstance = currentMenuInstance.GetComponent<ContextMenuInstance>();
        if (currentInstance == null)
        {
            Debug.LogError("Prefabs' root must have ContextMenuInstance script.");
            Destroy(currentMenuInstance);
            return;
        }

        currentInstance.Initialize(target, screenPos, parentCanvas);
    }

    public void Hide()
    {
        if (currentMenuInstance != null)
            Destroy(currentMenuInstance);

        currentMenuInstance = null;
        currentInstance = null;
    }
}
