using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridMap : MonoBehaviour
{
    public GameObject cube;
    public Camera cam;
    public bool displayGridGizmos;
    public Transform obstalces;
    //public Transform StartPosition;//This is where the program will start the pathfinding from.
    public LayerMask WallMask;//This is the mask that the program will look for when trying to find obstructions to the path.
    public Vector2 vGridWorldSize;//A vector3 to store the length, width and height of the graph in world units.
    public float fNodeRadius;//This stores how big each square on the graph will be
    public float fDistanceBetweenNodes;//The distance that the squares will spawn from eachother.
    Node[, , ] NodeArray;//The array of nodes that the A Star algorithm uses.
    public List<Node> FinalPath;//The completed path that the red line will be drawn along
    public Node[] finalPathArray;//The array for keeping the final path
    public float [] heights;
    List<float> heightList;
    HashSet <float> heightSet;

    public void FinalPathArray()
    {
        this.finalPathArray = this.FinalPath.ToArray();
    }
    float fNodeDiameter;//Twice the amount of the radius (Set in the start function)
    int iGridSizeX, iGridSizeY, iGridSizeZ;//Size of the Grid in Array units.

    private void Start()//Ran once the program starts
    {
        fNodeDiameter = fNodeRadius * 2;
        //---------read coordinates of the obstacles from a file-------
        int counter = 0;  
        string line;  
        System.IO.StreamReader file =   
            new System.IO.StreamReader(@"E:\Others\Unity\New Unity Project\Assets\MapCoordinates.txt");
            String[] strlist = new String[3];  
        while((line = file.ReadLine()) != null)  
        {   
            counter++;
            String[] spearator = { "(", ",", ")" }; 
            Int32 count = 4; 
            strlist = line.Split(spearator, count, 
                StringSplitOptions.RemoveEmptyEntries); 
            cube.transform.localScale = new Vector3(0.9f,float.Parse(strlist[2]), 0.9f);
            Vector3 zeroZero = transform.position - Vector3.right * vGridWorldSize.x / 2 - Vector3.forward * vGridWorldSize.y / 2;
            Vector3 worldPointObj = zeroZero + Vector3.right * (float.Parse(strlist[0]) * fNodeDiameter + fNodeRadius) + Vector3.forward * (float.Parse(strlist[1]) * fNodeDiameter + fNodeRadius) + Vector3.up*(float.Parse(strlist[2])/2 * fNodeDiameter) ;//Get the world coordinates of the 
            print(line + "\t" + worldPointObj.x + ", " + worldPointObj.y + ", " + worldPointObj.z);
            Instantiate(cube,worldPointObj,Quaternion.identity, obstalces);
            cube.layer = 8;
            cube.transform.position = worldPointObj;
            cube.name = "Obj";
        }  
        file.Close();  
        // ------------------------------

        //---------------------Getting all the GameObjects heights on the map and store them in an array in order
        // to use for the Z paramater of the grid (levels of the grid)----------------------------------------
        heightSet = new HashSet < float > ();
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach(GameObject obj in allObjects){
            if (obj.name == "Camera" || obj.name == "Directional Light" || obj.name == "GameManager" || obj.name == "Obstacles" || obj.name == "Plane")
            {
                continue;
            }
            float h = obj.transform.position.y + obj.transform.localScale.y / 2;
            if(!heightSet.Contains(h)){
                heightSet.Add(h);
            }
            continue;
        }
        heightList = new List<float>(heightSet);
        heightList.Add(0);
        heights = new float [heightList.Count];
        heights = heightList.ToArray();
        print("array count = " + heights.Length);
        Array.Sort(heights);
        for (int i = 0; i < heights.Length; i++)
        {
            print("heights[" + i + "] = " + heights[i]);
        }

        iGridSizeX = Mathf.RoundToInt(vGridWorldSize.x / fNodeDiameter);//Divide the grids world co-ordinates by the diameter to get the size of the graph in array units.
        iGridSizeY = Mathf.RoundToInt(vGridWorldSize.y / fNodeDiameter);//Divide the grids world co-ordinates by the diameter to get the size of the graph in array units.
        iGridSizeZ = Mathf.RoundToInt(heights.Length / fNodeDiameter);//Divide the grids world co-ordinates by the diameter to get the size of the graph in array units.
        
        CreateGrid();//Draw the grid
    }

    void CreateGrid()
    {
        NodeArray = new Node[iGridSizeX, iGridSizeY , iGridSizeZ]; 
        Vector3 bottomLeft = transform.position - Vector3.right * vGridWorldSize.x / 2 - Vector3.forward * vGridWorldSize.y / 2;//Get the real world position of the bottom left of the grid.
        for (int x = 0; x < iGridSizeX; x++)//Loop through the array of nodes.
        {
            for (int y = 0; y < iGridSizeY; y++)//Loop through the array of nodes
            {
                for (int z = 0; z < iGridSizeZ; z++)
                {
                    Vector3 worldPoint = bottomLeft + Vector3.right * (x * fNodeDiameter + fNodeRadius) + Vector3.forward * (y * fNodeDiameter + fNodeRadius) + Vector3.up*(heights[z] * fNodeDiameter); ;//Get the world coordinates of the 
                    //If the node is not being obstructed
                    //Quick collision check against the current node and anything in the world at its position. If it is colliding with an object with a WallMask,
                    //the if statement will return false.
                    bool Wall = Physics.CheckSphere(worldPoint, 0.4f, WallMask);
                    int newBase = 0;
                    int newTop = 0;

                    if (Wall){
                        // Printing the coordinates of the yellow nodes of the grid
                        print("Node["+x+","+y+","+z+"]" +"--> x:" + worldPoint.x + " y:" +worldPoint.y + " z:" +worldPoint.z);
                        //------------get the height of the object that has occupied the Node(cell)-----------------------
                        Vector3 WPoint = worldPoint;
                        Ray ray = new Ray(WPoint + 20*Vector3.up, Vector3.down);//Cast a ray to get where the world coordinate is pointing at
                        RaycastHit hit;//Stores the position where the ray hit.
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, WallMask)){
                            var selection = hit.transform;
                            var rend = selection.GetComponent<Renderer>();
                            if (rend != null)
                            {
                            //rend.material.color = Color.cyan;
                            // Getting the Y position of the obstacle that has occupied the node respectively
                            Vector3 posit = rend.transform.position;
                            // Getting the size of the obstacle that has occupied the node respectively
                            Vector3 objectSize = rend.transform.localScale;
                            //print("WorldPosX: " + posit.x + " WorldPosY: " + posit.y + " WorldPosZ: " + posit.z + " --- Object height: " + objectSize.y);
                            newTop = (int) Math.Round(objectSize.y, MidpointRounding.AwayFromZero);
                            }
                        }
                        //--------------------------------------------------------------------------------------------------
                    }
                    NodeArray[x, y , z] = new Node(Wall, worldPoint, x, y, z, newBase, newTop); 
                }
            }
        }
    }

    //Function that gets the neighboring nodes of the given node.
    public List<Node> GetNeighboringNodes(Node a_NeighborNode)
    {
        List<Node> NeighborList = new List<Node>();//Make a new list of all available neighbors.
        int icheckX;//Variable to check if the XPosition is within range of the node array to avoid out of range errors.
        int icheckY;//Variable to check if the YPosition is within range of the node array to avoid out of range errors.
        int icheckZ;//Variable to check if the ZPosition is within range of the node array to avoid out of range errors.
        //Checking 8 neighbors of each node
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++){

                    if (i==0 && j==0 && k==0)
                        continue;

                    icheckX = a_NeighborNode.iGridX + i;
                    icheckY = a_NeighborNode.iGridY + j;
                    icheckZ = a_NeighborNode.iGridZ + k;
                    if (icheckX >= 0 && icheckX < iGridSizeX){//If the XPosition is in range of the array
                        if (icheckY >= 0 && icheckY < iGridSizeY){ //If the YPosition is in range of the array
                            if (icheckZ >= 0 && icheckZ < iGridSizeZ){ //If the ZPosition is in range of the array
                                NeighborList.Add(NodeArray[icheckX, icheckY, icheckZ]); 
                            }
                        }
                    }                
                }
            }
        }
        return NeighborList;//Return the neighbors list.
    }

    //Gets the closest node to the given world position.
    public Node NodeFromWorldPoint(Vector3 a_vWorldPos)
    { 
        float ixPos = ((a_vWorldPos.x + vGridWorldSize.x / 2) / vGridWorldSize.x);
        float iyPos = ((a_vWorldPos.z + vGridWorldSize.y / 2) / vGridWorldSize.y);
        float izPos = a_vWorldPos.y  / heights.Length;

        ixPos = Mathf.Clamp01(ixPos);
        iyPos = Mathf.Clamp01(iyPos);
        izPos = Mathf.Clamp01(izPos);

        int ix = Mathf.RoundToInt((iGridSizeX - 1) * ixPos);
        int iy = Mathf.RoundToInt((iGridSizeY - 1) * iyPos);
        int iz = Mathf.RoundToInt((iGridSizeZ - 1) * izPos);
        print("iz = " + iz);
        float min = Mathf.Abs(iz - heights[0]);
        int index = 0;
        for (int z = 1; z < iGridSizeZ; z++)
        {
            print("minimum = " + min);
            if (Mathf.Abs(iz - heights[z]) < min)
            {
                min = Mathf.Abs(iz - heights[z]);
                index = z;
            }
        }
        print("index = " + index);
        iz = index;
        return NodeArray[ix, iy, iz]; 
    }

    public void ReadPath() 
    {
        List<Node> path = new List<Node>();//List to hold the path sequentially
        int counter = 0;  
        string line;  
        System.IO.StreamReader file =   
            new System.IO.StreamReader(@"E:\Others\Unity\New Unity Project\Assets\FinalPath.txt");
            String[] strlist = new String[3];  
        while((line = file.ReadLine()) != null)  
        {   
            counter++;
            String[] spearator = { "(", ",", ")" }; 
            Int32 count = 4; 
            strlist = line.Split(spearator, count, 
                StringSplitOptions.RemoveEmptyEntries);

            Node N = NodeArray[Int32.Parse(strlist[0]), Int32.Parse(strlist[1]), Int32.Parse(strlist[2]) ];
            path.Add(N);
        }
        file.Close();
        this.FinalPath = path;  
        this.finalPathArray = this.FinalPath.ToArray();

    }

    //Function that draws the wireframe and grid tiles
    private void OnDrawGizmos()
    {
        if (!displayGridGizmos) { // only show the path when display grid Gizmos is off
			if (FinalPath != null) {
				foreach (Node n in FinalPath) {
					Gizmos.color = Color.red;
					Gizmos.DrawCube(n.vPosition, new Vector3(1, 0.1f, 1) * (fNodeDiameter - fDistanceBetweenNodes));//Draw the node at the position of the node.
				}
			}
		}
		else {
			if (NodeArray != null) {
				foreach (Node n in NodeArray) {
					Gizmos.color = (n.bIsWall)?Color.yellow:Color.white;
					if (FinalPath != null)
						if (FinalPath.Contains(n))
							Gizmos.color = Color.red;
					Gizmos.DrawCube(n.vPosition, new Vector3(1, 0.1f, 1) * (fNodeDiameter - fDistanceBetweenNodes));//Draw the node at the position of the node.
				}
			}
        }
    }
}