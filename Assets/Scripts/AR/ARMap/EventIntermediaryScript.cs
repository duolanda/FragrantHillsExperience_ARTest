using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;

public class EventIntermediaryScript : MonoBehaviour
{
    private ARSessionOrigin arSessionOrigin;

    void Start()
    {
        GameObject arSessionOriginObject = GameObject.Find("AR Session Origin");
        if (arSessionOriginObject != null)
        {
            arSessionOrigin = arSessionOriginObject.GetComponent<ARSessionOrigin>();
        }
        Debug.Log("arSessionOrigin"+arSessionOrigin);
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // 将触摸位置转换为视口中的射线
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            // 发射射线并检测是否碰到了Collider
            if (Physics.Raycast(ray, out hit))
            {
                // 检测射线是否碰到了我们的目标物体
                if (hit.collider != null && hit.collider.gameObject.name == "Plane")
                {
                    if (arSessionOrigin != null)
                    {
                        arSessionOrigin.GetComponent<TrackedImageInfoManager>().OnImageClicked(this.gameObject);
                    }
                }
                // 如果触摸的是具有特定标签的对象，比如“Interactive”，用来实现其他触摸交互
                else if (hit.collider.gameObject.CompareTag("Interactive"))
                {
                    // 可以在这里处理触摸到特定对象的逻辑
                }
            }
            else {
                // 射线没有碰撞到任何对象，这意味着点击的是屏幕上的其他位置，关闭所有 Canvas 并显示 circleBorder
                arSessionOrigin.GetComponent<TrackedImageInfoManager>().CloseAllCanvasesAndShowBorders();
            }
        }
    }
}
