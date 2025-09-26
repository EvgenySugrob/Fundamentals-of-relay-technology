using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryButtons : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private PlacementSystem placementSystem;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (placementSystem != null && prefab != null)
        {
            placementSystem.StartPlacementFromPrefab(prefab);
        }
    }
}
