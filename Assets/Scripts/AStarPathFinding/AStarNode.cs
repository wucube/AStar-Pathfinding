using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 节点类型
/// </summary>
public enum NodeType
{
    Passable,
    Impassable
}

public class AStarNode
{
    //节点坐标
    public int x;
    public int y;

    //寻路消耗
    public float F_Cost;
    public float G_Cost;
    public float H_Cost;

    //父节点
    public AStarNode baseNode;

    public NodeType nodeType;

    public AStarNode(int x, int y,NodeType nodeType)
    {
        this.x = x;
        this.y = y;
        this.nodeType = nodeType;
    }
}
