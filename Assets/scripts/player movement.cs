using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 14f;
    public float jumpForce = 8f;
    public float airControl = 0.7f;
    public int maxJumps = 2;

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

    // Add at the top for sphere settings
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

    // Track previous states for proper jump reset
    private bool wasGrounded = false;
    private bool wasOnCeiling = false;
    private bool wasWallRunning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // In the Rigidbody component (Inspector), freeze rotation X and Z (but NOT Y) for a capsule!
        // Cursor.lockState = CursorLockMode.Locked; // Removed for mobile
        // Cursor.visible = true; // Ensure cursor is visible
    }

    void Update()
    {
        // --- GROUND & ROOF CHECKS USING GIZMO SPHERE ---
        Vector3 sphereOrigin = transform.position + Vector3.down * 0.5f;
        float sphereRadius = jumpResetSphereRadius;

        RaycastHit hit;
        if (Physics.Raycast(sphereOrigin, Vector3.down, out hit, 20f))
        {
            Vector3 gizmoPos = hit.point;
            Collider playerCollider = GetComponent<Collider>();
            if (playerCollider != null)
            {
                Collider[] overlaps = Physics.OverlapSphere(gizmoPos, sphereRadius);
                foreach (var col in overlaps)
                {
                    if (col == playerCollider)
                    {
                        // Reset jumps only while the player is touching the gizmo sphere
                        jumpCount = 0;
                        break;
                    }
                }
            }
        }

        // --- CEILING CHECK ---
        float ceilingCheckDistance = 1.5f;
        bool isOnCeiling = Physics.Raycast(transform.position, Vector3.up, ceilingCheckDistance);

        // --- ANDROID INPUT ---
        // Replace keyboard/mouse with joystick and button flags
        Vector2 joyInput = joystick != null ? joystick.Direction : Vector2.zero;
        float horizontal = joyInput.x;
        float vertical = joyInput.y;
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();
        Vector3 moveInput = (camForward * vertical + camRight * horizontal).normalized;

        // --- MOVEMENT ---
        float speed = runPressed ? runSpeed : walkSpeed;
        Vector3 targetVel = moveInput * speed;
        Vector3 velocityChange = targetVel - new Vector3(rb.velocity.x, 0, rb.velocity.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, isGrounded ? 20f : 10f);
        rb.AddForce(velocityChange * (isGrounded ? 1f : airControl), ForceMode.Acceleration);

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
        if (isTouchingWall && !isGrounded && moveInput.magnitude > 0.1f && runPressed)
        {
            if (!isWallRunning)
            {
                isWallRunning = true;
                wallRunTimer = wallRunDuration;
                jumpCount = 0; // Reset jumps on wallrun start
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
                jumpCount = 0; // Reset jumps while wallrunning
                wasWallRunning = true;
                return;
            }
        }

        // --- VAULTING DETECTION ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isVaulting)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, camForward, out hit, vaultDistance))
            {
                if (hit.collider.CompareTag("Building") && hit.point.y - transform.position.y < vaultHeight)
                {
                    StartCoroutine(VaultRoutine(hit.point + Vector3.up * 1.3f + camForward * 1.1f));
                    rb.velocity = Vector3.zero;
                    return;
                }
            }
        }

        // --- JUMPING ---
        if (jumpPressed)
        {
            if (isGrounded || isWallRunning || isOnCeiling)
            {
                Vector3 v = rb.velocity;
                v.y = 0;
                rb.velocity = v;
                if (isWallRunning)
                    rb.velocity = wallNormal * wallJumpForce + Vector3.up * jumpForce;
                else
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpCount = 1; // First jump used
            }
            else if (jumpCount < maxJumps)
            {
                Vector3 v = rb.velocity;
                v.y = 0;
                rb.velocity = v;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpCount++; // Second jump used
            }
            jumpPressed = false;
        }

        // --- STORE PREVIOUS STATES ---
        wasGrounded = isGrounded;
        wasOnCeiling = isOnCeiling;
        wasWallRunning = isWallRunning;
    }

    // --- VAULTING COROUTINE ---
    System.Collections.IEnumerator VaultRoutine(Vector3 end)
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

    // --- ANDROID BUTTONS ---
    public SimpleJoystick joystick; // Assign in Inspector
    public bool jumpPressed = false;
    public bool runPressed = false;
    public void OnJumpButton() { jumpPressed = true; }
    public void OnRunButtonDown() { runPressed = true; }
    public void OnRunButtonUp() { runPressed = false; }
}
