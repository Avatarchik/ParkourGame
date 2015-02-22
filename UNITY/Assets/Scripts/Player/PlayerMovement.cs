using UnityEngine;
using System.Collections;
using Rewired;

public class PlayerMovement : MonoBehaviour 
{
    //Public Variables
    public Settings settings = new Settings();
    private PlayerAbilities staminaEndurance;

    public bool isRunning;
    public bool isGrounded;
    public bool isVaulting;

    public Vector3 velocity { private set; get; }
    public float velocityMagnitude { private set; get; }
    public float runTime { private set; get; }

    //Private Variables
    private Player playerInput;

    private Rigidbody rigidbody;
    private Transform transform;
    private CapsuleCollider capsule;
    private PlayerController playerController;

    Vector3 inputDir = Vector3.zero;
    Vector3 groundContactNormal = Vector3.zero;

    private bool shouldRun;

    private void Awake()
    {
        playerInput = Rewired.ReInput.players.GetPlayer(0);
        playerInput.isPlaying = true;

        staminaEndurance = GetComponent<PlayerAbilities>();

        rigidbody = GetComponent<Rigidbody>();
        transform = GetComponent<Transform>();
        capsule = GetComponent<CapsuleCollider>();
        playerController = GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        velocity = rigidbody.velocity;
        velocityMagnitude = velocity.magnitude;

        float jumpPressed = playerInput.GetButtonTimePressed("Jump");
        if (jumpPressed != 0 && jumpPressed < 1f && !isVaulting)
            CheckVaultAvailability();

        if (isGrounded && !isVaulting)
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

            float endurance = EnduranceLevel(settings.walkSpeed, settings.runSpeed);

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

    private float EnduranceLevel(float walkSpeed, float runSpeed)
    {
        float endurance = staminaEndurance.enduranceLevel.Evaluate(runTime);
        return Mathf.Clamp(endurance, walkSpeed, runSpeed);
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

    #region Vaulting

    private void CheckVaultAvailability()
    {
        //Phase 1, check if there is something in front of us
        RaycastHit frontRay;
        if (Physics.Raycast(transform.position, transform.forward, out frontRay, 2))
        {
            RaycastHit ledgeRay;
            if (Physics.Raycast(frontRay.point + (transform.forward * 0.1f) + (Vector3.up * 2), Vector3.down, out ledgeRay, 3))
            {
                RaycastHit groundRay;
                if (Physics.Raycast(frontRay.point + (transform.forward * 0.3f) + (Vector3.up * 2), Vector3.down, out groundRay, 3))
                {
                    isVaulting = true;
                    StartCoroutine(HandleVault(ledgeRay, groundRay.distance));
                }
            }
        }
    }

    private IEnumerator HandleVault(RaycastHit ledgeRay, float groundDis)
    {
        Vector3 startVelocity = rigidbody.velocity;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = ledgeRay.point + new Vector3(0, 1f, 0);

        float startTime = Time.time;
        float lerpSpeed = groundDis > 1.5f ? 3 : groundDis > 1.1f ? 1.2f : 1;

        //Animation
        if (groundDis > 1.5f)
        {
            playerController.cameraAnimation.CrossFade(isRunning ? "FP Camera Vault Low Fast" : "FP Camera Vault Low Slow", 0.05f);
        }

        yield return new WaitForFixedUpdate();

        //Phase 2, Move Player to ledge
        while (Time.time - startTime < 1 / lerpSpeed)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, (Time.time - startTime) * lerpSpeed);
            yield return new WaitForFixedUpdate();
        }

        if (groundDis > 1.2f)//climb and drop
        {
            startVelocity.y = 0;
            rigidbody.velocity = startVelocity;
        }
        else//climb up and pull up
        {
            rigidbody.velocity = Vector3.zero;
        }


        isVaulting = false;
    }

    #endregion

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
