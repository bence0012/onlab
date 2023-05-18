using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;


public enum celltype
{
    unoccupied, north, east, south, west, occuoied
}

public class Cell
{
    public Vector3 pos=new Vector3();
    public celltype kind=celltype.unoccupied;
    public GameObject obj;

    
}

public class Door
{
    public int wallId;
    public bool generate;
    public int whichWall;
    public Vector3 pos;
    public Vector3 rot=new Vector3(1,1,1);
    public Vector3 scale;
    public GameObject obj;
    
    
    
}

public class procGen : MonoBehaviour
{


    public GameObject tile;

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


    List<Door> doors;

    List<Matrix4x4> walls;
    List<Matrix4x4> grounds;

    List<BoxCollider> wallColliders;
    BoxCollider groundCollider;

    // Start is called before the first frame update
    void Start()
    {
        doors = new List<Door>();
        groundCollider = gameObject.AddComponent<BoxCollider>();
        groundCollider.material = physicMaterial;

        wallColliders = new List<BoxCollider>();
        for (int i = 0; i < 8; i++)
        {
            wallColliders.Add(gameObject.AddComponent<BoxCollider>());
            wallColliders[i].material = physicMaterial;

        }


    }

    // Update is called once per frame
    void Update()
    {
       
        hasChanged();

        RenderWalls();
        GenerateGround();
        RenderGrounds();
        
    }

    

    void GenerateWall()
    {
        walls = new List<Matrix4x4>();
        
        numOfWallsX = Mathf.Max(1, (int)(roomLength.x / minLength));
        numOfWallsY = Mathf.Max(1, (int)(roomLength.y / minLength));
        
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
                door.wallId = 0;
                door.pos = new Vector3();
                door.generate = RandomBool();

                doors.Add(door);
                if (door.whichWall < 1)
                    door.wallId = UnityEngine.Random.Range(0, numOfWallsX);
                else
                    door.wallId = UnityEngine.Random.Range(0, numOfWallsY);
                if (!door.generate)
                    door.wallId = -1;
            }


            GenerateWall();
            GenerateDoors();
            CalculateColliders();
            GenerateTiles();
            

            prevSize =roomLength;
        }
    }

    void GenerateDoors()
    {
        foreach (Door i in doors)
        {
            i.obj = GameObject.Instantiate(doorPrefab);
            i.obj.GetComponent<Transform>().position = i.pos;
            Quaternion rot= new Quaternion(0, 0, 0, 1);
            rot.SetLookRotation(i.rot);
            i.obj.GetComponent<Transform>().rotation=rot;
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
                    cells[i, j].kind = celltype.north;
                else if (j == 0)
                    cells[i, j].kind = celltype.south;
                else if (i == countX - 1)
                    cells[i, j].kind = celltype.east;
                else if (i == 0)
                    cells[i, j].kind = celltype.west;

                if (UnityEngine.Random.value>0.5f)
                {
                    cells[i, j].obj = Instantiate(tile);
                    cells[i, j].obj.GetComponent<Transform>().position=cells[i, j].pos;
                    
                }

            }
        }

    }


}
