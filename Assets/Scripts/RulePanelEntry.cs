using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class RulePanelEntry : MonoBehaviour
{
    public TMP_InputField key;
    public TMP_Dropdown drawInstruction;
    public TMP_InputField value;

    public RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
