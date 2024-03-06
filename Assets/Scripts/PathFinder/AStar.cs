using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStar {
    public static List<ScenicSpot> FindPath(ScenicSpot startSpot, ScenicSpot targetSpot) {
        List<ScenicSpot> openSet = new List<ScenicSpot>();
        HashSet<ScenicSpot> closedSet = new HashSet<ScenicSpot>();
        openSet.Add(startSpot);

        while (openSet.Count > 0) {
            ScenicSpot currentSpot = openSet[0];
            for (int i = 1; i < openSet.Count; i++) {
                if (openSet[i].FCost < currentSpot.FCost || (openSet[i].FCost == currentSpot.FCost && openSet[i].hCost < currentSpot.hCost)) {
                    currentSpot = openSet[i];
                }
            }

            openSet.Remove(currentSpot);
            closedSet.Add(currentSpot);

            if (currentSpot == targetSpot) {
                return RetracePath(startSpot, targetSpot);
            }

            foreach (ScenicSpot neighbor in currentSpot.neighbors) {
                if (closedSet.Contains(neighbor)) {
                    continue;
                }

                float newMovementCostToNeighbor = currentSpot.gCost + Vector3.Distance(currentSpot.position, neighbor.position);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = Vector3.Distance(neighbor.position, targetSpot.position);
                    neighbor.parent = currentSpot;

                    if (!openSet.Contains(neighbor)) {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<ScenicSpot>(); // 路径未找到
    }

    private static List<ScenicSpot> RetracePath(ScenicSpot startSpot, ScenicSpot endSpot) {
        List<ScenicSpot> path = new List<ScenicSpot>();
        ScenicSpot currentSpot = endSpot;

        while (currentSpot != startSpot) {
            path.Add(currentSpot);
            currentSpot = currentSpot.parent;
        }
        path.Reverse();
        return path;
    }
}
