using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;

namespace Test
{
    public class TestForAR : MonoBehaviour
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
        
        public VideoPlayer videoPlayer; // 引用你的VideoPlayer组件
        public GameObject testPrefab;
        private Dictionary<string, string> ScenicSpotDictionary = new Dictionary<string, string>();
        
        private static T[] FromJson<T>(string json) {
            ScenicSpotList<T> wrapper = JsonUtility.FromJson<ScenicSpotList<T>>(json);
            return wrapper.items;
        }

        void Awake()
        {
            LoadScenicSpots();
        }

        void Start()
        {
            var canvas = testPrefab.GetComponentInChildren<Canvas>();
            // canvas.worldCamera = worldSpaceCanvasCamera;

            Transform infoPanel = canvas.transform.Find("Background");
            Transform titleTransform = infoPanel.transform.Find("Title");
            Transform spotDetailTransform = infoPanel.transform.Find("SpotDetail");
            TextMeshPro spotTitle = titleTransform.GetComponent<TextMeshPro>();
            TextMeshPro spotDetail = spotDetailTransform.GetComponent<TextMeshPro>();
            
            Debug.Log(canvas);
            Debug.Log(infoPanel);
            Debug.Log(titleTransform);
            Debug.Log(spotTitle);
            
            spotTitle.text = "123";
            
            ScenicSpotDictionary.TryGetValue("香雾窟", out string detail);
            Debug.Log("拿到介绍："+detail);
            spotDetail.text = detail;
            
            string title = "梯云山馆";
            if (title == "梯云山馆")
            {
                VideoClip videoClip = Resources.Load<VideoClip>("Videos/占位视频");
                ChangeVideoByClip(videoClip);
            }

            if (title == "香雾窟")
            {
                VideoClip videoClip = Resources.Load<VideoClip>("Videos/占位视频-蓝");
                ChangeVideoByClip(videoClip);
            }
        }

        void Update()
        {
            
        }
        
        void LoadScenicSpots()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("Json/ScenicSpotsDetails");
            string jsonText = textAsset.text;
            
            ScenicSpot[] scenicSpotList = FromJson<ScenicSpot>(jsonText);
            foreach (ScenicSpot scenicSpot in scenicSpotList)
            {
                ScenicSpotDictionary.Add(scenicSpot.name, scenicSpot.description);
                Debug.Log($"Name: {scenicSpot.name}, Description: {scenicSpot.description}");
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
