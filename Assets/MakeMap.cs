using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.AI.Navigation;


public class MakeMap : MonoBehaviour
{

    public static MakeMap Instance { get; private set; } = null;

    List<AiMove> moves = new List<AiMove>();

    public void Add(AiMove move) {  moves.Add(move); }

    public GameObject room;
    // Start is called before the first frame update

    public GameObject box;

    public int seed = 100;
    public int roomNum = 7;
    int prevSeed = 0;
    List<Door> doors = new List<Door>();
    List<GameObject> rooms=new List<GameObject> ();
    List<procGen> procgen=new List<procGen>();

    NavMeshSurface navMesh;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        navMesh=transform.GetComponent<NavMeshSurface> ();
        haschanged();
        
        foreach (Door door in doors)
            if (door.other != null)
            {
                foreach (Cell cell in door.other.nearCell)
                    door.nearCell.AddRange(door.other.nearCell);
                foreach (Cell cell in door.nearCell)
                    door.other.nearCell.AddRange(door.nearCell);
            }
        
    }

    // Update is called once per frame
    void Update()
    {
        haschanged();
        foreach(procGen gen in procgen)
            gen.Render();

        foreach (var move in moves)
            if(move.GetStarted() == false)
                move.StartOutside();
    }

    public Cell GetCell()
    {
        return procgen[0].GetCell();
    }

    
    int trynum = 100;
    void haschanged()
    {
        if (prevSeed != seed)
        {
            prevSeed = seed;

            if (rooms.Count!=0)
                foreach (GameObject room in rooms)
                {
                    room.GetComponent<procGen>().DestroyAll();

                    Destroy(room);
                }
            procgen.Clear();
            rooms.Clear();
            doors.Clear();
            rooms.Add(Instantiate(room));
            procgen.Add(rooms[0].GetComponent<procGen>());
            UnityEngine.Random.InitState(seed);
            procgen[0].SetSeed(seed);
            procgen[0].StartOutside();
            procgen[0].Change();
            while (procgen[0].GetDoors().Count < 2)
            {
                procgen[0].Change();


            }
            doors.AddRange(procgen[0].GetDoors());
            int i = 1;
            while (doors[i-1]!=null)
            {
                if (i == roomNum)
                    break;
                rooms.Add(Instantiate(room));
                procgen.Add(rooms[i].GetComponent<procGen>());

                procgen[i].SetSeed(seed+i);
                procgen[i].StartOutside();
                
                
                bool found=false;
                bool badGen = false;
                int j = 0;
                while (!found)
                {
                    procgen[i].transform.position = new Vector3(0, 0, 0);
                    procgen[i].Change();
                    
                    foreach (Door door in procgen[i].GetDoors())
                    {
                        foreach (Door originDoor in doors)
                        {
                            if (door.whichWall == originDoor.connectTo && !originDoor.occupied)
                            {
                                rooms[i].transform.position = originDoor.obj.transform.position - door.obj.transform.localPosition;
                                badGen = false;
                                foreach(GameObject room in rooms)
                                {
                                    
                                    List<Vector3> l = room.GetComponent<procGen>().GetPoints();
                                    List<Vector3> r = procgen[i].GetPoints();

                                    badGen = l[0].x > r[0].x && l[1].x < r[0].x && l[0].z > r[0].z && l[1].z < r[0].z && l[1].x < r[0].x && l[0].x > r[0].x && l[1].z < r[0].z && l[0].z > r[0].z || l[0].x > r[1].x && l[1].x < r[1].x && l[0].z > r[1].z && l[1].z < r[1].z && l[1].x < r[1].x && l[0].x > r[1].x && l[1].z < r[1].z && l[0].z > r[1].z||l[0].x > r[0].x && l[1].x < r[0].x && l[0].z > r[0].z && l[1].z < r[0].z && l[1].x < r[0].x && l[0].x > r[0].x && l[1].z < r[0].z && l[0].z > r[0].z || l[0].x > r[1].x && l[1].x < r[1].x && l[0].z > r[1].z && l[1].z < r[1].z && l[1].x < r[1].x && l[0].x > r[1].x && l[1].z < r[1].z && l[0].z > r[1].z ||l[0].x > r[2].x && l[1].x < r[2].x && l[0].z > r[2].z && l[1].z < r[2].z && l[1].x < r[2].x && l[0].x > r[2].x && l[1].z < r[2].z && l[0].z > r[2].z || l[0].x > r[3].x && l[1].x < r[3].x && l[0].z > r[3].z && l[1].z < r[3].z && l[1].x < r[3].x && l[0].x > r[3].x && l[1].z < r[3].z && l[0].z > r[3].z|| r[0].x > l[0].x && r[1].x < l[0].x && r[0].z > l[0].z && r[1].z < l[0].z && r[1].x < l[0].x && r[0].x > l[0].x && r[1].z < l[0].z && r[0].z > l[0].z || r[0].x > l[1].x && r[1].x < l[1].x && r[0].z > l[1].z && r[1].z < l[1].z && r[1].x < l[1].x && r[0].x > l[1].x && r[1].z < l[1].z && r[0].z > l[1].z || r[0].x > l[0].x && r[1].x < l[0].x && r[0].z > l[0].z && r[1].z < l[0].z && r[1].x < l[0].x && r[0].x > l[0].x && r[1].z < l[0].z && r[0].z > l[0].z || r[0].x > l[1].x && r[1].x < l[1].x && r[0].z > l[1].z && r[1].z < l[1].z && r[1].x < l[1].x && r[0].x > l[1].x && r[1].z < l[1].z && r[0].z > l[1].z || r[0].x > l[2].x && r[1].x < l[2].x && r[0].z > l[2].z && r[1].z < l[2].z && r[1].x < l[2].x && r[0].x > l[2].x && r[1].z < l[2].z && r[0].z > l[2].z || r[0].x > l[3].x && r[1].x < l[3].x && r[0].z > l[3].z && r[1].z < l[3].z && r[1].x < l[3].x && r[0].x > l[3].x && r[1].z < l[3].z && r[0].z > l[3].z;
                                    if(badGen)
                                        break;
                                }
                                if (badGen) continue;
                                
                                found = true;
                                door.occupied = true;
                                if (originDoor.obj.transform.localScale.x > door.obj.transform.localScale.x)
                                {
                                    Destroy(door.obj);
                                    originDoor.other = door;
                                    door.other = originDoor;
                                    
                                    
                                    originDoor.win = true;
                                    door.generate = true;
                                }
                                else
                                {
                                    Destroy(originDoor.obj);
                                    door.win = true;
                                    door.other = originDoor;
                                    originDoor.other = door;
                                    
                                    originDoor.generate = true;
                                }
                                originDoor.occupied = true;

                                break;
                            }

                        }
                        
                        if (found)
                            break;
                    }
                    j++;
                    if (j == trynum)
                        break;
                }
                if(j== trynum )
                {
                    procgen[i].DestroyAll();
                    Destroy(rooms[i]);
                    procgen.RemoveAt(i);
                    rooms.RemoveAt(i);
                    return;
                }


                if(found)
                    doors.AddRange(procgen[i].GetDoors());
                i++;
            }
            foreach(Door door in doors)
            {
                door.Close(false);
            }
            foreach (procGen gen in procgen)
                gen.ReGenerate();
            foreach (procGen gen in procgen)
            {
                gen.CalcCellPos();
            }
        }
        
    }

    public void ResetProcGenCells()
    {
        foreach(var gen in procgen)
            gen.ResetCellValues();
    }


}
