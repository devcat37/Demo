using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharControl : MonoBehaviour {


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
