using System;
using System.Collections.Generic;
using System.IO;
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
        
        private static T[] FromJson<T>(string json) {
            ScenicSpotList<T> wrapper = JsonUtility.FromJson<ScenicSpotList<T>>(json);
            return wrapper.items;
        }

        void Awake()
        {
            debugInfo.text = "AWAKE NOW\n";
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
            LoadScenicSpots();
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        void UpdateInfo(ARTrackedImage trackedImage)
        {
            debugInfo.text = "This will be show\n";
            // Set canvas camera
            var canvas = trackedImage.GetComponentInChildren<Canvas>();
            canvas.worldCamera = worldSpaceCanvasCamera;

            Transform infoPanel = canvas.transform.Find("Background");
            Transform titleTransform = infoPanel.transform.Find("Title");
            Transform spotDetailTransform = infoPanel.transform.Find("SpotDetail");
            TextMeshPro title = titleTransform.GetComponent<TextMeshPro>();
            TextMeshPro spotDetail = spotDetailTransform.GetComponent<TextMeshPro>();

            debugInfo.text = "Debug Info!!!\n";
            debugInfo.text += "infoPanel："+infoPanel+"\n";
            debugInfo.text += "titleTransform："+titleTransform+"\n";
            debugInfo.text += "title text：" + title.text;
            
            // 更新景点名称
            title.text = trackedImage.referenceImage.name;
            //更新景点介绍
            ScenicSpotDictionary.TryGetValue(name, out string detail);
            spotDetail.text = detail;

            if (title.text == "梯云山馆")
            {
                VideoClip videoClip = Resources.Load<VideoClip>("Videos/占位视频");
                ChangeVideoByClip(videoClip);
            }

            if (title.text == "香雾窟")
            {
                VideoClip videoClip = Resources.Load<VideoClip>("Videos/占位视频-蓝");
                ChangeVideoByClip(videoClip);
            }
            


            // Disable the visual plane if it is not being tracked
            if (trackedImage.trackingState != TrackingState.None)
            {
                canvas.gameObject.SetActive(true);

                // The image extents is only valid when the image is being tracked
                trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);
            }
            else
            {
                canvas.gameObject.SetActive(false);
            }
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

                UpdateInfo(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
                UpdateInfo(trackedImage);
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
            debugInfo.text = "Json Done\n";
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