using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of the player
    public float jumpHeight = 2f; // Height of the jump
    public float gravity = -9.81f; // Gravity force

    private CharacterController controller; // Reference to the CharacterController component
    private Vector3 moveDirection; // Stores the player's movement direction
    private Vector3 velocity; // Tracks vertical velocity (for jumping and gravity)
    private bool isGrounded; // Tracks if the player is on the ground

    public Transform cameraTransform; // Reference to the camera's transform

    void Start()
    {
        // Get the CharacterController component attached to the player
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if the player is grounded
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Reset vertical velocity when grounded
        }

        // Get input from the player (WASD or arrow keys)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Get the forward and right directions relative to the camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Flatten the directions to ignore vertical movement
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Calculate the movement direction relative to the camera
        moveDirection = (forward * vertical + right * horizontal).normalized;

        // Move the player
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Calculate jump velocity
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply vertical velocity to the player
        controller.Move(velocity * Time.deltaTime);
    }
}