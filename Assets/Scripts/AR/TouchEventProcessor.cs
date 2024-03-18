using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;

public class TouchEventProcessor : MonoBehaviour
{
    private ARSessionOrigin arSessionOrigin;
    private TrackedImageInfoManager trackedImageInfoManager;

    void Start()
    {
        GameObject arSessionOriginObject = GameObject.Find("AR Session Origin");
        if (arSessionOriginObject != null)
        {
            arSessionOrigin = arSessionOriginObject.GetComponent<ARSessionOrigin>();
            trackedImageInfoManager = arSessionOrigin.GetComponent<TrackedImageInfoManager>();
        }
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
                        trackedImageInfoManager.OnImageClicked(hit.collider.gameObject.transform.parent.parent.gameObject);
                    }
                }
                // 如果触摸的是具有特定标签的对象，比如“Interactive”，用来实现其他触摸交互
                else if (hit.collider.gameObject.CompareTag("Interactive"))
                {
                    if (hit.collider.gameObject.name == "Button")
                    {
                        Debug.Log("触发选择按钮");
                        trackedImageInfoManager.OnPushSelectButton(hit.collider.gameObject.transform);
                    }else if(hit.collider.gameObject.name == "ScenicVideo")
                    {
                        Debug.Log("触发视频");
                        trackedImageInfoManager.OnVideo(hit.collider.gameObject.transform);
                    }
                    
                }
            }
            else {
                // 射线没有碰撞到任何对象，这意味着点击的是屏幕上的其他位置，关闭所有 Canvas 并显示 circleBorder
                trackedImageInfoManager.CloseAllCanvasesAndShowBorders();
            }
        }
    }
}
