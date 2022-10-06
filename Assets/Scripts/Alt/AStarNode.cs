using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AStarNode
{
    public Vector2Int gridPosition;

    public List<AStarNode> neighbours = new List<AStarNode>();

    public bool isObstacle = false;
    public bool isCostCalculated = false;

    public int gCost_DistanceFromStart = 0;
    public int hCost_DistanceFromGoal = 0;
    public int fCost_Total = 0;
    public int pickOrder = 0;

    public AStarNode(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }
}

