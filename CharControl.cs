using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharControl : MonoBehaviour {

    /* public bool isGrounded;
    public bool isCrouching;

    private Animator anim;
    private CharacterController controller;

    public string moveAxis = "Vertical";
    public string turnAxis = "Horizontal";

    [Header("Movement stats")]
    public float walkSpeed;
    public float runSpeed;
    public float crouchSpeed;

    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;

    float moveDelay = 0.1f;

    public Camera camera;
    Transform cameraT;

	private void Start () {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        cameraT = camera.transform;
	}
	
	private void FixedUpdate () {

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            if(isCrouching)
            {
                isCrouching = false;
            }
            else
            {
                isCrouching = true;
            }
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        isGrounded = controller.isGrounded;

        float moveInput = Input.GetAxis(moveAxis);
        float turnInput = Input.GetAxis(turnAxis);

        Move(moveInput);
        Turn(turnInput, moveInput);
    }

    private void Move(float moveFloat)
    {
        float targerSpeed;
        if(Input.GetKey(KeyCode.LeftShift) && anim.GetBool("isWalking"))
        {
            targerSpeed = runSpeed;
            anim.SetBool("isRunning", true);
        }
        else if(isCrouching && !anim.GetBool("isRunning"))
        {
            targerSpeed = crouchSpeed;
            anim.SetBool("isCrouching", true);
        }
        else
        {
            targerSpeed = walkSpeed;
            anim.SetBool("isWalking", (moveFloat > 0.1) ? true : false);
            anim.SetBool("isRunning", false);
        }
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targerSpeed, ref speedSmoothVelocity, speedSmoothTime);
        transform.Translate(transform.forward * currentSpeed * moveFloat * Time.deltaTime, Space.World);

    }

    private void Turn(float turnFloat, float moveFloat)
    {
        Vector2 input = new Vector2(turnFloat, moveFloat);
        if (input.normalized != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(turnFloat, moveFloat) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }
    }
    private void Jump()
    {
        if(isGrounded)
        {

        }
    } */

    public float InputX;
    public float InputZ;
    public Vector3 desiredMoveDirection;
    public bool blockRotationPlayer;
    public float desiredRotationSpeed = 0.05f;
    public Animator anim;
    public float Speed;
    public float allowPlayerRotation = 0.1f;
    public Camera cam;
    public CharacterController controller;
    public bool isGrounded;
    [Range(1, 20)]
    public float playerSpeed = 3.5f;
    private float verticalVel;
    private Vector3 moveVector;

    private void Start()
    {
        GameObject.Find(Player.Count.ToString() + "/Camera").GetComponent<Camera>().enabled = true;


        anim = GameObject.Find(Player.Count.ToString()).GetComponent<Animator>();
        cam = GameObject.Find(Player.Count.ToString() + "/Camera").GetComponent<Camera>();
        controller = GameObject.Find(Player.Count.ToString()).GetComponent<CharacterController>();
    }
    private void Update()
    {
        InputMagnitude();

        isGrounded = controller.isGrounded;
        if(isGrounded)
        {
            verticalVel -= 0f;
        }
        else
        {
            verticalVel -= 2f;
        }
        moveVector = new Vector3(0, verticalVel, 0);
        controller.Move(moveVector);
    }

    void PlayerMoveAndRotation()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        var camera = Camera.main;
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        right.Normalize();
        forward.Normalize();

        desiredMoveDirection = forward * InputZ + right * InputX;
        controller.Move(desiredMoveDirection * Speed * playerSpeed * Time.deltaTime);
        if(!blockRotationPlayer)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
        }
    }

    void InputMagnitude()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        anim.SetFloat("InputZ", InputZ, .0f, Time.deltaTime * 2f);
        anim.SetFloat("InputX", InputX, .0f, Time.deltaTime * 2f);

        Speed = new Vector2(InputX, InputZ).sqrMagnitude;

        if(Speed > allowPlayerRotation)
        {
            anim.SetFloat("InputMagnitude", Speed, 0.0f, Time.deltaTime);
            PlayerMoveAndRotation();
        }
        else if(Speed < allowPlayerRotation)
        {
            anim.SetFloat("InputMagnitude", Speed, 0.0f, Time.deltaTime);
        }
    }
}
