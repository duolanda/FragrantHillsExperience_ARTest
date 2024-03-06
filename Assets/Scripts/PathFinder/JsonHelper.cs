using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScenicSpotList<T> {
    public T[] items;
}

public static class JsonHelper {
    public static string ToJson<T>(T[] array, bool prettyPrint = false) {
        ScenicSpotList<T> wrapper = new ScenicSpotList<T> { items = array };
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    public static T[] FromJson<T>(string json) {
        ScenicSpotList<T> wrapper = JsonUtility.FromJson<ScenicSpotList<T>>(json);
        return wrapper.items;
    }
}