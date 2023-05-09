using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class movement : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed=7;
    public float sprintSpeed=10;
    float moveSpeed=10;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Slopes")]
    public float maxAngle;
    private RaycastHit slopeHit;
    private bool exitSlope;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orient;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Animator animator;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;
        readyToJump = true;
    }

    // Update is called once per frame
    void Update()
    {
        grounded=Physics.Raycast(transform.position, Vector3.down, playerHeight*0.5f+0.2f,whatIsGround);
        if(grounded)
            animator.SetBool("isGrounded",true);
        else
            animator.SetBool("isGrounded", false);



        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(KeyCode.Space)&& readyToJump && grounded)
        {

            
            animator.SetBool("isJumping",true);
            animator.SetBool("isGrounded", false);
            readyToJump=false;
            exitSlope = true;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        SpeedControl();

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        moveDirection = orient.forward * verticalInput + orient.right * horizontalInput;

        
        if (new Vector2(rb.velocity.x, rb.velocity.z).magnitude < 0.1)
            animator.SetBool("isMoving", false);
        else
            animator.SetBool("isMoving", true);



        if (grounded && Input.GetKey(KeyCode.LeftShift))
            moveSpeed=sprintSpeed;
        else
            moveSpeed=normalSpeed;

        if (OnSlope()&& !exitSlope)
        {
            rb.AddForce(GetSlopeMoveDir() * moveSpeed * 20f, ForceMode.Force);

            if(rb.velocity.y >0)
                rb.AddForce(Vector3.down*80f,ForceMode.Force);
        }
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed*10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed*airMultiplier*10f, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 velo = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (velo.magnitude > moveSpeed)
            {
                Vector3 limit = velo.normalized * moveSpeed;
                rb.velocity = new Vector3(limit.x, rb.velocity.y, limit.z);
            }
        }
    }
    
    private void Jump()
    {
        rb.velocity=new Vector3(rb.velocity.x, 0, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }


    private void ResetJump()
    {
        readyToJump=true;
        exitSlope=false;
        animator.SetBool("isJumping", false);

    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit, playerHeight * 0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle <maxAngle && angle!=0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDir()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}

