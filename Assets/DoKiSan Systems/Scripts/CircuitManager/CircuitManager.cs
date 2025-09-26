using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CircuitManager : MonoBehaviour
{
    public static CircuitManager Instance { get; private set; }

    [Header("Wire Prefab")]
    [SerializeField] private GameObject wirePrefab;

    public CircuitBuildMode Mode { get; private set; } = CircuitBuildMode.Normal;

    private InteractiveObject firstSelectedObject;
    private readonly List<CircuitConnection> connections = new List<CircuitConnection>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (Mode != CircuitBuildMode.Normal && Mouse.current.rightButton.wasPressedThisFrame)
        {
            ExitBuildMode();
        }
    }

    public void ToggleBuildModeUI()
    {
        if (Mode == CircuitBuildMode.BuildingByUI)
            ExitBuildMode();
        else
        {
            Mode = CircuitBuildMode.BuildingByUI;
            firstSelectedObject = null;
        }
    }

    public void StartBuildFromContext(InteractiveObject starter)
    {
        if (starter == null) return;
        Mode = CircuitBuildMode.BuildingByContext;
        firstSelectedObject = starter;
    }

    public void ExitBuildMode()
    {
        Mode = CircuitBuildMode.Normal;
        firstSelectedObject = null;
    }

    public void OnObjectClicked(InteractiveObject obj)
    {
        if (obj == null) return;
        if (Mode == CircuitBuildMode.Normal) return;

        if (Mode == CircuitBuildMode.BuildingByUI)
        {
            HandleUIBuildClick(obj);
            return;
        }

        if (Mode == CircuitBuildMode.BuildingByContext)
        {
            HandleContextBuildClick(obj);
            return;
        }
    }

    private void HandleUIBuildClick(InteractiveObject obj)
    {
        if (firstSelectedObject == null)
        {
            firstSelectedObject = obj;
            return;
        }

        if (firstSelectedObject == obj) return;

        CreateConnection(firstSelectedObject, obj);
        firstSelectedObject = obj;

        RecalculateCircuits();
    }

    private void HandleContextBuildClick(InteractiveObject obj)
    {
        if (firstSelectedObject == null)
        {
            firstSelectedObject = obj;
            return;
        }

        if (firstSelectedObject == obj)
        {
            ExitBuildMode();
            return;
        }

        CreateConnection(firstSelectedObject, obj);
        ExitBuildMode();

        RecalculateCircuits();
    }

    private void CreateConnection(InteractiveObject a, InteractiveObject b)
    {
        if (AreConnected(a, b)) return;

        var go = Instantiate(wirePrefab);
        var wire = go.GetComponent<Wire>();
        wire.Initialize(a, b);

        var conn = new CircuitConnection(a, b, wire);
        connections.Add(conn);

        a.AddConnection(conn);
        b.AddConnection(conn);
    }

    private bool AreConnected(InteractiveObject a, InteractiveObject b)
    {
        foreach (var c in connections)
            if (c.Contains(a, b)) return true;
        return false;
    }

    public void RemoveObject(InteractiveObject obj)
    {
        var toRemove = new List<CircuitConnection>();
        foreach (var conn in connections)
        {
            if (conn.Contains(obj))
            {
                conn.Destroy();
                toRemove.Add(conn);

                var other = conn.GetOther(obj);
                other?.RemoveConnection(conn);
            }
        }

        foreach (var r in toRemove)
            connections.Remove(r);

        RecalculateCircuits();
    }

    public void RecalculateCircuits()
    {
        var allObjects = FindObjectsOfType<InteractiveObject>();
        foreach (var o in allObjects) o.SetPowered(false);

        // Теперь ищем цепи от батареек
        var batteries = FindObjectsOfType<Battery>();
        foreach (var battery in batteries)
        {
            if (battery.PositivePin == null || battery.NegativePin == null)
                continue;

            var visited = new HashSet<InteractiveObject>();
            var path = new List<InteractiveObject>();

            ExploreFromBatteryPin(battery.PositivePin, battery.NegativePin, battery.PositivePin, visited, path);
        }
    }

    private void ExploreFromBatteryPin(BatteryPin startPin, BatteryPin targetPin,
                                       InteractiveObject current,
                                       HashSet<InteractiveObject> visited,
                                       List<InteractiveObject> path)
    {
        if (current is Switch sw && !sw.IsOn)
            return;

        visited.Add(current);
        path.Add(current);

        foreach (var conn in current.GetConnections())
        {
            var other = conn.GetOther(current);
            if (other == null) continue;

            if (other is Switch swOther && !swOther.IsOn)
                continue;

            // Проверяем, дошли ли мы до "-" пина этой же батарейки
            if (other == targetPin)
            {
                Debug.Log($"Цепь замкнута ({startPin.ParentBattery.name}): {FormatPath(path)} -> {targetPin.name}");
                var fullPath = new List<InteractiveObject>(path) { targetPin };
                MarkPathPowered(fullPath);
                continue;
            }

            if (!visited.Contains(other))
            {
                var visitedCopy = new HashSet<InteractiveObject>(visited);
                var pathCopy = new List<InteractiveObject>(path);
                ExploreFromBatteryPin(startPin, targetPin, other, visitedCopy, pathCopy);
            }
        }
    }

    private void MarkPathPowered(List<InteractiveObject> path)
    {
        foreach (var obj in path)
        {
            obj.SetPowered(true);
        }
    }

    private string FormatPath(List<InteractiveObject> path)
    {
        if (path == null || path.Count == 0) return "(empty)";
        var names = new System.Text.StringBuilder();
        for (int i = 0; i < path.Count; i++)
        {
            if (i > 0) names.Append(" -> ");
            names.Append(path[i].name);
        }
        return names.ToString();
    }
}
