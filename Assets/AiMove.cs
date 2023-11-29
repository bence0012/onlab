using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Scripting;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

class Pathfind
{
    LinkedList<Cell> Path;

    Cell start;
    Cell end;
    bool found = false;
    int stop = 0;
    public Pathfind(Cell startcell, Cell endcell)
    {
        start = startcell;
        end = endcell;
    }
    public LinkedList<Cell> GetPath()
    {
        if(found) { 
            if (Path != null)
                return Path;
        }
        return null;
    }

    public void FindPath()//, Cell end)
    {
        List<Cell> list = new List<Cell>();
        list.Add(start);
        Dijkstra(list);
        BackTrack();

        
    }

    void BackTrack()
    {
        stop = 0;
        Path=new LinkedList<Cell>();
        
        Cell next = end;
        while (true)
        {
            if (stop == 200) return;
          
            Path.AddFirst(next);
            if (next.value==0)
                break;
            foreach(Cell cell in next.neighbours)
                if (cell.value < next.value)
                {
                    if (cell.value == -1)
                        continue;


                    next = cell;
                    break;
                }
            if (next.nearDoor != null && next.nearDoor.other != null  )
            {
                foreach (Cell cell in next.nearDoor.other.nearCell)
                    if (cell.value < next.value)
                    {
                        if (cell.value == -1)
                            continue;


                        next = cell;
                        break;
                    }
            }
            stop++;
        }
    }

    void Dijkstra(List<Cell> start, int depth = 0, bool troughDoor = false)
    {
        if (found)
            return;
        

        List<Cell> next = new List<Cell>();
        List<Cell> door = new List<Cell>();

        foreach (Cell cell in start)
        {
            if (cell.value != -1)
                continue;
            if(cell.value==-1)
                cell.value = depth;
            if (cell == end)
            {
                found = true;
                return;
            }
            
            foreach (Cell neighbour in cell.neighbours)
            {
                if (neighbour.obj!=null || neighbour.value==int.MaxValue)
                {
                    
                    neighbour.value = int.MaxValue;
                    continue;
                }
                
                if ((neighbour.value == -1 ))
                {
                    next.Add(neighbour);
                   
                }
            }
            if (cell.nearDoor != null && cell.nearDoor.other != null   )
                foreach (Cell neighbour in cell.nearDoor.other.nearCell)
                {
                    if (neighbour.obj != null || neighbour.value == int.MaxValue)
                    {

                        neighbour.value = int.MaxValue;
                        continue;
                    }
                    if (neighbour.value == -1)  
                    {
                        next.Add(neighbour);
                        
                    }
                }
        }
       
        if (next.Count > 0)
            Dijkstra(next, depth + 1);
    }
}

public class AiMove : MonoBehaviour
{
    public Dictionary<Cell, bool> visitedCells = new Dictionary<Cell, bool>();
    public GameObject visited;
    public GameObject toVisit;
    public MakeMap map;
    public int searchDepth;

    [SerializeField]
    Transform goal;

    List<GameObject> targets = new List<GameObject>();

    Cell startcell;

    Thread t;

    List<GameObject> objs = new List<GameObject>();

    bool generated = false;

    Pathfind pathfind = null;

    LinkedList<Cell> path = new LinkedList<Cell>();

    bool started = false;

    int val = 0;

    int pathNum = 0;

    bool tr = true;

    List<GameObject> objects = new List<GameObject>();

    public bool GetStarted()
    {
        return started;
    }

    private void Start()
    {
        if (MakeMap.Instance != null)
            MakeMap.Instance.Add(this);
    }

    public void StartOutside()
    {
        if (MakeMap.Instance != null && startcell == null)
        {
            startcell = MakeMap.Instance.GetCell();
            goal.position = startcell.pos + new Vector3(0, 0.5f, 0);
            transform.position=goal.position + new Vector3(0, 0.5f, 0);
        }
        map.ResetProcGenCells();

        Search(startcell, searchDepth);
        Cell endcell = visitedCells.FirstOrDefault(x => x.Value == false).Key;
        pathfind = new Pathfind(startcell, endcell);
        t = new Thread(new ThreadStart(pathfind.FindPath));
        t.Start();
        startcell = endcell;
        started = true;
    }

    
    void Update()
    {
        if (started)
        {
            if (!t.IsAlive && tr)
            {

                if (pathfind.GetPath() != null)
                {
                    path = pathfind.GetPath();
                }
                Debug.Log(path.Count);
                foreach (GameObject obj in objects)
                    Destroy(obj);
                foreach (Cell cell in path)
                {
                    GameObject obj = Instantiate(toVisit, cell.pos + new Vector3(0, 0.5f, 0), Quaternion.identity);
                    obj.name = cell.value.ToString();
                    objects.Add(obj);
                }
                tr = false;

            }
            if (GetComponent<Rigidbody>().velocity.magnitude < 1.5)
                val += 1;

            

            if (val == 5)
            {
                transform.position = goal.position + new Vector3(0, 0.5f, 0);
                val = 0;
            }
            GetComponent<Rigidbody>().velocity = (goal.position - transform.position) * 5;

            if (!generated && !t.IsAlive)
            {
                foreach (var obj in objs)
                    Destroy(obj);

                foreach (KeyValuePair<Cell, bool> entry in visitedCells)
                {


                    if (entry.Value)
                        objs.Add(Instantiate(visited, entry.Key.pos, Quaternion.identity));
                    else
                        objs.Add(Instantiate(toVisit, entry.Key.pos, Quaternion.identity));

                }


                generated = true;
            }

            if (path != null)
            {


                if (path.Count > pathNum)
                {

                    goal.position = path.ElementAt(pathNum).pos + new Vector3(0, 0.5f, 0);
                }
                if (path.Count - 1 > pathNum && !t.IsAlive)
                {
                    if ((goal.position - transform.position).magnitude < 1f)
                    {
                        pathNum++;
                        val = 0;
                    }
                }
                if (path.Count - 1 == pathNum && !t.IsAlive)
                {
                    t.Abort();
                    map.ResetProcGenCells();
                    Search(path.ElementAt(pathNum), searchDepth);
                    startcell = path.ElementAt(pathNum);
                    Cell endcell = visitedCells.FirstOrDefault(x => x.Value == false).Key;
                    pathfind = new Pathfind(startcell, endcell);
                    t = new Thread(new ThreadStart(pathfind.FindPath));
                    t.Start();
                    tr = true;
                    generated = false;
                    pathNum = 0;
                }
            }
        }

        /*if (goal != null)
        {
         
            RaycastHit hit;
            foreach (GameObject target in targets)
            {
                Physics.Raycast(transform.position, target.transform.position - transform.position, out hit, 200, 3);
                if (Mathf.Abs(Vector3.Angle(transform.forward, target.transform.position - transform.position)) < 45 / 2 && hit.transform.tag == "target")
                {
                    goal.position = target.transform.position;

                }

            }
        }*/
    }

    public void Register(GameObject target)
    {
        targets.Add(target);
    }

    public void Unregister(GameObject target)
    {
        targets.Remove(target);
    }

    private void Search(Cell start, int depth, bool troughDoor = false)
    {
        if (start.obj != null) return;
        if (troughDoor && visitedCells.ContainsKey(start)) return;
        if (depth <= 0)
            if (!visitedCells.ContainsKey(start))
            {
                visitedCells.Add(start, false);
                return;
            }
            else return;
        if (visitedCells.TryGetValue(start, out bool success))
        {
            if (success) return;
            else
                visitedCells[start] = true;
            foreach (Cell cell in start.neighbours)
                Search(cell, depth - 1);

            if (start.nearDoor != null && start.nearDoor.other != null && !troughDoor)
            {
                foreach (Cell cell in start.nearDoor.other.nearCell)
                {
                    if (!visitedCells.ContainsKey(cell))
                        Search(cell, depth - 1, true);
                }
                
            }
            return;
        }

        visitedCells.Add(start, true);

        foreach (Cell cell in start.neighbours)
            Search(cell, depth - 1);

        if (start.nearDoor != null && start.nearDoor.other != null && !troughDoor)
        {
            foreach (Cell cell in start.nearDoor.other.nearCell)
            {
                Search(cell, depth - 1, true);
            }
        }
    }

    void OnApplicationQuit()
    {
        t.Abort();
        GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        GC.Collect();
    }
}


    
