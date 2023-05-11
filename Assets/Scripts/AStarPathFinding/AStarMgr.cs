using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A星寻路管理器
/// </summary>
public class AStarMgr 
{
#region 单例模版类
    private static AStarMgr instance;
    public static AStarMgr Instance
    {
        get
        {
            if (instance == null)
                instance = new AStarMgr();
            return instance;
        }
    }
    private AStarMgr() { }
    
#endregion

    //地图的边界
    private int mapWidth;
    private int mapHeight;

    //地图中所有节点的容器
    public AStarNode[,] nodes;

    //开启列表
    private List<AStarNode> openList = new List<AStarNode>();
    //关闭列表
    private List<AStarNode> closeList = new List<AStarNode>();

    /// <summary>
    /// 初始化地图信息
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    public void InitMapInfo(int mapWidth, int mapHeight)
    {
        //记录地图的宽高
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;

        nodes = new AStarNode[mapWidth, mapHeight];

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                //没有地图数据，就随机生成障碍节点。若有地图数据，应直接读取
                AStarNode node = new AStarNode(i, j, Random.Range(0, 100) < 20 ? NodeType.Impassable : NodeType.Passable);
                nodes[i, j] = node;
            }
        }
    }
    /// <summary>
    /// 寻路方法
    /// </summary>
    /// <param name="startPos">起点</param>
    /// <param name="endPos">终点位置</param>
    /// <returns></returns>
    public List<AStarNode> FindPath(Vector2 startPos,Vector2 endPos)
    {
        //实际开发中，要将传入的坐标点转为所属的格子，再用格子来寻路


        //首先判断传入的两个点是否合法：1.要在地图范围内 2.要非阻挡点
        if (startPos.x < 0 || startPos.x > mapWidth ||
            startPos.y < 0 || startPos.y > mapHeight ||
            endPos.x < 0 || endPos.x > mapWidth ||
            endPos.y<0 || endPos.y>mapHeight)
        {
            Debug.Log("开始或结束点在地图格子范围外");
            return null;
        }
            
        AStarNode startNode = nodes[(int)startPos.x, (int)startPos.y];
        AStarNode endNode = nodes[(int)endPos.x, (int)endPos.y];
        if (startNode.nodeType == NodeType.Impassable || endNode.nodeType == NodeType.Impassable)
        {
            Debug.Log("开始或结束点为阻碍节点");
            return null;
        }

        //清空上一次寻路数据，避免影响本次的寻路计算
        closeList.Clear();
        openList.Clear();
        startNode.baseNode = null;
        startNode.F_Cost = 0;
        startNode.G_Cost = 0;
        startNode.H_Cost = 0;
        closeList.Add(startNode);


        while (true)
        {
            /*
        从起点开始找周围的点，并加入到开启列表中
        左上节点(x-1,y-1); 正上节点(x,y-1); 右上节点(x+1,y-1); 正左节点(x-1,y);正右节点(x+1,y);左下节点(x-1,y+1);右下节点(x+1,y+1);
         */
            FindNearNodesToAddToOpenList(startNode.x - 1, startNode.y - 1, 1.4f, startNode, endNode);
            FindNearNodesToAddToOpenList(startNode.x, startNode.y - 1, 1f, startNode, endNode);
            FindNearNodesToAddToOpenList(startNode.x + 1, startNode.y - 1, 1.4f, startNode, endNode);
            FindNearNodesToAddToOpenList(startNode.x - 1, startNode.y, 1f, startNode, endNode);
            FindNearNodesToAddToOpenList(startNode.x + 1, startNode.y, 1f, startNode, endNode);
            FindNearNodesToAddToOpenList(startNode.x - 1, startNode.y + 1, 1.4f, startNode, endNode);
            FindNearNodesToAddToOpenList(startNode.x, startNode.y + 1, 1f, startNode, endNode);
            FindNearNodesToAddToOpenList(startNode.x + 1, startNode.y + 1, 1.4f, startNode, endNode);


            //开启列表为空，都没有找到终点，就判断为死路
            if (openList.Count == 0)
            {
                Debug.Log("死路");
                return null;
            }

            //选出开启列表中，寻路消耗最小的点
            openList.Sort(SortOpenList);

            //寻路消耗最小的节点放入到关闭列表中，该节点从开启列表中移除
            closeList.Add(openList[0]);
            //找到的消耗最小节点变成新的基础节点
            startNode = openList[0];
            openList.RemoveAt(0);
            //判断该节点是否为终点
            if (startNode == endNode){
                //回溯路径
                List<AStarNode> path = new List<AStarNode>();
                path.Add(endNode);
                while (endNode.baseNode != null)
                {
                    path.Add(endNode.baseNode);
                    endNode = endNode.baseNode;
                }
                //翻转列表
                path.Reverse();

                return path;
            }
        }
       
    }
    /// <summary>
    /// 为开启列表中的节点排序，以F_Cost为基准
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int SortOpenList(AStarNode a, AStarNode b)
    {
        return a.F_Cost >= b.F_Cost ? 1 : -1;
    }

    /// <summary>
    /// 寻找临近节点添加到开启列表中
    /// </summary>
    /// <param name="x">临近节点的坐标x</param>
    /// <param name="y">临近节点的坐标y</param>
    /// <param name="G_Cost">临近节点到基础节点的距离</param>
    /// <param name="baseNode">临近节点的基础节点</param>
    /// <param name="endNode">终点</param>
    private void FindNearNodesToAddToOpenList(int x, int y,float G_Cost,AStarNode baseNode,AStarNode endNode)
    {
        //边界判断
        if (x < 0 || x >= mapWidth ||
           y < 0 || y >= mapHeight)
            return;

        //在范围内则取点
        AStarNode node = nodes[x, y];

        //判断节点是否为阻挡点，是否在开启或关闭列表中。
        if(node ==null || node.nodeType==NodeType.Impassable ||
           closeList.Contains(node))
        {
            return;
        }

        //如果节点已在开启列表中，但本次也找到该节点，则比较节点的两个G值，取较小的G值并更新节点信息 
        if (openList.Contains(node))
        {
            float  G_CostThis = baseNode.G_Cost + G_Cost;
            if (G_CostThis < node.G_Cost)
            {
                node.G_Cost = G_CostThis;
                node.F_Cost = node.G_Cost + node.H_Cost;
                node.baseNode = baseNode;
                return;
            }
            else
                return;
        }

        //计算F值
        node.baseNode = baseNode;
        node.G_Cost = baseNode.G_Cost + G_Cost;//节点到起点距离 = 该节点到基础节点的距离 + 基础节点到起点的距离
        node.H_Cost = Mathf.Abs(endNode.x - node.x) + Mathf.Abs(endNode.y - node.y); //节点到终点的距离，采用曼哈顿距离
        node.F_Cost = node.G_Cost + node.H_Cost;

        openList.Add(node);
    }
}
