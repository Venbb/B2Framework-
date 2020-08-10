using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[Serializable]
public class TabControlEntry
{
    [SerializeField]
    private GameObject _panel = null;
    public GameObject Panel { get { return _panel; } }

    [SerializeField]
    private Toggle _tab = null;
    public Toggle Tab { get { return _tab; } }
}

[RequireComponent(typeof(ToggleGroup))]
public class TabControlComponent : MonoBehaviour
{
    [SerializeField]
    private List<TabControlEntry> entries = null;

    protected virtual void Start()
    {
        TabControlEntry defaultActiveEntry = null;
        foreach (TabControlEntry entry in entries)
        {
            AddButtonListener(entry);

            if (entry.Tab.isOn)
                defaultActiveEntry = entry;
        }

        if (null != defaultActiveEntry)
        {
            SelectTab(defaultActiveEntry);
        }
    }

    public void AddEntry(TabControlEntry entry)
    {
        entries.Add(entry);
    }

    private void AddButtonListener(TabControlEntry entry)
    {
        entry.Tab.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                SelectTab(entry);
        });
    }

    private void SelectTab(TabControlEntry selectedEntry)
    {
        foreach (TabControlEntry entry in entries)
        {
            bool isSelected = entry == selectedEntry;
            if (null != entry.Panel)
                entry.Panel.SetActive(isSelected);
        }
    }
}