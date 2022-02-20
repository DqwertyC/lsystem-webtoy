using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventSystem))]
public class MenuController : MonoBehaviour
{
    bool paused = false;
    bool hidden = false;
    public GameObject menuPanel;
    public RenderCameraController activeCamera;

    public IterationController iterationController;
    public MediaController mediaController;
    public Slider drawSlider;
    public Slider angleSlider;

    public List<GameObject> objectsToHide;
    public List<Button> buttonsToFade;
    public List<GameObject> objectsToDisableOnWeb;

    EventSystem eventSystem;

    private void Start()
    {
        eventSystem = GetComponent<EventSystem>();
        HideOfflineObjects();
    }

    // Update is called once per frame
    void Update()
    {
        paused = menuPanel.activeInHierarchy;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        activeCamera.targetLSystem.iteration = iterationController.value;
        activeCamera.targetLSystem.drawPercent = drawSlider.value;
        activeCamera.targetLSystem.turnAngle = angleSlider.value;
        activeCamera.controlsAreActive = !paused;
        mediaController.forcePause = paused;
    }
    

    public void TogglePause()
    {
        paused = !paused;
        menuPanel.SetActive(paused);
        eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        HideOfflineObjects();
    }

    public void ToggleHide()
    {
        hidden = !hidden;

        foreach (GameObject o in objectsToHide)
        {
            RectTransform transform = o.GetComponent<RectTransform>();
            transform.position = (hidden ? 10 : 0.1f) * transform.position;
            //o.SetActive(!hidden);
        }

        foreach (Button b in buttonsToFade)
        {
            ColorBlock colors = b.colors;
            colors.normalColor = new Color()
            {
                r = colors.normalColor.r,
                g = colors.normalColor.g,
                b = colors.normalColor.b,
                a = hidden ? 0 : 1
            };
            colors.selectedColor = colors.normalColor;
            b.colors = colors;
        }

        eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        HideOfflineObjects();
    }

    void HideOfflineObjects()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            foreach (GameObject o in objectsToDisableOnWeb)
            {
                o.SetActive(false);
            }
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
