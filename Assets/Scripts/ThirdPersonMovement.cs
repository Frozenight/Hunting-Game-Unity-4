using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class ThirdPersonMovement : MonoBehaviour
{
    [SerializeField]
    private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField]
    private Transform debugTransform;
    [SerializeField]
    private Transform arrowPrefab;
    [SerializeField]
    private Transform spawnArrowPosition;
    [SerializeField]
    private Image crosshair;
    [SerializeField]
    private Transform armastructure;

    [SerializeField]
    private CinemachineFreeLook aimCamera;

    private Vector3 mouseWorldPosition;

    public CharacterController controller;
    public Transform cam;

    public static bool respawn = false;
    public static bool walking = false;
    private bool running = false;

    private float speed = 6;
    private float runningSpeed = 12;
    private float walkingSpeed = 6;
    private float sprintCrouchSpeed = 5;
    private float crouchingSpeed = 3;
    private float gravity = -50f;
    private int jumpHeight = 10;
    private float respawn_Height = -50f;
    private float groundDistance = 0.8f;

    private float walkingCharacterControllerOffset = 4.8f;
    private float runningCharacterControllerOffset = 4.6f;
    private float prepareForJumpCharacterControllerOffset = 2.4f;
    private float CharacterControllerHeight = 5f;

    public static Vector3 respawn_point = new Vector3(2, 6, 2);
    Vector3 velocity;

    public Transform groundCheck;
    public LayerMask groundMask;

    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    public Transform groundCheckBounce;
    public LayerMask groundMaskBounce;

    private bool canBounce = false;

    private bool isJumping;
    private bool isGrounded;
    private bool isCrouching = false;
    private bool hasBowOut = false;
    public bool isAiming = false;
    public bool canShoot = true;
    public bool isInShootingPhase = false;

    private float jumpCooldown = 0.5f;
    private bool canJump = true;


    public bool IsJumpAvailable
    {
        get { return isJumpAvalable; }
        set
        {
            if (value != isJumpAvalable)
            {
                isJumpAvalable = value;
                if (isJumpAvalable)
                {
                    StartJumpCD();
                }
            }
        }
    }

    private bool isJumpAvalable;

    public Animator m_Animator;

    LookAtAnimationRig obj;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_Animator = gameObject.GetComponent<Animator>();
        transform.rotation = Quaternion.Euler(0, 0, 0);
        obj = GetComponentInChildren<LookAtAnimationRig>();
        // Value has to be changed through script, cinemachine bug
        aimCamera.m_YAxis.Value = 0.7f;
    }

    void Update()
    {
        movement();
    }

    void movement()
    {
        if (Input.GetKeyUp(KeyCode.F))
            bowActions();
        if (Input.GetMouseButtonUp(1))
            aiming();
        aimingUpdate();
        if (Input.GetMouseButtonUp(0))
            shoot();

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        // on Ground
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            m_Animator.SetBool("isGrounded", true);
            m_Animator.SetBool("isJumping", false);
            isJumping = false;
            m_Animator.SetBool("isFalling", false);
            IsJumpAvailable = true;
        }
        // not Touching Ground
        else
        {
            m_Animator.SetBool("isGrounded", false);
            isJumping = false;
            IsJumpAvailable = false;

            if (((isJumping) && velocity.y < 0)|| velocity.y < -2)
            {
                m_Animator.SetBool("isFalling", true);
            }
        }
        // Jump
        if (Input.GetButtonUp("Jump") && isGrounded)
        {
            Jump();
        }
        // Crouch
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded)
        {
            Crouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Stop_Crouch();
        }
        // Respawn
        if (this.transform.position.y < respawn_Height)
        {
            controller.enabled = false;
            transform.position = respawn_point;
            respawn = true;
            controller.enabled = true;
        }
        else
            respawn = false;
        // Bounce
        canBounce = Physics.CheckSphere(groundCheckBounce.position, groundDistance, groundMaskBounce);
        if (canBounce)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2 * -2 * gravity);
        }
        //gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        //speed
        currentSpeed();
        //walk
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            walking = true;
            sprint();
            m_Animator.SetBool("isWalking", true);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            // Restrics player rotation when he is aiming/shooting
            if (isAiming || isInShootingPhase) {}
            else
            {
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            // ---------------------------------------------------
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            walking = false;
            running = false;
            m_Animator.SetBool("isWalking", false);
            m_Animator.SetBool("isRunning", false);
        }
        if (!isGrounded)
            walking = false;
        fixHeightWithBowOut();
    }

    void Crouch()
    {
        m_Animator.SetBool("isCrouching", true);
        isCrouching = true;
    }

    void Stop_Crouch()
    {
        m_Animator.SetBool("isCrouching", false);
        isCrouching = false;
    }

    void fixHeightWithBowOut()
    {
        if (hasBowOut)
            if (isCrouching)
            {
                controller.height = prepareForJumpCharacterControllerOffset;
                controller.center = new Vector3(0, 2.1f, 0);
            }
            else if (running)
                controller.height = runningCharacterControllerOffset;
            else if (walking)
                controller.height = walkingCharacterControllerOffset;
            else
                controller.height = CharacterControllerHeight;
        if (!hasBowOut)
            if (isCrouching)
            {
                controller.height = 4;
                controller.center = new Vector3(0, 2.1f, 0);
            }
            else
                controller.height = CharacterControllerHeight;
    }

    void sprint()
    {
        if (isAiming || isInShootingPhase)
            return;

        if (Input.GetKeyDown("left shift") && isGrounded)
        {
            running = true;
            walking = false;
            m_Animator.SetBool("isRunning", true);
        }
        if (Input.GetKeyUp("left shift"))
        {
            running = false;
            m_Animator.SetBool("isRunning", false);
        }
    }

    void bowActions()
    {
        if (!hasBowOut)
        {
            m_Animator.SetBool("hasBowOut", true);
            hasBowOut = true;
        }
        else if (!isAiming)
        {
            m_Animator.SetBool("hasBowOut", false);
            hasBowOut = false;
        }
    }

    void aiming()
    {
        if (!hasBowOut)
            return;
        if (isInShootingPhase)
            return;

        if (!isAiming)
        {
            isAiming = true;
            m_Animator.SetBool("isAiming", true);
            m_Animator.SetBool("isShooting", false);
            aimCamera.gameObject.SetActive(false);
            aimCamera.transform.SetPositionAndRotation(cam.transform.position, cam.transform.rotation);
            aimCamera.Priority = 20;
            aimCamera.gameObject.SetActive(true);
            crosshair.gameObject.SetActive(true);
        }
        else
        {
            isAiming = false;
            hasBowOut = false;
            m_Animator.SetBool("isAiming", false);
            m_Animator.SetBool("hasBowOut", false);

            aimCamera.Priority = 5;
            crosshair.gameObject.SetActive(false);
        }
    }

    void aimingUpdate()
    {
        mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }

        if (isAiming || isInShootingPhase)
        {
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            obj.disableAiming();
        }
    }

    void shoot()
    {
        if (isAiming && canShoot)
        {
            Vector3 aimDir = (mouseWorldPosition - spawnArrowPosition.position).normalized;
            Instantiate(arrowPrefab, spawnArrowPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
            m_Animator.SetBool("isShooting", true);
            m_Animator.SetBool("isAiming", false);
            isInShootingPhase = true;
            canShoot = false;
            isAiming = false;
            StartCoroutine(GetComponent<shootCooldown>().Cooldown());
        }
    }

    void currentSpeed()
    {
        if (running && isCrouching)
            speed = sprintCrouchSpeed;
        else if (running)
            speed = runningSpeed;
        else if (isCrouching)
            speed = crouchingSpeed;
        else
            speed = walkingSpeed;
    }

    void Jump()
    {
        if (isAiming || isInShootingPhase || !canJump)
            return;

        velocity.y = Mathf.Sqrt(-jumpHeight * gravity);
        m_Animator.SetBool("isJumping", true);
        controller.height = 5;
        controller.center = new Vector3(0, 2.6f, 0);
        isJumping = true;
    }

    void StartJumpCD()
    {
        StartCoroutine(StartCooldown(jumpCooldown));
    }

    public IEnumerator StartCooldown(float cooldown)
    {
        canJump = false;
        yield return new WaitForSeconds(cooldown);
        canJump = true;
    }
}