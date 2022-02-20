using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LSystemDrawer : MonoBehaviour
{
    [Range(0, 6)]
    public int iteration = 0;
    private int lastIteration = -1;
    private int lockedIteration = 0;

    [Range(0.01f, 1.0f)]
    public float currentZoom = 1.0f;
    private float oldZoom = 1.0f;

    [Range(0.0f, 1.0f)]
    public float drawPercent = 1.0f;
    private float lastDrawPercent = -1;

    [Range(0.0f, 360.0f)]
    public float turnAngle = 0.0f;
    private float lastTurnAngle = -1;
    private float lockedTurnAngle = 0;

    public bool redraw;

    public LSystem currentSystem;

    public Gradient baseGradient = new Gradient();
    public Material baseMaterial;

    List<GameObject> renderers = new List<GameObject>();
    GameObject segmentHolder;

    private bool isCalculating = false;
    private Thread calculationThread;

    private Bounds currentBounds;

    private int calculationFrames;

    float currentScale;

    public int totalPoints;
    private bool isThreaded = true;

    // Start is called before the first frame update
    void Start()
    {
        segmentHolder = new GameObject("Segment Holder");
        segmentHolder.transform.parent = transform;
        segmentHolder.transform.position = Vector3.zero;

        currentZoom = 1.0f;

        currentSystem = new LSystem(LSystemSamples.KochSnowflake);
        turnAngle = currentSystem.definition.turnAngle;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            isThreaded = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lastDrawPercent != drawPercent)
        {
            //baseGradient.alphaKeys = new GradientAlphaKey[]
            //{
            //    new GradientAlphaKey(drawPercent > 0 ? 1.0f : 0.0f, -0.1f),
            //    new GradientAlphaKey(drawPercent > 0 ? 1.0f : 0.0f, drawPercent-float.Epsilon),
            //    new GradientAlphaKey(drawPercent < 1 ? 0.0f : 1.0f, drawPercent+float.Epsilon),
            //    new GradientAlphaKey(drawPercent < 1 ? 0.0f : 1.0f, 1.1f)
            //};

            lastDrawPercent = drawPercent;
            redraw = true;
        }

        if (isThreaded)
        {
            if (!isCalculating)
            {
                lockedIteration = iteration;
                lockedTurnAngle = turnAngle;
                if (lastIteration != lockedIteration || lastTurnAngle != turnAngle)
                {
                    if (currentSystem.TrySetBackgroundIteration(lockedIteration, turnAngle))
                    {
                        calculationThread = new Thread(new ThreadStart(currentSystem.CalculateBackground));
                        calculationThread.Start();
                        isCalculating = true;
                        calculationFrames = 0;
                    }
                }
                else if (redraw)
                {
                    Redraw();
                }
            }
            else
            {
                calculationFrames++;
                if (calculationThread != null && !calculationThread.IsAlive)
                {
                    calculationThread = null;
                    isCalculating = false;
                    Redraw();

                }
            }
        }
        else
        {
            lockedIteration = iteration;
            lockedTurnAngle = turnAngle;

            if (lastIteration != lockedIteration || lastTurnAngle != turnAngle || redraw)
            {
                Redraw();
            }
        }

        if (currentZoom != oldZoom)
        {
            UpdateLineWidth();
            oldZoom = currentZoom;
        }
    }

    private void Redraw()
    {
        foreach (GameObject child in renderers)
        {
            Destroy(child);
        }

        segmentHolder.transform.localScale = new Vector3(1, 1, 1);
        renderers = GetRenderers(currentSystem.GetPaths(lockedIteration, lockedTurnAngle), baseGradient, out currentBounds);
        currentBounds.Encapsulate(currentSystem.axiomBounds);

        float xScale = 80 / currentBounds.size.x;
        float yScale = 80 / currentBounds.size.y;
        currentScale = Mathf.Min(xScale, yScale);

        UpdateLineWidth();

        // Scale and center the path
        segmentHolder.transform.localScale = new Vector3(currentScale, currentScale, 1);
        segmentHolder.transform.position = -currentScale * currentBounds.center;

        Vector3 pos = segmentHolder.transform.localPosition;
        segmentHolder.transform.localPosition = new Vector3(pos.x, pos.y, 0);

        iteration = lockedIteration;
        turnAngle = lockedTurnAngle;

        lastIteration = iteration;
        lastTurnAngle = turnAngle;

        redraw = false;
    }

    private void UpdateLineWidth()
    {
        foreach (GameObject child in renderers)
        {
            LineRenderer lr = child.GetComponent<LineRenderer>();
            float width = currentZoom * 0.3f / currentScale;
            lr.startWidth = width;
            lr.endWidth = width;
        }
    }

    private List<GameObject> GetRenderers(List<List<Vector3>> segments, Gradient masterGradient, out Bounds bounds)
    {
        List<GameObject> renderers = new List<GameObject>();

        Bounds newBounds = new Bounds();
        newBounds.min = Vector3.zero;
        newBounds.max = Vector3.zero;

        totalPoints = 0;
        foreach (List<Vector3> segment in segments) totalPoints += segment.Count;

        int currentPos = 0;
        foreach (List<Vector3> segment in segments)
        {
            foreach (Vector3 point in segment)
                newBounds.Encapsulate(point);

            List<Vector3> modifiedSegment = new List<Vector3>();

            float start = ((float)currentPos) / totalPoints;
            currentPos += segment.Count;
            float end = ((float)currentPos) / totalPoints;

            if (end <= drawPercent)
            {
                // Draw all
                modifiedSegment.AddRange(segment);
            }
            else if (start >= drawPercent)
            {
                // Draw none
            }
            else
            {
                float maxDrawIndex = ((drawPercent - start) / (end - start)) * (segment.Count - 1);
                float lerpPortion = maxDrawIndex % 1;
                int prevIndex = Mathf.FloorToInt(maxDrawIndex);
                int postIndex = Mathf.CeilToInt(maxDrawIndex);

                Vector3 endPoint = Vector3.Lerp(segment[prevIndex], segment[postIndex], lerpPortion);

                for (int i = 0; i < maxDrawIndex; i++)
                {
                    modifiedSegment.Add(segment[i]);
                }

                modifiedSegment.Add(endPoint);
            }

            List<GradientColorKey> modifiedColorKeys = new List<GradientColorKey>();
            modifiedColorKeys.Add(new GradientColorKey(masterGradient.Evaluate(start), 0.0f));
            foreach (GradientColorKey colorKey in masterGradient.colorKeys)
            {
                if (colorKey.time > start && colorKey.time < end)
                {
                    float modifiedTime = (colorKey.time - start) / (end - start);
                    modifiedColorKeys.Add(new GradientColorKey(colorKey.color, modifiedTime));
                }
            }
            modifiedColorKeys.Add(new GradientColorKey(masterGradient.Evaluate(end), 1.0f));

            List<GradientAlphaKey> modifiedAlphaKeys = new List<GradientAlphaKey>();
            modifiedAlphaKeys.Add(new GradientAlphaKey(masterGradient.Evaluate(start).a, 0.0f));
            foreach (GradientAlphaKey alphaKey in masterGradient.alphaKeys)
            {
                if (alphaKey.time > start && alphaKey.time < end)
                {
                    float modifiedTime = (alphaKey.time - start) / (end - start);
                    modifiedAlphaKeys.Add(new GradientAlphaKey(alphaKey.alpha, modifiedTime));
                }
            }
            modifiedAlphaKeys.Add(new GradientAlphaKey(masterGradient.Evaluate(end).a, 1.0f));

            Gradient currentGradient = new Gradient();
            currentGradient.SetKeys(modifiedColorKeys.ToArray(), modifiedAlphaKeys.ToArray());

            GameObject child = new GameObject("LSystem Segment");
            child.gameObject.transform.SetParent(segmentHolder.transform);
            child.gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            child.gameObject.AddComponent<LineRenderer>();
            child.transform.localPosition = Vector3.zero;

            LineRenderer lr = child.GetComponent<LineRenderer>();
            lr.material = baseMaterial;
            lr.positionCount = modifiedSegment.Count;
            lr.SetPositions(modifiedSegment.ToArray());
            lr.colorGradient = currentGradient;
            lr.numCornerVertices = 90;
            lr.numCapVertices = 90;

            lr.alignment = LineAlignment.TransformZ;

            lr.useWorldSpace = false;

            renderers.Add(child);
        }

        bounds = newBounds;
        return renderers;
    }

    Vector3 GetLerpedPoint(List<Vector3> points, float floatIndex)
    {
        Vector3 pointBefore = points[Mathf.FloorToInt(floatIndex)];
        Vector3 pointAfter = points[Mathf.CeilToInt(floatIndex)];
        return Vector3.Lerp(pointBefore, pointAfter, floatIndex % 1);
    }
}
