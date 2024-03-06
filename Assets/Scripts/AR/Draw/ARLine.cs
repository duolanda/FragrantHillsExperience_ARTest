using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARLine
{
    private int positionCount = 0;

    private Vector3 prevPointDistance = Vector3.zero;
    
    public LineRenderer LineRenderer { get; set; }
    public Vector3 ParentPosition;
    private LineSettings settings;
    

    public ARLine(LineSettings settings)
    {
        this.settings = settings;
    }

    public void AddPoint(Vector3 position)
    {
        if(prevPointDistance == null)
            prevPointDistance = position;

        if(prevPointDistance != null && Mathf.Abs(Vector3.Distance(prevPointDistance, position)) >= settings.minDistanceBeforeNewPoint)
        {
            prevPointDistance = position;
            positionCount++;

            LineRenderer.positionCount = positionCount;

            // index 0 positionCount must be - 1
            LineRenderer.SetPosition(positionCount - 1, position);

            // applies simplification if reminder is 0
            if(LineRenderer.positionCount % settings.applySimplifyAfterPoints == 0 && settings.allowSimplification)
            {
                LineRenderer.Simplify(settings.tolerance);
            }
        }   
    }

    public void AddNewLineRenderer(Transform parent, ARAnchor anchor, Vector3 position)
    {
        positionCount = 2;
        GameObject go = new GameObject($"LineRenderer");
        
        go.transform.parent = anchor?.transform ?? parent;
        go.transform.position = position;
        go.tag = settings.lineTagName;

        ParentPosition = go.transform.position;
        
        LineRenderer goLineRenderer = go.AddComponent<LineRenderer>();
        goLineRenderer.startWidth = settings.startWidth;
        goLineRenderer.endWidth = settings.endWidth;

        goLineRenderer.startColor = settings.startColor;
        goLineRenderer.endColor = settings.endColor;

        goLineRenderer.material = settings.defaultMaterial;
        goLineRenderer.useWorldSpace = true;
        goLineRenderer.positionCount = positionCount;

        goLineRenderer.numCornerVertices = settings.cornerVertices;
        goLineRenderer.numCapVertices = settings.endCapVertices;

        goLineRenderer.SetPosition(0, position);
        goLineRenderer.SetPosition(1, position);

        LineRenderer = goLineRenderer;

        ARDebugManager.Instance.LogInfo($"New line renderer created");
    } 
    
    public void AddNewLineRenderer(SerializableLineData deserializedData)
    {
        GameObject go = new GameObject($"LineRenderer");

        // go.transform.parent = lr.transform;
        go.transform.position = new Vector3(deserializedData.parentPosition.x,deserializedData.parentPosition.y,deserializedData.parentPosition.z);
        go.tag = settings.lineTagName;
        
        LineRenderer goLineRenderer = go.AddComponent<LineRenderer>();
        
        goLineRenderer.startWidth = settings.startWidth;
        goLineRenderer.endWidth = settings.endWidth;

        goLineRenderer.startColor = settings.startColor;
        goLineRenderer.endColor = settings.endColor;

        goLineRenderer.material = settings.defaultMaterial;
        goLineRenderer.useWorldSpace = true;
        goLineRenderer.positionCount = positionCount;

        goLineRenderer.numCornerVertices = settings.cornerVertices;
        goLineRenderer.numCapVertices = settings.endCapVertices;
        
        goLineRenderer.positionCount = deserializedData.points.Count;
        for (int i = 0; i < deserializedData.points.Count; i++)
        {
            SerializableVector3 pointPosition = deserializedData.points[i];
            Vector3 position = new Vector3(pointPosition.x, pointPosition.y, pointPosition.z);
            goLineRenderer.SetPosition(i, position);
        }
        
        LineRenderer = goLineRenderer;

        ARDebugManager.Instance.LogInfo($"New received line renderer created");
    }
}