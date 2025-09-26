using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ContextMenuInstance : MonoBehaviour, IPointerExitHandler
{
    [Header("Links inside prefab")]
    [SerializeField] private RectTransform basePanel;  // BaseInteractiveContextMenu (фон) — сюда добавляются кнопки как дочерние
    [SerializeField] private GameObject buttonPrefab;  // ActionButton prefab (root)

    private Canvas parentCanvas;
    private readonly List<(Button btn, ContextMenuItem item, TextMeshProUGUI txt)> activeButtons
        = new List<(Button, ContextMenuItem, TextMeshProUGUI)>();


    public RectTransform BasePanel => basePanel;

    /// <summary>Инициализация меню — вызывает менеджер сразу после Instantiate.</summary>
    public void Initialize(InteractiveObject target, Vector2 screenPos, Canvas canvas)
    {
        parentCanvas = canvas;
        BuildButtons(target);
        gameObject.SetActive(true);
        AdjustPosition(screenPos);
    }

    private void BuildButtons(InteractiveObject target)
    {
        ClearButtons();

        var items = target.GetContextMenuItems();
        if (items == null) return;

        foreach (var item in items)
        {
            GameObject go = Instantiate(buttonPrefab, basePanel);
            // ищем компонент Button и TMP в дочерних элементах (ActionButton -> Button)
            Button btn = go.GetComponentInChildren<Button>(true);
            TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>(true);

            if (btn == null || txt == null)
            {
                Debug.LogWarning("ActionButton prefab должен содержать Button и TextMeshProUGUI в дочерних элементах.");
                Destroy(go);
                continue;
            }

            txt.text = item.GetLabel();

            // сохраняем локальные переменные для closure
            ContextMenuItem localItem = item;
            btn.onClick.AddListener(() =>
            {
                try { localItem.Action?.Invoke(); }
                catch (System.Exception ex) { Debug.LogException(ex); }

                if (localItem.CloseOnClick)
                {
                    ContextMenuManager.Instance.Hide();
                }
                else
                {
                    // обновляем подписи (напр. "Вкл" -> "Выкл")
                    RefreshDynamicLabels();
                }
            });

            activeButtons.Add((btn, item, txt));
        }

        // если нужны дополнительные LayoutForce rebuild — можно вызвать:
        LayoutRebuilder.ForceRebuildLayoutImmediate(basePanel);
    }

    private void RefreshDynamicLabels()
    {
        foreach (var e in activeButtons)
        {
            if (e.item.DynamicLabel != null)
                e.txt.text = e.item.GetLabel();
        }

        // пересчитать размер панели
        LayoutRebuilder.ForceRebuildLayoutImmediate(basePanel);
    }

    private void ClearButtons()
    {
        foreach (Transform t in basePanel)
            Destroy(t.gameObject);

        activeButtons.Clear();
    }

    private void OnDestroy()
    {
        ClearButtons();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // закрываем меню, когда курсор уходит с фона меню
        ContextMenuManager.Instance.Hide();
    }

    private void AdjustPosition(Vector2 screenPos)
    {
        // корректно позиционируем меню внутри Canvas и не даём ему выйти за экран
        // Используем RectTransformUtility.ScreenPointToWorldPointInRectangle, чтобы работать со ScreenSpace - Camera и Overlay

        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        // переводим экранную позицию в мировую внутри canvas
        Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
        RectTransform rootRt = (RectTransform)transform;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPos, cam, out Vector3 worldPos))
        {
            rootRt.position = worldPos;
        }
        else
        {
            // fallback
            rootRt.position = (Vector3)screenPos;
        }

        // проверка границ экрана — корректируем смещение
        basePanel.GetWorldCorners(s_corners);
        Vector3 offset = Vector3.zero;

        // правый верх corner index 2, левый ниж index 0, левый верх index 1? (Unity corners: 0=bl,1=tl,2=tr,3=br)
        if (s_corners[2].x > Screen.width) offset.x = Screen.width - s_corners[2].x;
        if (s_corners[0].x < 0) offset.x = -s_corners[0].x;
        if (s_corners[0].y < 0) offset.y = -s_corners[0].y;
        if (s_corners[1].y > Screen.height) offset.y = Screen.height - s_corners[1].y;

        rootRt.position += offset;

        // ещё раз перестраиваем, чтобы корректно показать изменившуюся форму
        LayoutRebuilder.ForceRebuildLayoutImmediate(basePanel);
    }

    private static readonly Vector3[] s_corners = new Vector3[4];
}
