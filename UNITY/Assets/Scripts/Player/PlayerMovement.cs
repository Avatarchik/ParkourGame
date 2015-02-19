using UnityEngine;
using System.Collections;
using Rewired;

public class PlayerMovement : MonoBehaviour 
{
    //Public Variables
    public Settings settings = new Settings();
    public PlayerAbilities staminaEndurance;

    public bool isRunning;
    public bool isGrounded;

    public Vector3 velocity { private set; get; }
    public float velocityMagnitude { private set; get; }
    public float runTime { private set; get; }

    //Private Variables
    Player playerInput;

    Rigidbody rigidbody;
    Transform transform;
    CapsuleCollider capsule;

    Vector3 inputDir = Vector3.zero;
    Vector3 groundContactNormal = Vector3.zero;

    bool shouldRun;

    void Awake()
    {
        playerInput = Rewired.ReInput.players.GetPlayer(0);
        playerInput.isPlaying = true;

        staminaEndurance = new PlayerAbilities();

        rigidbody = GetComponent<Rigidbody>();
        transform = GetComponent<Transform>();
        capsule = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        velocity = rigidbody.velocity;
        velocityMagnitude = velocity.magnitude;

        if (isGrounded)
        {
            if (!isRunning && !shouldRun)
            {
                float sprintPressed = playerInput.GetButtonTimePressed("Sprint");
                shouldRun = sprintPressed != 0f && sprintPressed < 1f;
            }

            if (shouldRun)
            {
                isRunning = true;
                shouldRun = false;
            }

            inputDir.x = playerInput.GetAxisRaw("Horizontal");
            inputDir.z = playerInput.GetAxisRaw("Vertical");

            if (isRunning && Mathf.Abs(inputDir.z) < 0.5f)
                isRunning = false;

            if (isRunning && Mathf.Abs(inputDir.z) > 0.5f)
                runTime += Time.deltaTime;

            if (!isRunning)
                runTime = 0;

            float endurance = EnduranceLevel();

            inputDir *= isRunning ? Mathf.Clamp(settings.runSpeed * endurance, 0.0f, settings.runSpeed) : settings.walkSpeed;
            inputDir = transform.rotation * inputDir;
            inputDir -= rigidbody.velocity;
            inputDir = Vector3.ClampMagnitude(inputDir, settings.maxVelocityChange);
            inputDir.y = 0;
            rigidbody.AddForce(inputDir, ForceMode.VelocityChange);
        }
        else
        {
            isRunning = false;
        }

        StickToGroundHelper();
        GroundCheck();

    }

    private float EnduranceLevel()
    {
        float endurance = staminaEndurance.enduranceLevel.Evaluate(runTime);
        return endurance;
    }

    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(groundContactNormal, Vector3.up);
        return settings.slopeCurveModifier.Evaluate(angle);
    }

    private void StickToGroundHelper()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo, ((capsule.height / 2f) - capsule.radius) + settings.stickToGroundHelperDistance))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, hitInfo.normal);
            }
        }
    }

    private void GroundCheck()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo, ((capsule.height / 2f) - capsule.radius) + settings.groundCheckDistance))
        {
            isGrounded = true;
            groundContactNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            groundContactNormal = Vector3.up;
        }
    }

    [System.Serializable]
    public class Settings
    {
        public float walkSpeed = 5;
        public float runSpeed = 10;

        public AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

        public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float stickToGroundHelperDistance = 0.5f; // stops the character
        public float maxVelocityChange = 0.25f;
    }
}
