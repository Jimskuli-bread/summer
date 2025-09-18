using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public Camera cam;
    public LayerMask grappleMask;
    public float maxDistance = 40f;
    public float spring = 4.5f;
    public float damper = 7f;
    public float massScale = 4.5f;
    public KeyCode grappleKey = KeyCode.Mouse0;
    public LineRenderer ropeRenderer;

    private Rigidbody rb;
    private SpringJoint joint;
    private Vector3 grapplePoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (ropeRenderer != null)
        {
            ropeRenderer.enabled = false;
            ropeRenderer.positionCount = 2;
            ropeRenderer.startColor = Color.black;
            ropeRenderer.endColor = Color.black;
            ropeRenderer.startWidth = 0.04f;
            ropeRenderer.endWidth = 0.04f;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            if (joint == null)
            {
                Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleMask))
                {
                    grapplePoint = hit.point;
                    joint = gameObject.AddComponent<SpringJoint>();
                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = grapplePoint;

                    float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

                    // The distance grapple will try to keep from grapple point. 
                    joint.maxDistance = distanceFromPoint * 0.8f;
                    joint.minDistance = distanceFromPoint * 0.25f;

                    // Adjust these values for swinging feel
                    joint.spring = spring;
                    joint.damper = damper;
                    joint.massScale = massScale;

                    if (ropeRenderer != null)
                        ropeRenderer.enabled = true;
                }
            }
            else
            {
                ReleaseGrapple();
            }
        }

        // Release with jump
        if (joint != null && Input.GetButtonDown("Jump"))
        {
            ReleaseGrapple();
        }

        // Rope visual update
        if (joint != null && ropeRenderer != null)
        {
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, grapplePoint);
        }
        else if (ropeRenderer != null)
        {
            ropeRenderer.enabled = false;
        }
    }

    void FixedUpdate()
    {
        // Optional: Add extra swing force for more control
        if (joint != null)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 camRight = cam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();
            Vector3 swingDir = (camRight * horizontal + camForward * vertical).normalized;
            rb.AddForce(swingDir * 30f, ForceMode.Acceleration);
        }
    }

    void ReleaseGrapple()
    {
        if (joint != null)
            Destroy(joint);
        if (ropeRenderer != null)
            ropeRenderer.enabled = false;
    }
}