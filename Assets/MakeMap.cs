using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MakeMap : MonoBehaviour
{
    public GameObject room;
    // Start is called before the first frame update

    public int seed = 100;
    public int roomNum = 7;
    int prevSeed = 0;
    List<Door> doors = new List<Door>();
    List<GameObject> rooms=new List<GameObject> ();
    List<procGen> procgen=new List<procGen>();
    void Start()
    {
        haschanged();
    }

    // Update is called once per frame
    void Update()
    {
        haschanged();
        foreach(procGen gen in procgen)
            gen.Render();
        //procgen[0].Change();
        //if (procgen.GetDoors().Count < 2)
            //procgen.Change();
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
                                    Destroy(door.obj);
                                else
                                    Destroy(originDoor.obj);
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
                door.Close();
            }
            foreach (procGen gen in procgen)
                gen.ReGenerate();
        }

    }
    
}
