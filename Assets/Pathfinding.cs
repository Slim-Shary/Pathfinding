using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding : MonoBehaviour {

    GridMap GridReference;//For referencing the grid class
    public Transform StartPosition;//Starting position to pathfind from
    public Transform TargetPosition;//Target position to pathfind to
    public bool navigate = true;
    public bool pathFlag;
    public float speed = 1.0f;
    public float step = 0;
    public float timer = 0f;

    private void Awake()//When the program starts
    {
        GridReference = GetComponent<GridMap>();//Get a reference to the game manager
    }
    private void Start() 
    {
        if(pathFlag){
            GridReference.ReadPath();
        }
        else{
        FindPath(StartPosition.position, TargetPosition.position);//Find a path to the goal
        }
    }
    private void Update()
    {
        //FindPath(StartPosition.position, TargetPosition.position);//used for moving the target object
        if (navigate)
        {
            timer += Time.deltaTime;
            step += Time.deltaTime * speed;
            //Node start = GridReference.finalPathArray[0];
            Node current = GridReference.finalPathArray[(int)step];
            StartPosition.position = Vector3.MoveTowards(StartPosition.position, current.vPosition, step);
            // Debug.Log("Start Position= " + StartPosition.position + " , Target position= " + TargetPosition.position);
            if (current == GridReference.finalPathArray[GridReference.finalPathArray.Length - 1]) //Vector3.Distance(StartPosition.transform.position, TargetPosition.transform.position) < 0.1f
            {
                print("Number of steps to the target is: " + GridReference.finalPathArray.Length + "\n END!");
                navigate = false;
                print("time for navigation: " + timer);
                step = (int) step;
            }
        }
    }

    void FindPath(Vector3 a_StartPos, Vector3 a_TargetPos)
    {
        print("StartWorldPosX: " + a_StartPos.x + " StartWorldPosY: " + a_StartPos.z + " StartWorldPosZ: " + a_StartPos.y);
        print("TaregetWorldPosX: " + a_TargetPos.x + " TaregetWorldPosY: " + a_TargetPos.z + " TaregetWorldPosZ: " + a_TargetPos.y + "\n");
        
        Node StartNode = GridReference.NodeFromWorldPoint(a_StartPos);//Gets the node closest to the starting world position
        Node TargetNode = GridReference.NodeFromWorldPoint(a_TargetPos);//Gets the node closest to the target world position
        StartNode.iGridZ = 0;

        print("StartX: " + StartNode.iGridX + " StartY: " + StartNode.iGridY + " StartZ: " + StartNode.iGridZ);
        print("TaregetX: " + TargetNode.iGridX + " TaregetY: " + TargetNode.iGridY + " TaregetZ: " + TargetNode.iGridZ);
        List<Node> OpenList = new List<Node>();//List of nodes for the open list
        HashSet<Node> ClosedList = new HashSet<Node>();//Hashset of nodes for the closed list

        OpenList.Add(StartNode);//Add the starting node to the open list to begin the program

        while(OpenList.Count > 0)//Whilst there is something in the open list
        {
            Node CurrentNode = OpenList[0];//Create a node and set it to the first item in the open list
            for(int i = 1; i < OpenList.Count; i++)//Loop through the open list starting from the second object
            {
                if (OpenList[i].FCost < CurrentNode.FCost || OpenList[i].FCost == CurrentNode.FCost)//If the f cost of that object is less than or equal to the f cost of the current node
                {
                    if (OpenList[i].ihCost < CurrentNode.ihCost)
                    {
                        CurrentNode = OpenList[i];//Set the current node to that object   
                    }
                }
            }
            OpenList.Remove(CurrentNode);//Remove that from the open list
            ClosedList.Add(CurrentNode);//And add it to the closed list

            if (CurrentNode == TargetNode )//If the current node is the same as the target node
            {
                Debug.Log("CurrentX: " + CurrentNode.iGridX + " CurrentY: " + CurrentNode.iGridY + " CurrentZ: " + CurrentNode.iGridZ);
                Debug.Log("TaregetX: " + TargetNode.iGridX + " TaregetY: " + TargetNode.iGridY + " TaregetZ: " + TargetNode.iGridZ);
                GetFinalPath(StartNode, TargetNode);
                print("Path found!");
                break;//Calculate the final path
            }

            foreach (Node NeighborNode in GridReference.GetNeighboringNodes(CurrentNode))//Loop through each neighbor of the current node
            {
                if (NeighborNode.bIsWall || ClosedList.Contains(NeighborNode))//If the neighbor is a wall or has already been checked
                {
                    continue;//Skip it
                }
                int MoveCost = CurrentNode.igCost + GetDistance(CurrentNode, NeighborNode, 2);//Get the G cost of neighbor

                if (MoveCost < NeighborNode.igCost || !OpenList.Contains(NeighborNode))//If the g cost is greater than the g cost or it is not in the open list
                {
                    NeighborNode.igCost = MoveCost;//Set the g cost to the g cost that was calculated 
                    NeighborNode.ihCost = GetDistance(NeighborNode, TargetNode, 2);//Set the h cost
                    NeighborNode.ParentNode = CurrentNode;//Set the parent of the node for retracing steps

                    if(!OpenList.Contains(NeighborNode))//If the neighbor is not in the openlist
                    {
                        OpenList.Add(NeighborNode);//Add it to the list
                    }
                }
            }
        }
    }



    void GetFinalPath(Node a_StartingNode, Node a_EndNode)
    {
        List<Node> FinalPath = new List<Node>();//List to hold the path sequentially 
        Node CurrentNode = a_EndNode;//Node to store the current node being checked

        while(CurrentNode != a_StartingNode)//While loop to work through each node going through the parents to the beginning of the path
        {
            FinalPath.Add(CurrentNode);//Add that node to the final path
            CurrentNode = CurrentNode.ParentNode;//Move onto its parent node
        }

        FinalPath.Reverse();//Reverse the path to get the correct order

        GridReference.FinalPath = FinalPath;//Set the final path
        GridReference.FinalPathArray();
    }

    int GetDistance(Node a_nodeA, Node a_nodeB, int key)
    {
        int ix = Mathf.Abs(a_nodeA.iGridX - a_nodeB.iGridX);//x1-x2
        int iy = Mathf.Abs(a_nodeA.iGridY - a_nodeB.iGridY);//y1-y2
        int iz = Mathf.Abs(a_nodeA.iGridZ - a_nodeB.iGridZ);//z1-z2
        int cost = 0;
        if (key == 0)//Manhattan
            return ix + iy + iz; 
        if (key == 1)//Euclidean
            return (int) ( Math.Sqrt(Math.Pow(a_nodeA.iGridX - a_nodeB.iGridX, 2) + Math.Pow(a_nodeA.iGridY - a_nodeB.iGridY, 2) + Math.Pow(a_nodeA.iGridZ - a_nodeB.iGridZ, 2) ) ); //
        if (key == 2)//Diagonal Distance
            {
                if (ix < iy && ix < iz)
                {
                    cost += ix * 17;
                    iy -= ix;
                    ix = iz - ix;
                }
                else if (iy < iz)
                {
                    cost += iy * 17;
                    ix -= iy;
                    iy = iz - iy;
                }
                else
                {
                    cost += iz * 17;
                    ix -= iz;
                    iy -= iz;
                }
                
                // remaining distance 2d in dX and dY
                if (ix < iy)
                {
                    cost += ix * 14;
                    ix = iy - ix;
                }
                else
                {
                    cost += iy * 14;
                    ix -= iy;
                }
                
                // remaining lateral distance in dX
                return cost + ix * 10;
            }   
        return -1;
    }
}
