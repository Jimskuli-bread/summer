using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 14f;
    public float jumpForce = 8f;
    public float airControl = 0.7f;
    public int maxJumps = 2;

    [Header("Mobile Smoothing")]
    public float joystickDeadzone = 0.12f;
    public float acceleration = 12f;
    public float deceleration = 16f;
    public float coyoteTime = 0.15f; // Time after leaving ground to still allow jump
    public float jumpBufferTime = 0.15f; // Time to buffer jump input before landing

    [Header("Parkour Settings")]
    public float wallJumpForce = 14f;
    public float wallRunUpwardSpeed = 7f;
    public float wallRunDuration = 2.2f;
    public float wallDetectionDistance = 0.8f;
    public float wallRunGravity = 1.2f;
    public float vaultHeight = 1.6f;
    public float vaultDistance = 1.5f;

    [Header("References")]
    public Transform cameraTransform;
    public SimpleJoystick joystick; // Assign in Inspector
    public Button jumpButton; // Assign in Inspector
    public Button grappleButton; // Assign in Inspector

    [Header("Ground Check")]
    public float jumpResetSphereRadius = 0.3f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallRunning;
    private bool isVaulting;
    private float wallRunTimer;
    private Vector3 wallNormal;
    private int jumpCount = 0;
    private Vector3 vaultStart;
    private Vector3 vaultEnd;
    private float vaultTimer;
    // Smoothing and jump helpers
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private Vector3 currentVelocity = Vector3.zero;

    // State tracking
    private bool wasGrounded = false;
    private bool wasOnCeiling = false;
    private bool wasWallRunning = false;

    public bool jumpPressed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (jumpButton != null)
            jumpButton.onClick.AddListener(OnJumpButton);
        if (grappleButton != null)
            grappleButton.onClick.AddListener(OnGrappleButton);
    }

    void Update()
    {
        // --- GROUND CHECK ---
        Vector3 sphereOrigin = transform.position + Vector3.down * 0.5f;
        float sphereRadius = jumpResetSphereRadius;
        RaycastHit hit;
        isGrounded = false;
        if (Physics.Raycast(sphereOrigin, Vector3.down, out hit, 20f))
        {
            Collider playerCollider = GetComponent<Collider>();
            if (playerCollider != null)
            {
                Collider[] overlaps = Physics.OverlapSphere(hit.point, sphereRadius);
                foreach (var col in overlaps)
                {
                    if (col == playerCollider)
                    {
                        jumpCount = 0;
                        isGrounded = true;
                        break;
                    }
                }
            }
        }

        // --- CEILING CHECK ---
        float ceilingCheckDistance = 1.5f;
        bool isOnCeiling = Physics.Raycast(transform.position, Vector3.up, ceilingCheckDistance);

        // --- JOYSTICK INPUT ---
        Vector3 moveInput = Vector3.zero;
        if (joystick != null && joystick.Direction.magnitude > joystickDeadzone)
        {
            Vector2 joyInput = joystick.Direction;
            Vector3 camForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 camRight = cameraTransform != null ? cameraTransform.right : Vector3.right;
            camRight.y = 0f;
            camRight.Normalize();
            moveInput = (camForward * joyInput.y + camRight * joyInput.x).normalized;
        }
        // --- SMOOTH ACCELERATION ---
        float speed = runSpeed;
        Vector3 targetVel = moveInput * speed;
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float accel = (moveInput.magnitude > 0.01f) ? acceleration : deceleration;
        currentVelocity = Vector3.MoveTowards(flatVel, targetVel, accel * Time.deltaTime);
        rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);

        // --- VAULTING ---
        if (isVaulting)
            return;

        // --- WALL DETECTION ---
        isTouchingWall = false;
        wallNormal = Vector3.zero;
        RaycastHit wallHit;
        if (Physics.Raycast(transform.position, transform.right, out wallHit, wallDetectionDistance))
        {
            if (wallHit.collider.CompareTag("Building"))
            {
                isTouchingWall = true;
                wallNormal = wallHit.normal;
            }
        }
        else if (Physics.Raycast(transform.position, -transform.right, out wallHit, wallDetectionDistance))
        {
            if (wallHit.collider.CompareTag("Building"))
            {
                isTouchingWall = true;
                wallNormal = wallHit.normal;
            }
        }

        // --- WALLRUN START/STOP ---
        if (isTouchingWall && !isGrounded && moveInput.magnitude > 0.1f)
        {
            if (!isWallRunning)
            {
                isWallRunning = true;
                wallRunTimer = wallRunDuration;
                jumpCount = 0;
            }
        }
        else
        {
            isWallRunning = false;
        }

        // --- WALLRUNNING ---
        if (isWallRunning)
        {
            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0f)
            {
                isWallRunning = false;
            }
            else
            {
                Vector3 alongWall = Vector3.Cross(wallNormal, Vector3.up);
                if (Vector3.Dot(alongWall, moveInput) < 0)
                    alongWall = -alongWall;
                Vector3 wallRunDir = alongWall.normalized * speed * 1.1f;
                Vector3 desiredVelocity = wallRunDir + Vector3.up * wallRunUpwardSpeed;
                rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, Time.deltaTime * 8f);
                rb.velocity += Vector3.down * wallRunGravity * Time.deltaTime;
                jumpCount = 0;
                wasWallRunning = true;
                return;
            }
        }

        // --- VAULTING DETECTION ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isVaulting)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, moveInput, out hit, vaultDistance))
            {
                if (hit.collider.CompareTag("Building") && hit.point.y - transform.position.y < vaultHeight)
                {
                    StartCoroutine(VaultRoutine(hit.point + Vector3.up * 1.3f + moveInput * 1.1f));
                    rb.velocity = Vector3.zero;
                    return;
                }
            }
        }

        // --- COYOTE TIME & JUMP BUFFER ---
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpPressed)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // --- JUMPING ---
        if (jumpBufferTimer > 0f && (coyoteTimer > 0f || isWallRunning || isOnCeiling))
        {
            Vector3 v = rb.velocity;
            v.y = 0;
            rb.velocity = v;
            if (isWallRunning)
                rb.velocity = wallNormal * wallJumpForce + Vector3.up * jumpForce;
            else
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount = 1;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
        else if (jumpBufferTimer > 0f && jumpCount < maxJumps)
        {
            Vector3 v = rb.velocity;
            v.y = 0;
            rb.velocity = v;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
            jumpBufferTimer = 0f;
        }
        jumpPressed = false;

        // --- STORE PREVIOUS STATES ---
        wasGrounded = isGrounded;
        wasOnCeiling = isOnCeiling;
        wasWallRunning = isWallRunning;
    }

    // --- VAULTING COROUTINE ---
    private System.Collections.IEnumerator VaultRoutine(Vector3 end)
    {
        isVaulting = true;
        vaultStart = transform.position;
        vaultEnd = end;
        vaultTimer = 0f;
        float duration = 0.4f;
        while (vaultTimer < 1f)
        {
            vaultTimer += Time.deltaTime / duration;
            Vector3 vaultPos = Vector3.Lerp(vaultStart, vaultEnd, Mathf.SmoothStep(0, 1, vaultTimer));
            rb.MovePosition(vaultPos);
            yield return null;
        }
        isVaulting = false;
    }

    // --- GRAPPLE BUTTON ---
    public void OnGrappleButton() { TryGrapple(); }

    // --- JUMP BUTTON ---
    public void OnJumpButton() { jumpPressed = true; }

    // --- GRAPPLE LOGIC ---
    private void TryGrapple()
    {
        if (isTouchingWall && !isGrounded)
        {
            isWallRunning = true;
            wallRunTimer = wallRunDuration;
            jumpCount = 0;
        }
    }

    void OnDrawGizmos()
    {
        Vector3 sphereOrigin = transform.position + Vector3.down * 0.5f;
        float sphereRadius = jumpResetSphereRadius;

        // Raycast down to find what is below the player (roof, ground, etc.)
        RaycastHit hit;
        if (Physics.Raycast(sphereOrigin, Vector3.down, out hit, 20f))
        {
            // If the hit collider is tagged "Building", treat it as a roof (City layer)
            if (hit.collider.CompareTag("Building"))
            {
                Gizmos.color = Color.red;
#if UNITY_EDITOR
                UnityEditor.Handles.Label(hit.point + Vector3.right * 0.2f, "<City>");
#endif
            }
            else
            {
                Gizmos.color = Color.green;
#if UNITY_EDITOR
                UnityEditor.Handles.Label(hit.point + Vector3.right * 0.2f, "<Default>");
#endif
            }

            Gizmos.DrawSphere(hit.point, sphereRadius);
        }
    }
}
