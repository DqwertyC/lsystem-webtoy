using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class RenderCameraController : MonoBehaviour
{
    public Camera renderCamera;
    public LSystemDrawer targetLSystem;
    private float cameraSize;

    private float startCameraSize;
    private float maxCameraSize;
    private Vector3 startCameraPos;

    private Vector3 dragLast;

    private RectTransform rectTransform;

    public float imageSize;

    public bool controlsAreActive;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startCameraPos = renderCamera.transform.position;
        cameraSize = renderCamera.orthographicSize;
        startCameraSize = cameraSize;
        maxCameraSize = cameraSize;
        controlsAreActive = true;

        imageSize = Screen.height / 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (controlsAreActive)
        {
            UpdateZoom();
            UpdateDrag();
        }
    }

    void UpdateZoom()
    {
        float scrollValue = Input.mouseScrollDelta.y;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Rect newRect = new Rect(corners[0], corners[2] - corners[0]);

        if (newRect.Contains(Input.mousePosition) && Mathf.Abs(scrollValue) > 0.1f)
        {
            Vector2 mousePos = (Vector2)Input.mousePosition - newRect.center;
            Vector2 oldCamCenter = renderCamera.transform.position;
            float oldCamSize = renderCamera.orthographicSize;
            Vector2 worldPos = oldCamCenter + (mousePos * oldCamSize / imageSize);

            if (scrollValue > 0.1f)
            {
                cameraSize *= 0.9f;
            }
            else if (scrollValue < -0.1f)
            {
                cameraSize /= 0.9f;
            }

            if (cameraSize >= maxCameraSize) cameraSize = maxCameraSize;
            if (cameraSize <= 1) cameraSize = 1;

            Vector3 newCamCenter = worldPos - (mousePos * cameraSize / imageSize);
            newCamCenter.z = startCameraPos.z;

            renderCamera.orthographicSize = cameraSize;
            renderCamera.transform.position = newCamCenter;

            targetLSystem.currentZoom = cameraSize / startCameraSize;
        }
    }

    void UpdateDrag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragLast = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 mouseDelta = Input.mousePosition - dragLast;
            Vector3 camDelta = mouseDelta * (cameraSize / imageSize);

            renderCamera.transform.position = renderCamera.transform.position - camDelta;
            dragLast = Input.mousePosition;
        }
        else if (maxCameraSize == cameraSize)
        {
            renderCamera.transform.position = Vector3.Lerp(renderCamera.transform.position, startCameraPos, 0.05f);
        }
    }
}
