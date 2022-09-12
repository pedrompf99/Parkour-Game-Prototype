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
    private LayerMask layerMaskClimb;
    [SerializeField]
    private bool canClimb;
    [SerializeField]
    private float maxHeight;
    #endregion


    #region Private Properties
    private float currentSpeed;
    private float velocityY;
    private Animator animator;
    private Transform cameraT;
    private CharacterController controller;
    private bool isClimbing = false;
    private Vector3 placeToJump = Vector3.one;
    
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
        if(!isClimbing)
            Move(input, running);

        //HandleJumping();

        HandleClimbing();

        float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f);
        animator.SetFloat("speedPercentage", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
    }



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

    void HandleClimbing()
    {
        RaycastHit ray;
        if(Physics.Raycast(transform.position + transform.forward/2 + new Vector3(0, maxHeight, 0), - Vector3.up, out ray, maxHeight, layerMaskClimb))
        {
            canClimb = true;
        } else
        {
            canClimb = false;
        }

        if (canClimb == true)
        {
            if (Input.GetAxis("Jump") == 1 && !isClimbing)
            {
                isClimbing = true;
                placeToJump = ray.point;                
            }
        }

        animator.SetBool("isClimbing", isClimbing);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Step Up"))
        {
            MatchTarget(placeToJump, transform.rotation, AvatarTarget.LeftFoot, new MatchTargetWeightMask(Vector3.one, 0), 0.3f, 0.66f);
        }

        if (isClimbing && !animator.isMatchingTarget)
        {
            isClimbing = false;
        }

    }

    public void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget target, MatchTargetWeightMask weightMask, float normalisedStartTime, float normalisedEndTime)
    {
        if (animator.isMatchingTarget)
            return;

        animator.MatchTarget(matchPosition, matchRotation, target, weightMask, normalisedStartTime, normalisedEndTime);
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
