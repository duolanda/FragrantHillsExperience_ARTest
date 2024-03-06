using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathDrawer : MonoBehaviour {
    public LineRenderer lineRenderer; // 在编辑器中指定或在 Start 中创建
    
    void Start() {
        if (lineRenderer == null) {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // // 设置 LineRenderer 的属性
        lineRenderer.widthMultiplier = 0.05f; // 路径的宽度
        lineRenderer.endColor = lineRenderer.startColor = new Color(229/255f, 120/255f, 12/255f, 1f);
    }

    public void DrawPath(List<ScenicSpot> path) {
        lineRenderer.positionCount = path.Count;
        Vector3[] points = path.Select(spot => spot.position).ToArray();
        lineRenderer.SetPositions(points);
    }
    
}

