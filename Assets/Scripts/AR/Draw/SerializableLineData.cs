using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableLineData
{
    public List<SerializableVector3> points;
    public SerializableVector3 parentPosition;

    public SerializableLineData(ARLine arLine)
    {
        points = new List<SerializableVector3>();
        for (int i = 0; i < arLine.LineRenderer.positionCount; i++)
        {
            points.Add(new SerializableVector3(arLine.LineRenderer.GetPosition(i)));
        }

        parentPosition = new SerializableVector3(arLine.ParentPosition);
    }
}

[System.Serializable]
public class LineDataContainer
{
    public List<SerializableLineData> lineDataList = new List<SerializableLineData>();
}

[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}