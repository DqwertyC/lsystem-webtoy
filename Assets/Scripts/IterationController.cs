using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IterationController : MonoBehaviour
{


    public int minValue = 0;
    public int maxValue = 6;
    public int value = 1;

    public TMP_Text iterationDisplay;

    public Button addButton;
    public Button subButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        iterationDisplay.text = value.ToString();

        subButton.interactable = true;
        addButton.interactable = true;

        if (maxValue == value)
        {
            addButton.interactable = false;
        }

        if (minValue == value)
        {
            subButton.interactable = false;
        }
    }

    public void Add()
    {
        value++;
        value = (int)Mathf.Clamp(value, minValue, maxValue);
    }

    public void Sub()
    {
        value--;
        value = (int)Mathf.Clamp(value, minValue, maxValue);
    }
}
