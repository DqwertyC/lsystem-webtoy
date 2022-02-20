using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ShowHideController : MonoBehaviour
{
    RectTransform rectTransform;
    Slider slider;

    // Start is called before the first frame update
    public Button toggleButton;

    public float moveTime = 1;
    public Vector2 moveDelta = 55 * Vector2.up;

    float initialRotation;

    public bool hidden;

    bool isMoving = false;

    Vector2 startPosition;
    float startTime;

    void Start()
    {
        initialRotation = toggleButton.transform.rotation.eulerAngles.z - (hidden ? 180 : 0);
        rectTransform = GetComponent<RectTransform>();
        slider = GetComponent<Slider>();
        toggleButton.onClick.AddListener(ToggleHide);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            float elapsedPercent = (Time.time - startTime) / moveTime;

            if (elapsedPercent > 1) 
                elapsedPercent = 1;

            rectTransform.anchoredPosition = startPosition + (hidden ? -1 : 1) * (moveDelta * elapsedPercent);
            toggleButton.transform.rotation = Quaternion.Euler(new Vector3(0, 0, (hidden ? 180 : 0) + initialRotation + (180 * elapsedPercent)));

            if (elapsedPercent == 1)
            {
                hidden = !hidden;
                toggleButton.interactable = true;
                isMoving = false;

                if (null != slider)
                    slider.interactable = !hidden;
            }
        }
    }

    public void ToggleHide()
    {
        toggleButton.interactable = false;
        startPosition = rectTransform.anchoredPosition;
        startTime = Time.time;
        isMoving = true;
    }

    private IEnumerator Hide()
    {
        
        Vector2 startPosition = rectTransform.anchoredPosition;

        for (float i = 0; i <= moveTime; i++)
        {
            rectTransform.anchoredPosition = startPosition + (moveDelta * i / moveTime);
            toggleButton.transform.rotation = Quaternion.Euler(new Vector3(0, 0, initialRotation + (180 * i / moveTime)));
            yield return null;
        }
        toggleButton.interactable = true;
        hidden = true;
    }

    private IEnumerator Show()
    {
        toggleButton.interactable = false;
        Vector2 startPosition = rectTransform.anchoredPosition;

        for (float i = 0; i <= moveTime; i++)
        {
            rectTransform.anchoredPosition = startPosition - (moveDelta * i / moveTime);
            toggleButton.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180 + initialRotation + (180 * i / moveTime)));
            yield return null;
        }
        toggleButton.interactable = true;
        hidden = false;
    }
}
