using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Transactions;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;


public enum celltype
{
    unoccupied, north, east, south, west, occupied
}


public class Cell
{
    public Vector3 pos = new Vector3();
    public celltype kind = celltype.unoccupied;
    public GameObject obj;
    public float rot = 90;
    public List<Cell> neighbours = new List<Cell>();
    public List<GameObject> objects = new List<GameObject>();
    public bool visited = false;
   

}

public class Door
{
    public int wallId;
    public bool generate;
    public int whichWall;
    public int connectTo;
    public Vector3 pos;
    public Vector3 rot=new Vector3(1,1,1);
    public Vector3 scale;
    public GameObject obj;
    public bool occupied =false;
    
    
}

public class procGen : MonoBehaviour
{

    [Header("Room Size")]
    public Vector2 roomLength;
    public float minLength=0.7f;
    int numOfWallsX;
    int numOfWallsY;
    float wallSize;
    float scaleX;
    float scaleY;

    [Header("Drawables")]

    public List<Mesh> meshes;
    public List<Material> materials;
    public Vector2 groundDividers;

    public PhysicMaterial physicMaterial;

    [Header("Additional elements")]
    public GameObject doorPrefab;


    Vector2 prevSize;
    List<Door> doors = new List<Door>();
    List<Matrix4x4> walls;
    List<Matrix4x4> grounds;
    List<BoxCollider> wallColliders;
    BoxCollider groundCollider;

    [Header("Anywhere")]
    public GameObject vase;
    public GameObject chair1;
    public GameObject chair2;
    public GameObject table1;
    public GameObject table2;
    public GameObject drawer1;
    public GameObject drawer2;

    [Header("Near Walls")]
    public GameObject emptyRack;
    public GameObject boxRack;
    public GameObject serverRack;

    [Header("Number of Objects")]
    public int oneByOneNumber=10;
    public int fiveByFiveNumber=10;
    public int wallObjectNumber=10;
    int oneNum;
    int fiveNum;
    int wallNum;


    List<GameObject> anyObjs;
    List<GameObject> wallObjs;

    Dictionary<GameObject, Action<Cell>> objFuncPair;

   
    void Start()
    {
       
        
    }

    public float GetRadius()
    {
        return (transform.position - new Vector3(roomLength.x / 2, roomLength.y / 2, transform.position.z)).magnitude;
    }
    public List<Vector3> GetPoints()
    {
        List<Vector3> points=new List<Vector3>();
        points.Add(transform.position + new Vector3(roomLength.x / 2, transform.position.y, roomLength.y / 2 ));

        points.Add(transform.position + new Vector3(-roomLength.x / 2, transform.position.y,  -roomLength.y / 2));
        //Instantiate(vase).transform.position = points[0];
        //Instantiate(vase).transform.position = points[1];

        return points;
    }
    public void SetSeed(int seed)
    {
        UnityEngine.Random.InitState(seed);

    }

    public void DestroyAll()
    {
        foreach(Cell cell in cells)
        {
            if(cell.obj!=null)
                Destroy(cell.obj);
        }
        foreach (Door door in doors)
            if (door.generate)
                Destroy(door.obj);
    }

    public void StartOutside()
    {
        numOfWallsX = Mathf.Max(1, (int)(roomLength.x / minLength));
        numOfWallsY = Mathf.Max(1, (int)(roomLength.y / minLength));
        groundCollider = gameObject.AddComponent<BoxCollider>();
        groundCollider.material = physicMaterial;


        wallColliders = new List<BoxCollider>();
        for (int i = 0; i < 8; i++)
        {
            wallColliders.Add(gameObject.AddComponent<BoxCollider>());
            wallColliders[i].material = physicMaterial;
            
        }
        objFuncPair = new Dictionary<GameObject, Action<Cell>>();
        anyObjs = new List<GameObject>();
        anyObjs.Add(vase);
        objFuncPair[vase] = OnebyOneObject;

        anyObjs.Add(chair1);
        objFuncPair[chair1] = OnebyOneObject;

        anyObjs.Add(chair2);
        objFuncPair[chair2] = OnebyOneObject;

        anyObjs.Add(table1);
        objFuncPair[table1] = FivebyFiveObject;

        anyObjs.Add(table2);
        objFuncPair[table2] = FivebyFiveObject;

        anyObjs.Add(drawer1);
        objFuncPair[drawer1] = OnebyOneObject;

        anyObjs.Add(drawer2);
        objFuncPair[drawer2] = OnebyOneObject;


        wallObjs = new List<GameObject>();
        wallObjs.Add(emptyRack);
        objFuncPair[emptyRack] = WallObject2;

        wallObjs.Add(boxRack);
        objFuncPair[boxRack] = WallObject2;

        wallObjs.Add(serverRack);
        objFuncPair[serverRack] = WallObject1;

        wallObjs.Add(vase);
        
    }
    public void Render()
    {
            GenerateWall();
            RenderWalls();
            GenerateGround();
            RenderGrounds();
        
    }
    public void Change()
    {
        
        roomLength = new Vector2(UnityEngine.Random.Range(6, 20), UnityEngine.Random.Range(6, 20));
        numOfWallsX = Mathf.Max(1, (int)(roomLength.x / minLength));
        numOfWallsY = Mathf.Max(1, (int)(roomLength.y / minLength));
        prevSize = new Vector2();
        hasChanged();
        change = true;
        
    }
    public List<Door> GetDoors()
    {
        List<Door> actualDoors=new List<Door>();
        foreach(Door door in doors)
            if(door.generate)
                actualDoors.Add(door);
       

        return actualDoors;
    }
    // Update is called once per frame
    bool change = false;
    void Update()
    {


        
    }

    

    void GenerateWall()
    {
        walls = new List<Matrix4x4>();
        
        
        
        wallSize = meshes[0].bounds.size.x;
        scaleX=(roomLength.x/numOfWallsX)/wallSize;
        scaleY=(roomLength.y/numOfWallsY)/wallSize;

        

        for (int i = 0; i < numOfWallsX; i++)
        {
            if (i == doors[0].wallId && doors[0].generate)
            {
                doors[0].pos = transform.position + new Vector3(-roomLength.x / 2 + wallSize * scaleX / 2 + i * scaleX * wallSize, 0, roomLength.y / 2 + 1.8f);
                doors[0].rot = new Vector3(0, 0, 1);
                doors[0].scale=new Vector3(scaleX*1.01f, 1, 1);
                continue;
            }
            var t=transform.position+new Vector3(-roomLength.x/2+wallSize*scaleX/2+i*scaleX*wallSize, 0,roomLength.y/2);
            var r = transform.rotation;
            var s = new Vector3(scaleX, 1,1);

            var mat = Matrix4x4.TRS(t, r, s);
            walls.Add(mat);
        }
        for (int i = 0; i < numOfWallsX; i++)
        {
            if (i == doors[1].wallId && doors[1].generate)
            {
                doors[1].pos = transform.position + new Vector3(-roomLength.x / 2 + wallSize * scaleX / 2 + i * scaleX * wallSize, 0, -roomLength.y / 2-1.8f);
                doors[1].rot = new Vector3(0, 0, -1);
                doors[1].scale = new Vector3(scaleX * 1.01f, 1, 1);

                continue;
            }
            var t = transform.position + new Vector3(-roomLength.x / 2 + wallSize * scaleX / 2 + i * scaleX * wallSize, 0, -roomLength.y / 2);
            var r = transform.rotation;
            r.SetLookRotation(new Vector3(0,0,-1));
            var s = new Vector3(scaleX, 1, 1);

            var mat = Matrix4x4.TRS(t, r, s);
            walls.Add(mat);
        }
        for (int i = 0; i < numOfWallsY; i++)
        {
            if (i == doors[2].wallId && doors[2].generate)
            {
                doors[2].pos = transform.position + new Vector3(-roomLength.x / 2 - 1.8f, 0, -roomLength.y / 2 + wallSize * scaleY / 2 + i * scaleY * wallSize);
                doors[2].rot = new Vector3(-1, 0, 0);
                doors[2].scale = new Vector3(scaleY*1.01f, 1, 1);

                continue;
            }
            var t = transform.position + new Vector3( -roomLength.x / 2,0, -roomLength.y / 2 + wallSize * scaleY / 2 + i * scaleY * wallSize);
            var r = transform.rotation;
            r.SetLookRotation(new Vector3(-1, 0, 0));
            var s = new Vector3(scaleY, 1, 1);

            var mat = Matrix4x4.TRS(t, r, s);
            walls.Add(mat);
            
        }
        for (int i = 0; i < numOfWallsY; i++)
        {
            if (i == doors[3].wallId && doors[3].generate)
            {
                doors[3].pos = transform.position + new Vector3(roomLength.x / 2 + 1.8f, 0, -roomLength.y / 2 + wallSize * scaleY / 2 + i * scaleY * wallSize);
                doors[3].rot = new Vector3(1, 0, 0);
                doors[3].scale = new Vector3(scaleY*1.01f, 1, 1);

                continue;
            }

            var t = transform.position + new Vector3(roomLength.x / 2, 0, -roomLength.y / 2 + wallSize * scaleY / 2 + i * scaleY * wallSize);
            var r = transform.rotation;
            r.SetLookRotation(new Vector3(1, 0, 0));
            var s = new Vector3(scaleY, 1, 1);

            var mat = Matrix4x4.TRS(t, r, s);
            walls.Add(mat);
        }
        
    }



    void RenderWalls()
    {
        if (walls != null)
        {
            Graphics.DrawMeshInstanced(meshes[0], 0, materials[0], walls.ToArray(), walls.Count);
        }
    }

    void GenerateGround()
    {
        grounds = new List<Matrix4x4>();


        for (int i = 0; i < numOfWallsX; i++)
        {
            for (int j = 0; j < numOfWallsY; j++)
            {
                var t = transform.position + new Vector3(-roomLength.x / 2 + wallSize * scaleX / 2 + i * scaleX * wallSize, -meshes[0].bounds.size.y / 2, -roomLength.y / 2 + wallSize * scaleY / 2 + j * scaleY * wallSize);
                var r = transform.rotation;
                var s = new Vector3(roomLength.x / groundDividers.x / numOfWallsX, 1, roomLength.y / groundDividers.y / numOfWallsY);

                var mat = Matrix4x4.TRS(t, r, s);
                grounds.Add(mat);
            }
        }
    }

    void RenderGrounds()
    {
        if (grounds != null)
        {
            Graphics.DrawMeshInstanced(meshes[1], 0, materials[1], grounds.ToArray(), grounds.Count);
        }
    }

    void CalculateColliders()
    {
        
        PutCollider(wallColliders[0],new Vector3(roomLength.x / 2 + 0.05f, 0, (doors[3].wallId + 1)*roomLength.y/numOfWallsY/2), new Vector3(meshes[0].bounds.size.z, meshes[0].bounds.size.y, roomLength.y / numOfWallsY * (numOfWallsY - doors[3].wallId-1)));
        PutCollider(wallColliders[1],new Vector3(roomLength.x / 2 + 0.05f, 0, (numOfWallsY- doors[3].wallId)*-roomLength.y/numOfWallsY/2), new Vector3(meshes[0].bounds.size.z, meshes[0].bounds.size.y, roomLength.y / numOfWallsY * (doors[3].wallId)));
        PutCollider(wallColliders[2], new Vector3(-roomLength.x / 2 - 0.05f, 0, (doors[2].wallId + 1) * roomLength.y / numOfWallsY / 2), new Vector3(meshes[0].bounds.size.z, meshes[0].bounds.size.y, roomLength.y / numOfWallsY * (numOfWallsY - doors[2].wallId - 1)));
        PutCollider(wallColliders[3], new Vector3(-roomLength.x / 2 - 0.05f, 0, (numOfWallsY - doors[2].wallId) * -roomLength.y / numOfWallsY / 2), new Vector3(meshes[0].bounds.size.z, meshes[0].bounds.size.y, roomLength.y / numOfWallsY * (doors[2].wallId)));
        PutCollider(wallColliders[4], new Vector3((doors[0].wallId + 1) * roomLength.x / numOfWallsX / 2,0,roomLength.y / 2 + 0.05f), new Vector3(roomLength.x / numOfWallsX * (numOfWallsX - doors[0].wallId - 1), meshes[0].bounds.size.y, meshes[0].bounds.size.z));
        PutCollider(wallColliders[5], new Vector3((numOfWallsX - doors[0].wallId) * -roomLength.x / numOfWallsX / 2,0,roomLength.y / 2 + 0.05f), new Vector3(roomLength.x / numOfWallsX * (doors[0].wallId), meshes[0].bounds.size.y, meshes[0].bounds.size.z));
        PutCollider(wallColliders[6], new Vector3((doors[1].wallId + 1) * roomLength.x / numOfWallsX / 2, 0, -roomLength.y / 2 - 0.05f), new Vector3(roomLength.x / numOfWallsX * (numOfWallsX - doors[1].wallId - 1), meshes[0].bounds.size.y, meshes[0].bounds.size.z));
        PutCollider(wallColliders[7], new Vector3((numOfWallsX - doors[1].wallId) * -roomLength.x / numOfWallsX / 2, 0, -roomLength.y / 2 - 0.05f), new Vector3(roomLength.x / numOfWallsX * (doors[1].wallId), meshes[0].bounds.size.y, meshes[0].bounds.size.z));

        groundCollider.center = new Vector3(0, -meshes[0].bounds.size.y/2-0.01f, 0);
        groundCollider.size=new Vector3(roomLength.x, meshes[1].bounds.size.y, roomLength.y);
    }


    void PutCollider(BoxCollider wallCollider, Vector3 center, Vector3 size)
    {
        
        wallCollider.center = center;
        wallCollider.size = size;
    }

    bool RandomBool()
    {
        
        if(UnityEngine.Random.value > 0.5f)
            return true;
        return false;
    }

    void hasChanged()
    {
        if (prevSize != roomLength)
        {
            oneNum = oneByOneNumber;
            fiveNum = fiveByFiveNumber;
            wallNum = wallObjectNumber;



            for (int i = 0; i < countX; i++)
                for (int j = 0; j < countY; j++)
                    if (cells[i, j].obj != null)
                        GameObject.Destroy(cells[i, j].obj);
            foreach (Door i in doors) { 
                if (i.obj != null)
                    Destroy(i.obj);
            }
            doors.Clear();
            for (int i = 0; i < 4; i++)
            {
                Door door = new Door();
                door.whichWall = i;
                switch (i)
                {
                    case 0: door.connectTo = 1; break;
                    case 1: door.connectTo = 0; break;
                    case 2: door.connectTo = 3; break;
                    case 3: door.connectTo = 2; break;

                }
                door.wallId = 0;
                door.pos = new Vector3();
                door.generate = RandomBool();
                if (!door.generate)
                    door.wallId = -1;
                else
                {
                    if (door.whichWall < 2)
                        door.wallId = UnityEngine.Random.Range(0, numOfWallsX);
                    else
                        door.wallId = UnityEngine.Random.Range(0, numOfWallsY);
                }

            
            doors.Add(door);
            }


            GenerateWall();
            GenerateDoors();
            CalculateColliders();
            GenerateTiles();
            foreach(Cell cell in cells)
                FillCell(cell);
            

            prevSize =roomLength;
        }
    
    }

    void GenerateDoors()
    {
        foreach (Door i in doors)
        {
            
                i.obj = GameObject.Instantiate(doorPrefab);
                i.obj.GetComponent<Transform>().position = i.pos;
                i.obj.transform.parent = this.transform;
                Quaternion rot = new Quaternion(0, 0, 0, 1);
                rot.SetLookRotation(i.rot);
                i.obj.GetComponent<Transform>().rotation = rot;
                i.obj.GetComponent<Transform>().localScale = i.scale;
            
        }
    }

    Cell[,] cells;
    int countX;
    int countY;
    void GenerateTiles()
    {

        countX = Mathf.FloorToInt(roomLength.x);
        countY = Mathf.FloorToInt(roomLength.y);
         
        cells = new Cell[countX, countY];
        for (int i= 0; i < countX; i++)
        {
            for(int j=0; j < countY; j++)
            {
                cells[i,j] = new Cell();
                cells[i,j].pos = transform.position + new Vector3(roomLength.x / 2 - 0.5f - (i * 1), -meshes[0].bounds.size.y / 2 - 0.01f, roomLength.y / 2 - 0.5f-(j*1));
                
                if (j == countY - 1)
                {
                    if (i == countX - 1 || i == 0)
                    {
                        cells[i, j].kind = celltype.occupied;
                        continue;
                    }
                    cells[i, j].kind = celltype.north;
                    cells[i, j].objects = new List<GameObject>(wallObjs);
                    cells[i, j].rot = 90;
                }
                else if (j == 0)
                {
                    if (i == countX - 1 || i == 0)
                    {
                        cells[i, j].kind = celltype.occupied;
                        continue;
                    }
                    cells[i, j].kind = celltype.south;
                    cells[i, j].objects = new List<GameObject>(wallObjs);
                    cells[i, j].rot = -90;

                }
                else if (i == countX - 1)
                {
                   
                    cells[i, j].kind = celltype.east;
                    cells[i, j].objects = new List<GameObject>(wallObjs);
                    cells[i, j].rot = 180;


                }
                else if (i == 0)
                {
                    
                    cells[i, j].kind = celltype.west;
                    cells[i, j].objects = new List<GameObject>(wallObjs);
                    cells[i, j].rot = 0;

                }
                else
                    cells[i, j].objects = new List<GameObject>(anyObjs);

                for (int k = 0; k < 4; k++)
                {
                    if ((cells[i, j].pos - doors[k].obj.transform.position).magnitude < 3.5f)
                    {
                        cells[i, j].kind = celltype.occupied;
                      
                    }
                }

            }
        }
        MakeGraph();
    }


    void MakeGraph()
    {
        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        int neigX = x + j;
                        int neigY = y + i;

                        if (neigX < 0 || neigX >= countX || neigY < 0 || neigY >= countY ||(j==0 && i==0))
                        {
                            continue;
                        }
                        cells[x, y].neighbours.Add(cells[x + j, y + i]);
                    }
                }
            }
        }
    }

    public int noObj = 100;
    void FillCell(Cell fill)
    {
        switch (fill.kind)
        {
            case celltype.occupied:
                return;
            case celltype.unoccupied:
                int num=UnityEngine.Random.Range(0, fill.objects.Count+noObj);
            
                if (num >= fill.objects.Count)
                {
                    
                    fill.kind = celltype.occupied;
                    break;
                }
                else
                {
                    
                    
                    fill.obj=fill.objects[num];

                    objFuncPair[fill.objects[num]](fill);
                    fill.kind=celltype.occupied;
                    
                }
                break;
            default:
                int num2 = UnityEngine.Random.Range(0, fill.objects.Count + noObj);

                if (num2 >= fill.objects.Count)
                {
                    

                    fill.kind = celltype.occupied;
                    break;
                }
                else
                {

                    

                    fill.obj=fill.objects[num2];
                    
                    objFuncPair[fill.objects[num2]](fill);
                    fill.kind = celltype.occupied;

                }
                break;
                
        }


    }

    void occupyCells(Cell cell, int depth)
    {
        
        if (depth == 0 || cell.kind==celltype.occupied)
            return;
        cell.kind = celltype.occupied;
        foreach(Cell neighbour in cell.neighbours)
        {
            occupyCells(neighbour, depth-1  );
        }

    }
    int counter = 0;
    int counter2 = 0;
    void RemovePossibleObjects(Cell cell, List<GameObject> objects, int depth)
    {
        if (cell.visited)
            return;
        if (depth == 0) { 
            foreach (GameObject obj in objects)
                cell.objects.Remove(obj);
            cell.visited = false;
            return; 
        }
        
        cell.visited=true;
        foreach(Cell neighbour in cell.neighbours)
            RemovePossibleObjects(neighbour, objects, depth-1);
        cell.visited = false;

    }

    void OnebyOneObject(Cell cell)
    {
        if (oneNum == 0)
        {
            cell.obj = null;
            return;
        }
        cell.obj = Instantiate(cell.obj);
        cell.obj.GetComponent<Transform>().position = cell.pos + new Vector3(0, 0.05f, 0);
        cell.obj.transform.parent = this.transform;
        cell.rot *= UnityEngine.Random.Range(0, 4);

        cell.obj.transform.rotation = Quaternion.Euler(0, cell.rot,0);
        occupyCells(cell, 1);
        List<GameObject> objects = new List<GameObject>();
        objects.Add(table1);
        objects.Add(table2);
        objects.Add(emptyRack);
        objects.Add(boxRack);
        objects.Add(serverRack);
        RemovePossibleObjects(cell, objects, 1);
        oneNum--;
    }
    void WallObject1(Cell cell)
    {
        if (wallNum == 0)
        {
            cell.obj = null;
            return;
        }
        cell.obj = Instantiate(cell.obj);
        cell.obj.GetComponent<Transform>().position = cell.pos + new Vector3(0, 0.05f, 0);
        cell.obj.transform.parent = this.transform;

        cell.obj.transform.rotation = Quaternion.Euler(0, cell.rot, 0);


        occupyCells(cell, 2);
        List<GameObject> objects = new List<GameObject>();
        objects.Add(table1);
        objects.Add(table2);
        
        RemovePossibleObjects(cell, objects, 3);
        objects.Clear();
        objects.Add(emptyRack);
        objects.Add(boxRack);
        RemovePossibleObjects(cell, objects, 2);
        objects.Clear();
        objects.Add(serverRack);
        RemovePossibleObjects(cell, objects, 1);

        wallNum--;
    }
    void WallObject2(Cell cell)
    {
        if (wallNum == 0)
        {
            cell.obj = null;
            return;
        }
        cell.obj = Instantiate(cell.obj);
        cell.obj.GetComponent<Transform>().position = cell.pos + new Vector3(0, 0.05f, 0);
        cell.obj.transform.parent = this.transform;

        cell.obj.transform.rotation = Quaternion.Euler(0, cell.rot, 0);


        occupyCells(cell, 2);
        List<GameObject> objects = new List<GameObject>();
        objects.Add(table1);
        objects.Add(table2);

        RemovePossibleObjects(cell, objects, 3);
        objects.Clear();
        objects.Add(emptyRack);
        objects.Add(boxRack);
        objects.Add(serverRack);
        RemovePossibleObjects(cell, objects, 2);
        wallNum--;
    }
    void FivebyFiveObject(Cell cell)
    {
        if (fiveNum == 0)
        {
            cell.obj = null;
            return;
        }
        cell.obj = Instantiate(cell.obj);
        cell.obj.GetComponent<Transform>().position = cell.pos + new Vector3(0, 0.05f, 0);
        cell.obj.transform.parent = this.transform;

        cell.rot *= UnityEngine.Random.Range(0, 4);

        cell.obj.transform.rotation = Quaternion.Euler(0, cell.rot, 0);

        occupyCells(cell, 5);
        fiveNum--;
        
    }
}
