using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public enum PlacementMode
{
    None,
    FromInventory,
    Reposition
}

public class PlacementSystem : MonoBehaviour
{
    public static PlacementSystem Instance;

    [Header("Materials")]
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;

    [Header("Settings")]
    [SerializeField] private LayerMask placementLayerMask; // где можно ставить (Ground/WorkingPlace)
    [SerializeField] private LayerMask overlapCheckMask;   // интерактивные объекты
    [SerializeField] private float maxRayDistance = 100f;
    [SerializeField] private string validTag = "WorkingPlace";

    [SerializeField] private Camera cam;

    private GameObject currentGhost;
    private GameObject prefabToPlace;
    private InteractiveObject originalObjectBeingRepositioned;

    private Renderer[] ghostRenderers;
    private Collider[] ghostColliders;
    private List<Material[]> originalMaterialsList = new List<Material[]>();

    private bool isPlacing = false;
    private bool isOverlapping = false;
    private PlacementMode mode = PlacementMode.None;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (!isPlacing || prefabToPlace == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, placementLayerMask))
        {
            if (currentGhost == null)
                CreateGhost();

            UpdateGhost(hit);

            if (mode == PlacementMode.FromInventory)
            {
                // старое поведение: по отпусканию ЛКМ
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    if (CanPlace(hit)) PlaceObject(hit);
                    else CancelPlacement();
                }
            }
            else if (mode == PlacementMode.Reposition)
            {
                // новое поведение: подтверждение ЛКМ, отмена ПКМ
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (CanPlace(hit)) PlaceObject(hit);
                }

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    CancelPlacement();
                }
            }
        }
        else
        {
            if (currentGhost != null)
                SetGhostMaterial(invalidMaterial);

            if (mode == PlacementMode.FromInventory && Mouse.current.leftButton.wasReleasedThisFrame)
                CancelPlacement();
            else if (mode == PlacementMode.Reposition && Mouse.current.rightButton.wasPressedThisFrame)
                CancelPlacement();
        }
    }

    public void StartPlacementFromPrefab(GameObject prefabAsset)
    {
        prefabToPlace = prefabAsset;
        originalObjectBeingRepositioned = null;
        isPlacing = true;
        mode = PlacementMode.FromInventory;
    }

    public void StartReposition(InteractiveObject obj)
    {
        if (obj == null) return;

        originalObjectBeingRepositioned = obj;
        obj.gameObject.SetActive(false);

        prefabToPlace = obj.gameObject;
        isPlacing = true;
        mode = PlacementMode.Reposition;
    }

    private void CreateGhost()
    {
        currentGhost = Instantiate(prefabToPlace);
        
        if(!currentGhost.activeSelf)
            currentGhost.SetActive(true);

        ghostRenderers = currentGhost.GetComponentsInChildren<Renderer>();
        originalMaterialsList.Clear();

        foreach (var r in ghostRenderers)
        {
            originalMaterialsList.Add(r.sharedMaterials);
        }

        ghostColliders = currentGhost.GetComponentsInChildren<Collider>();
        foreach (var col in ghostColliders)
            col.enabled = false;

        SetGhostMaterial(invalidMaterial);
    }

    private void UpdateGhost(RaycastHit hit)
    {
        currentGhost.transform.position = hit.point;
        currentGhost.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        isOverlapping = CheckOverlap();

        if (CanPlace(hit))
            SetGhostMaterial(validMaterial);
        else
            SetGhostMaterial(invalidMaterial);
    }

    private void PlaceObject(RaycastHit hit)
    {
        if (originalObjectBeingRepositioned != null)
        {
            originalObjectBeingRepositioned.transform.position = hit.point;
            originalObjectBeingRepositioned.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            var cols = originalObjectBeingRepositioned.GetComponentsInChildren<Collider>();
            foreach (var c in cols) c.enabled = true;

            originalObjectBeingRepositioned.gameObject.SetActive(true);

            Destroy(currentGhost);
            currentGhost = null;

            originalObjectBeingRepositioned = null;
            prefabToPlace = null;
            isPlacing = false;
            return;
        }

        // --- обычная установка нового объекта (из prefab asset) ---
        GameObject placed = Instantiate(prefabToPlace, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));

        Collider[] colliders = placed.GetComponentsInChildren<Collider>();
        foreach (var col in colliders) col.enabled = true;

        Renderer[] placedRenderers = placed.GetComponentsInChildren<Renderer>();
        int count = Mathf.Min(placedRenderers.Length, originalMaterialsList.Count);
        for (int i = 0; i < count; i++)
        {
            placedRenderers[i].materials = originalMaterialsList[i];
        }

        Destroy(currentGhost);
        currentGhost = null;
        prefabToPlace = null;
        isPlacing = false;
    }

    private void CancelPlacement()
    {
        if (currentGhost != null)
            Destroy(currentGhost);

        // если это был режим перестановки — восстановим оригинал
        if (originalObjectBeingRepositioned != null)
        {
            originalObjectBeingRepositioned.gameObject.SetActive(true);
            originalObjectBeingRepositioned = null;
        }

        currentGhost = null;
        prefabToPlace = null;
        isPlacing = false;
    }

    private bool CanPlace(RaycastHit hit)
    {
        return hit.collider.CompareTag(validTag) && !isOverlapping;
    }

    private bool CheckOverlap()
    {
        if (currentGhost == null) return true;

        foreach (var ghostCol in ghostColliders)
        {
            Collider[] hits = Physics.OverlapBox(
                ghostCol.bounds.center,
                ghostCol.bounds.extents,
                ghostCol.transform.rotation,
                overlapCheckMask
            );

            foreach (var h in hits)
            {
                // Отклоняем пересечение если это сам ghost
                if (h.transform.IsChildOf(currentGhost.transform)) continue;

                // Также игнорируем оригинал, если он по какой-то причине ещё активен (как защита)
                if (originalObjectBeingRepositioned != null && h.transform.IsChildOf(originalObjectBeingRepositioned.transform))
                    continue;

                return true; // есть пересечение
            }
        }

        return false;
    }

    private void SetGhostMaterial(Material mat)
    {
        if (ghostRenderers == null) return;

        foreach (Renderer rend in ghostRenderers)
        {
            int slots = Mathf.Max(1, rend.sharedMaterials.Length);
            Material[] mats = new Material[slots];
            for (int i = 0; i < slots; i++) mats[i] = mat;
            rend.materials = mats;
        }
    }
}
