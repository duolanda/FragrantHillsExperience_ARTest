﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror.Examples.MultipleAdditiveScenes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
    /// and overlays some information as well as the source Texture2D on top of the
    /// detected image.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class TrackedImageInfoManager : MonoBehaviour
    {
        [Serializable]
        public class ScenicSpot
        {
            public string name;
            public string description;
        }

        [Serializable]
        public class ScenicSpotList<T> {
            public T[] items;
        }
        
        [SerializeField]
        [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
        Camera m_WorldSpaceCanvasCamera;

        /// <summary>
        /// The prefab has a world space UI canvas,
        /// which requires a camera to function properly.
        /// </summary>
        public Camera worldSpaceCanvasCamera
        {
            get { return m_WorldSpaceCanvasCamera; }
            set { m_WorldSpaceCanvasCamera = value; }
        }

        [SerializeField]
        [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
        Texture2D m_DefaultTexture;

        /// <summary>
        /// If an image is detected but no source texture can be found,
        /// this texture is used instead.
        /// </summary>
        public Texture2D defaultTexture
        {
            get { return m_DefaultTexture; }
            set { m_DefaultTexture = value; }
        }
        
        public VideoPlayer videoPlayer; // 引用你的VideoPlayer组件
        public TextMeshProUGUI debugInfo;


        ARTrackedImageManager m_TrackedImageManager;
        
        private Dictionary<string, string> ScenicSpotDictionary = new Dictionary<string, string>();
        private Dictionary<GameObject, string> imageObjectToNameMap = new Dictionary<GameObject, string>();
        private Dictionary<int, GameObject> spotID2TrackedGO = new Dictionary<int, GameObject> ();
        private List<int> selectedScenicSpots = new List<int>();
        
        private ScenicSpotsManager scenicSpotsManager;
        private Client2 ClientControl;
        
        private static T[] FromJson<T>(string json) {
            ScenicSpotList<T> wrapper = JsonUtility.FromJson<ScenicSpotList<T>>(json);
            return wrapper.items;
        }

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
            LoadScenicSpots();
        }

        void Start()
        {
            GameObject NetworkManager = GameObject.Find("NetworkManager");
            if (NetworkManager != null)
            {
                ClientControl = NetworkManager.GetComponent<Client2>();
            }
            ClientControl.ConnectToServer(); //连接到服务器
            
            scenicSpotsManager = ScenicSpotsManager.Instance;
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
            Client2.SpotUpdateEvent += UpdateSelectSpotShow;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            Client2.SpotUpdateEvent -= UpdateSelectSpotShow;
        }

        void UpdateInfo(ARTrackedImage trackedImage)
        {
            var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
            var circleBorder = planeParentGo.transform.GetChild(0).gameObject;

            
            // Disable the canvas if it is not being tracked
            if (trackedImage.trackingState != TrackingState.None)
            {
                circleBorder.SetActive(true);

                // 根据AR中跟踪到的二维图像的实际物理大小动态调整与之关联的GameObject的大小
                trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);
                
                // var material = circleBorder.GetComponentInChildren<MeshRenderer>().material;
                // material.mainTexture = defaultTexture;
            }
            else
            {
                circleBorder.SetActive(false);
            }
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                // 添加映射
                if (!imageObjectToNameMap.TryGetValue(trackedImage.gameObject, out string imageName))
                {
                    imageObjectToNameMap[trackedImage.gameObject] = trackedImage.referenceImage.name;
                }
                // Give the initial image a reasonable default scale
                trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);
                UpdateInfo(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
                UpdateInfo(trackedImage);
        }

        public void OnImageClicked(GameObject trackedImageGameObject)
        {
            DeactivateAllTrackedImages(trackedImageGameObject);
            
            var planeParentGo = trackedImageGameObject.transform.GetChild(0).gameObject;
            var circleBorder = planeParentGo.transform.GetChild(0).gameObject;
            var canvas = trackedImageGameObject.GetComponentInChildren<Canvas>(true); // true未激活也能找
            canvas.worldCamera = worldSpaceCanvasCamera;
            
            Transform infoPanel = canvas.transform.Find("Background");
            Transform titleTransform = infoPanel.transform.Find("Title");
            Transform spotDetailTransform = infoPanel.transform.Find("SpotDetail");
            TextMeshProUGUI title = titleTransform.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI spotDetail = spotDetailTransform.GetComponent<TextMeshProUGUI>();

            imageObjectToNameMap.TryGetValue(trackedImageGameObject, out string scenicSpotName);

            // debugInfo.text = "scenicSpotName:" + scenicSpotName;
            // 更新景点名称
            title.text = scenicSpotName;
            //更新景点介绍
            ScenicSpotDictionary.TryGetValue(scenicSpotName, out string detail);
            spotDetail.text = detail;

            if (scenicSpotName == "双清别墅东侧平房" || scenicSpotName=="镇芳楼 镇南房")
            {
                //景点名太长了，会和详情重合
                spotDetail.text = "\n\n"+detail;
            }
            
            VideoClip videoClip = Resources.Load<VideoClip>("Videos/"+scenicSpotName);
            if (videoClip != null)
            {
                ChangeVideoByClip(videoClip);
            }
            else
            {
                // 展示景点插图而不是视频
                Transform ScenicVideoTransform = infoPanel.transform.Find("ScenicVideo");
                RawImage ScenicVideoImage = ScenicVideoTransform.GetComponent<RawImage>();
                Texture image = Resources.Load<Texture>("Illustration/" + scenicSpotName);
                ScenicVideoImage.texture = image;
                
                // 获取原始图片的宽高比
                float aspectRatio = (float)image.width / image.height;
    
                // 设定目标尺寸
                float targetWidth = 1920;
                float targetHeight = 1080;

                // 计算新的宽度和高度
                float newWidth, newHeight;

                // 比较原始图片是宽大还是高大
                if (aspectRatio >= 1) { // 宽大于或等于高，宽度设为1920，高度按比例缩放
                    newWidth = targetWidth;
                    newHeight = newWidth / aspectRatio;
                } else { // 高大于宽，高度设为1080，宽度按比例缩放
                    newHeight = targetHeight;
                    newWidth = newHeight * aspectRatio;
                }

                // 应用计算出的宽度和高度到RectTransform
                RectTransform rectTransform = ScenicVideoTransform.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
            }
            
            circleBorder.gameObject.SetActive(false);
            canvas.gameObject.SetActive(true);
        }

        public void OnPushSelectButton(Transform selecteButton)
        {
            GameObject trackedImageGameObject = selecteButton.parent.parent.parent.gameObject;
            
            imageObjectToNameMap.TryGetValue(trackedImageGameObject, out string spotName);;
            int id = scenicSpotsManager.spotsDictionary.FirstOrDefault(s => s.Value.name == spotName).Key;
            
            spotID2TrackedGO[id] = trackedImageGameObject; // 记录到字典里方便以后用

            if (selectedScenicSpots.Contains(id))
            {
                DeselectAScenicSpot(id, trackedImageGameObject, selecteButton.gameObject);
            }
            else
            {
                SelectAScenicSpot(id, trackedImageGameObject, selecteButton.gameObject);
            }
        }
        
        // 切换场景视频和物品视频
        public void OnVideo(Transform videoImage)
        {
            GameObject trackedImageGameObject = videoImage.parent.parent.parent.gameObject;
            
            imageObjectToNameMap.TryGetValue(trackedImageGameObject, out string spotName);

            if (videoPlayer.clip.name == spotName)
            {
                VideoClip videoClip = Resources.Load<VideoClip>("Videos/"+spotName+"_3d");
                if (videoClip != null)
                {
                    ChangeVideoByClip(videoClip);
                }
            }
            else
            {
                VideoClip videoClip = Resources.Load<VideoClip>("Videos/"+spotName);
                ChangeVideoByClip(videoClip);
            }
            
            
        }

        public void CloseAllCanvasesAndShowBorders()
        {
            foreach (var trackedImageGameObject in imageObjectToNameMap.Keys)
            {
                var canvas = trackedImageGameObject.GetComponentInChildren<Canvas>(true); 
                if (canvas != null)
                {
                    canvas.gameObject.SetActive(false); // 关闭 Canvas
                }
            }
            
            foreach (var item in imageObjectToNameMap.Keys)
            {
                item.SetActive(true);
            }
        }
        
        private void DeactivateAllTrackedImages(GameObject except)
        {
            foreach (var item in imageObjectToNameMap.Keys)
            {
                if (item != except)
                {
                    item.SetActive(false);
                }
            }
        }

        private void SelectAScenicSpot(int id, GameObject trackedGO, GameObject button)
        {
            selectedScenicSpots.Add(id);
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = "取消选择";
            
            ClientControl.UpdateLocalIDs(selectedScenicSpots);
        }
        
        private void DeselectAScenicSpot(int id, GameObject trackedGO, GameObject button)
        {
            selectedScenicSpots.Remove(id); 
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = "选择该景点";
            
            ClientControl.UpdateLocalIDs(selectedScenicSpots);
        }
        
        private void UpdateSelectSpotShow(List<int> idList)
        {
            // 更新选择的景点的按钮
            selectedScenicSpots = new List<int>();
            idList.ForEach(i => selectedScenicSpots.Add(i)); //深拷贝

            foreach (int id in spotID2TrackedGO.Keys) //只更新字典里记录的，没记录的无所谓
            {
                if(selectedScenicSpots.Contains(id))
                {
                    GameObject TrackedGO = spotID2TrackedGO[id];
                    Button button = TrackedGO.GetComponentInChildren<Button>();
                    TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    buttonText.text = "取消选择";
                }
                else
                {
                    GameObject TrackedGO = spotID2TrackedGO[id];
                    Button button = TrackedGO.GetComponentInChildren<Button>();
                    TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    buttonText.text = "选择该景点";
                }
            }

            debugInfo.text = "selectedScenicSpots：";
            foreach (int id in selectedScenicSpots)
            {
                debugInfo.text += id+",";
            }
        }

        
        void LoadScenicSpots()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("Json/ScenicSpotsDetails");
            string jsonText = textAsset.text;
            
            ScenicSpot[] scenicSpotList = FromJson<ScenicSpot>(jsonText);
            foreach (ScenicSpot scenicSpot in scenicSpotList)
            {
                ScenicSpotDictionary.Add(scenicSpot.name, scenicSpot.description);
                // Debug.Log($"Name: {scenicSpot.name}, Description: {scenicSpot.description}")
            }
        }
        
        void ChangeVideoByClip(VideoClip newClip)
        {
            if(videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
    
            videoPlayer.clip = newClip;
            videoPlayer.Play();
        }

    }
}