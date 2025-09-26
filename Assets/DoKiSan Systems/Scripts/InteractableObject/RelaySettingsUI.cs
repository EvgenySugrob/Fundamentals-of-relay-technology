using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelaySettingsUI : MonoBehaviour
{
    public static RelaySettingsUI Instance { get; private set; }

    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject sliderPrefab;

    private RelayBase currentRelay;
    private readonly List<GameObject> spawnedControls = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        gameObject.SetActive(false);
    }

    public void Open(RelayBase relay)
    {
        currentRelay = relay;
        gameObject.SetActive(true);

        ClearUI();

        List<RelayParameter> parameters = relay.GetParameters();

        foreach (var param in parameters)
        {
            var go = Instantiate(sliderPrefab, contentParent);
            spawnedControls.Add(go);

            var label = go.GetComponentInChildren<TMP_Text>();
            var slider = go.GetComponentInChildren<Slider>();

            label.text = $"{param.Name}: {param.Value:F2}";
            slider.minValue = param.MinValue;
            slider.maxValue = param.MaxValue;
            slider.value = param.Value;

            slider.onValueChanged.AddListener((val) =>
            {
                label.text = $"{param.Name}: {val:F2}";
                param.OnValueChanged?.Invoke(val);
            });
        }
    }

    public void Close()
    {
        currentRelay = null;
        gameObject.SetActive(false);
    }

    private void ClearUI()
    {
        foreach (var go in spawnedControls)
            Destroy(go);
        spawnedControls.Clear();
    }
}
