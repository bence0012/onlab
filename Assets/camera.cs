using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class camera : MonoBehaviour
{
    public float senX=1;
    public float senY=1;

    public Transform orient;

    float xRot;
    float yRot;

    Rigidbody rb;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * senX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * senY;

        yRot+=mouseX;
        xRot-=mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        transform.rotation=Quaternion.Euler(0,yRot,0);
        orient.rotation = Quaternion.Euler(xRot, yRot, 0);

        Vector2 body = new Vector2(rb.velocity.x, rb.velocity.z);
        Vector2 cam = new Vector2(orient.forward.x, orient.forward.z);
        if(body.magnitude>0.1)
            animator.SetFloat("movementOrientation", Vector2.SignedAngle(cam.normalized, body.normalized));
        
    }

    private void OnMouseDrag()
    {
          
    }
}
