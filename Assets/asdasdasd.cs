using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class asdasdasd : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject a;
    public GameObject b;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        print(Physics.Linecast(a.transform.position, b.transform.position, out RaycastHit asd,1,QueryTriggerInteraction.Ignore));
        //print(asd.transform.name);
    }
}
