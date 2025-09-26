using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static MouseCursorHandler;

public abstract class InteractiveObject : MonoBehaviour, IInteractable
{
    private PlacementSystem placementSystem;
    private readonly List<CircuitConnection> connections = new List<CircuitConnection>();

    protected virtual void Awake()
    {
        placementSystem = FindAnyObjectByType<PlacementSystem>();
    }

    public abstract List<ContextMenuItem> GetContextMenuItems();

    public virtual void SetPowered(bool state) { }

    public virtual void OnInteract()
    {
        CircuitManager.Instance.OnObjectClicked(this);
    }

    public abstract string GetObjectID();

    public virtual void RemoveObject()
    {
        CircuitManager.Instance.RemoveObject(this);
        Destroy(gameObject);
    }

    public void MoveWithPlacementSystem()
    {
        if (placementSystem != null)
            placementSystem.StartReposition(this);
    }

    public void AddConnection(CircuitConnection conn)
    {
        if (!connections.Contains(conn))
            connections.Add(conn);
    }

    public void RemoveConnection(CircuitConnection conn)
    {
        connections.Remove(conn);
    }

    public IEnumerable<CircuitConnection> GetConnections()
    {
        return connections;
    }

    public void OnHoverEnter()
    {
        
    }

    public void OnHoverExit()
    {
        
    }

    public void SetHighlight(bool state)
    {
        
    }
}
