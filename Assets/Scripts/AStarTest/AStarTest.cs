using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarTest : MonoBehaviour
{
    //左上第一个立方体的位置
    [SerializeField] private int beginX;
    [SerializeField] private int beginY;
    
    //每个立方体间的偏移位置
    [SerializeField] private int offsetX;
    [SerializeField] private int offsetY;

    //地图格子的宽高
    [SerializeField] private int mapWidth = 5;
    [SerializeField] private int mapHeight = 5;

    [SerializeField] private Material red;
    [SerializeField] private Material yellow;
    [SerializeField] private Material green;
    [SerializeField] private Material white;

    //起点的默认值为负
    private Vector2 beginPos = Vector2.right * -1;


    private Dictionary<string, GameObject> cubes = new Dictionary<string, GameObject>();
    private List<AStarNode> path = new List<AStarNode>();


    // Start is called before the first frame update
    void Start()
    {
        AStarMgr.Instance.InitMapInfo(mapWidth, mapHeight);

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                //创建立方体
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = new Vector3(beginX + i * offsetX, beginY + j * offsetY, 0);
                //重命名立方体的名称，存储到字典，便于查找
                obj.name = i + "_" + j;
                cubes.Add(obj.name, obj);

                //判断格子能否通行
                AStarNode node = AStarMgr.Instance.nodes[i, j];
                if(node.nodeType == NodeType.Impassable)
                {
                    obj.GetComponent<MeshRenderer>().material = red;
                }
            }

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //射线检测获取点击到的gameObject信息
            RaycastHit info;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out info, 1000))
            {
                //设置起点和终点
                if (beginPos == Vector2.right * -1) //起点为默认值，就表示没有设置起点
                {
                    //先清除上次寻路的路径
                    if (path != null)
                    {
                        for (int i = 0; i < path.Count; i++)
                        {
                            cubes[path[i].x + "_" + path[i].y].GetComponent<MeshRenderer>().material = white;
                        }
                    }

                    string[] strs = info.collider.gameObject.name.Split('_');
                    beginPos = new Vector2(int.Parse(strs[0]), int.Parse(strs[1]));
                    //起点设置为黄色
                    info.collider.gameObject.GetComponent<MeshRenderer>().material = yellow;
                }
                else
                {
                    string[] strs = info.collider.gameObject.name.Split('_');
                    Vector2 endPos = new Vector2(int.Parse(strs[0]), int.Parse(strs[1]));

                    //寻路
                    path = AStarMgr.Instance.FindPath(beginPos, endPos);
                    //避免死路时，起点的黄色不清除
                    cubes[(int)beginPos.x + "_" + (int)beginPos.y].GetComponent<MeshRenderer>().material = white;

                    //节点列表不为空，就寻路成功
                    if (path != null)
                    {
                        for (int i = 0; i < path.Count; i++)
                        {
                            cubes[path[i].x + "_" + path[i].y].GetComponent<MeshRenderer>().material = green;
                        }
                    }

                    //清除本次寻路设置的起点，设为默认值
                    beginPos = Vector2.right * -1;
                }
            }
        }

        
    }
}
