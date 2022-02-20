using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownController : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private List<string> systemNames;
    private List<LSystem.Definition> systems;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    public LSystem.Definition GetSelectedSystem()
    {
        return systems[dropdown.value];
    }

    public void RefreshDropdown(List<LSystem.Definition> newSystems)
    {
        systems = newSystems;
        systemNames = new List<string>();

        foreach (LSystem.Definition system in systems)
        {
            systemNames.Add(system.name);
        }

        if (null == dropdown)
            dropdown = GetComponent<TMP_Dropdown>();

        dropdown.ClearOptions();
        dropdown.AddOptions(systemNames);
    }
}
