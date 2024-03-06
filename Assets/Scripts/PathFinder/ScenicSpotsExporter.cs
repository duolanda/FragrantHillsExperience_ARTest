using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ScenicSpotsExporter : MonoBehaviour {
    void Start() {
        ExportScenicSpots();
    }

    void ExportScenicSpots() {
        var scenicSpotsParent = GameObject.Find("Map/ScenicSpots");
        List<ScenicSpot> scenicSpots = new List<ScenicSpot>();

        int id = 0;
        foreach (Transform child in scenicSpotsParent.transform) {
            ScenicSpot spot = new ScenicSpot(id++, child.name, child.position);
            scenicSpots.Add(spot);
        }

        string json = JsonHelper.ToJson<ScenicSpot>(scenicSpots.ToArray(), true);
        File.WriteAllText(Application.dataPath + "/Json/ScenicSpots.json", json);
        Debug.Log("Exported Scenic Spots to JSON");
    }
}