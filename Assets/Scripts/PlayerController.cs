using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    #region Walking/Running
    [SerializeField]
    private float walkSpeed = 4f;
    [SerializeField]
    private float runSpeed = 7f;
    #endregion
    #region Jumping
    [SerializeField]
    private float gravity = -12f;
    [SerializeField]
    private float jumpHeight = 1f;
    [SerializeField]
    [Range(0, 1)]
    private float airControlPercent;
    #endregion

    [Header("Smoothing")]
    #region Turn Smoothing
    [SerializeField]
    private float turnSmoothTime = 0.2f;
    private float turnSmoothVelocity;
    #endregion
    #region Speed Smoothing
    [SerializeField]
    private float speedSmoothTime = 0.1f;
    private float speedSmoothVelocity;
    #endregion

    [Header("Climbing")]
    #region Climbing
    [SerializeField]
    private float _wallAngleMax;
    [SerializeField]
    private float _groundAngleMax;
    [SerializeField]
    private LayerMask _layerMaskClimb;
    #endregion

    [Header("Heights")]
    [SerializeField]
    private float _overpassHeight;

    [Header("Offsets")]
    [SerializeField]
    private float _climbOriginDown;

    #region Private Properties
    private float currentSpeed;
    private float velocityY;
    private Animator animator;
    private Transform cameraT;
    private CharacterController controller;
    private bool _climbing;
    #endregion
    private void Start()
    {
        animator = GetComponent<Animator>();
        cameraT = Camera.main.transform;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Joystick1Button10);
        Move(input, running);

        HandleJumping();

        //HandleClimbing(running);


        float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f);
        animator.SetFloat("speedPercentage", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
    }

    /*private void HandleClimbing(bool running)
    {
        if (running)
        {
            if (CanClimb)
            {

            }
        }
    }*/

    /*private bool CanClimb()
    {
        bool _downHit;
        bool _forwardHit;
        bool _overpassHit;
        float _climbHeight;
        float _groundAngle;
        float _wallAngle;

        RaycastHit _downRaycastHit;
        RaycastHit _forwardRaycastHit;
        RaycastHit _overpassRaycastHit;

        Vector3 _endPosition;
        Vector3 _forwardDirectionXZ;
        Vector3 _forwardNormalXZ;
    }*/

    void HandleJumping()
    {
        animator.SetBool("isGrounded", controller.isGrounded);

        if (Input.GetAxis("Jump") == 1 && controller.isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            velocityY = jumpVelocity;
        }

        /*RaycastHit hit;

        if(Physics.Raycast(feetRaycast.position, Vector3.down, out hit, Mathf.Infinity))
        {
            Debug.Log(hit.transform.name + " - " + (Vector3.Distance(feetRaycast.position, hit.point)));
        }*/
    }

    void Move(Vector2 input, bool running)
    {
        Vector2 inputDir = input.normalized;
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        }

        
        float targetSpeed = ((running) ? runSpeed : walkSpeed) * input.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        velocityY += Time.deltaTime * gravity;
        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            velocityY = 0f;
        }
    }

    float GetModifiedSmoothTime(float smoothTime)
    {
        if (controller.isGrounded)
        {
            return smoothTime;
        }

        if(airControlPercent == 0)
        {
            return float.MaxValue;
        }
        return smoothTime / airControlPercent;
    }
}
