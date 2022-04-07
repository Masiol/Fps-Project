using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPlayer : MonoBehaviour
{
    #region - VARIABLES -
    
    [HideInInspector]
    public Vector2 input_Movement;
    [HideInInspector]
    Coroutine fireCoroutine;
    private InputSystem inputSystem;
    public WeaponController weaponController;

    [Header("References")]
    public CharacterController characterController;
    public Transform feet;
    public Transform playerCamera;
    public Animator animator;
    public GunGlowing gunGlowing;

    [Header("Player Movement")]
    public float currentSpeed;
    public float crouchMovementSpeed = 3f;
    public float walkingMovementSpeed = 6f;
    public float sprintMovementSpeed = 10f;
    private bool isWalking;
    public bool isSprinting;

    [Header("Mouse Settings")]
    [SerializeField] private float sensitivityX;
    [SerializeField] private float sensitivityY;
    [SerializeField] private float minY = -60f;
    [SerializeField] private float maxY = 60f;
    public Vector2 input_View; 
    private float lookX;
    private float lookY;

    [Header("Player Gravity")] 
    [SerializeField] private float gravityAmout = -15f;
    [SerializeField] private float jumpHeight = 3f;
    private float isGroundedRadius = 0.1f;
    private Vector3 verticalVelocity = Vector3.zero;
    public LayerMask groundMask;
    private bool playerGrounded;
    [HideInInspector]
    public bool playerJump;
    #endregion

    #region - AWAKE -
    private void Awake()
    {
        //Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Walking Speed
        currentSpeed = walkingMovementSpeed;

        //Movement
        inputSystem = new InputSystem();
        inputSystem.PlayerControls.Movement.performed += max => input_Movement = max.ReadValue<Vector2>();
        inputSystem.PlayerControls.Jump.performed += max => Jump();

        inputSystem.PlayerControls.Sprint.performed += max => Sprinting();
        inputSystem.PlayerControls.SprinReleased.performed += max => StopSprinting();

        //MouseXY
        inputSystem.PlayerControls.Mouse.performed += max => input_View = max.ReadValue<Vector2>();

        //Shooting
        inputSystem.PlayerControls.ChangeShooting.performed += max => weaponController.FullSemi();

        inputSystem.PlayerControls.Shooting.started += max => StartFire();
        inputSystem.PlayerControls.Shooting.canceled += max => StopFire();

        //Scoping
        inputSystem.PlayerControls.Scoping.performed += max => weaponController.Scoping();
        inputSystem.PlayerControls.StopScoping.performed += max => weaponController.StopScoping();


    }
    #endregion

    #region - UPDATE -
    private void Update()
    {
        //MouseXY
        MouseControl();

        //isGrounded
        playerGrounded = Physics.CheckSphere(feet.position, isGroundedRadius, groundMask);
        if(playerGrounded)
        {
            verticalVelocity.y = 0;
        }

        //Movement
        Vector3 MovementVelocity = (transform.right * input_Movement.x + transform.forward * input_Movement.y) * currentSpeed;
        characterController.Move(MovementVelocity * Time.deltaTime);

        if (characterController.velocity.magnitude > 0.1f)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        //Jump
        if (playerJump)
        {
            if (playerGrounded)
            {
                verticalVelocity.y = Mathf.Sqrt(-2f * jumpHeight * gravityAmout);
            }
            else
            {
                playerJump = false;
            }
            if (weaponController.isReloading)
            {
                CancelReloading();
            }
            if(weaponController.isScoping)
            {
                weaponController.StopScoping();
                weaponController.isScoping = false;
                
            }
        }

        //Gravity
        verticalVelocity.y += gravityAmout * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);

        //Ammo
        if (weaponController.currentAmmo <= 0)
        {
            weaponController.currentAmmo = 0;
            weaponController.isEmpty = true;
        }
        if(weaponController.isEmpty)
        {
            StopCoroutine(fireCoroutine);
        }

        //Scoping
        if (weaponController.isScoping)
        {
            animator.SetBool("Walking", false);
            animator.SetTrigger("toIdle");
            StopSprinting();
        }
        if (!weaponController.isScoping)
        {
            animator.SetBool("Walking", isWalking);
        }
    }
    #endregion

    #region - SHOOTING -
    private void StartFire()
    {
        if (!weaponController.isEmpty && !weaponController.isReloading && !isSprinting)
        {
            fireCoroutine = StartCoroutine(weaponController.RapidFire());
        }
        if(weaponController.isEmpty)
        {
            weaponController._audioSource.clip = weaponController.audioClips[3];
            weaponController._audioSource.Play();
        }
    }
    private void StopFire()
    {
        if (fireCoroutine != null)
            gunGlowing.startGlowing = false;
            StopCoroutine(fireCoroutine);
        return;
    }

    #endregion 

    #region - SPRINT - 
    private void Sprinting()
    {
        if (isWalking && !weaponController.isReloading)
        {
            CancelReloading();
            isWalking = false;
            isSprinting = true;
            animator.SetBool("Sprinting", isSprinting);
            currentSpeed = sprintMovementSpeed;
            if(weaponController.isScoping)
            {
                weaponController.isScoping = false;
                weaponController.StopScoping();
            }
        }
    }
    #endregion

    #region - STOP SPRINTING - 
    private void StopSprinting()
    {
        animator.SetBool("Sprinting", false);
        isSprinting = false;
        currentSpeed = walkingMovementSpeed;

        if(characterController.velocity.magnitude > 0.1f)
        {
            animator.SetTrigger("toWalk");
        }
        else
        {
            animator.SetTrigger("toIdle");
        }
        
    }
    #endregion

    #region - MOUSE CONTROL - 
    private void MouseControl()
    {
        lookX += input_View.x * sensitivityX * Time.deltaTime;
        lookY -= input_View.y * sensitivityY * Time.deltaTime;

        lookY = Mathf.Clamp(lookY, minY, maxY);
        this.transform.localEulerAngles = new Vector3(0, lookX, 0);
        playerCamera.localEulerAngles = new Vector3(lookY, 0, 0);
    }
    #endregion

    #region - ON ENABLE / DISABLE - 
    private void OnEnable()
    {
        inputSystem.Enable();
    }
    private void OnDestroy()
    {
        inputSystem.Disable();
    }
    #endregion

    #region - JUMP -
    private void Jump()
    {
        playerJump = true;
    }
    #endregion

    #region - ON DRAW GIZMOS - 
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(feet.position, isGroundedRadius);
    }
    #endregion

    #region - Reloading - 

    private void ChangeShooting()
    {
        weaponController.automaticShoot = !weaponController.automaticShoot;
    }
    public void Reloading()
    {
        animator.ResetTrigger("toIdle");
        animator.ResetTrigger("toWalk");
        animator.SetTrigger("Recoil");
    }
    public void CancelReloading()
    {
        animator.ResetTrigger("toIdle");
        animator.ResetTrigger("Recoil");
        animator.SetTrigger("toIdle");
        weaponController.recoilTime = 0;
        weaponController.isReloading = false;
        weaponController.recoilProgressBar.gameObject.SetActive(false);
    }
    #endregion
}