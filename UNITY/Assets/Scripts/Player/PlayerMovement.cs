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

        StickToGroundHelper();
        GroundCheck();

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

            inputDir.x = playerInput.GetAxis("Horizontal");
            inputDir.z = playerInput.GetAxis("Vertical");
            inputDir = Vector3.ClampMagnitude(inputDir, 1);

            if (isRunning && inputDir.z < 0.5f)
                isRunning = false;

            inputDir = transform.rotation * inputDir;

            float endurance = EnduranceLevel(settings.walkSpeed, settings.runSpeed);

            inputDir *= isRunning ? Mathf.Clamp(settings.runSpeed * endurance, 0.0f, settings.runSpeed) : settings.walkSpeed;
            float targetMagnitude = inputDir.magnitude;//Take the current magnitude so that when ProjectOnPlane has a different magnitude, normalize it and multiply

            inputDir = Vector3.ProjectOnPlane(inputDir, groundContactNormal);//Make the input follow the angle of the plane we are standing on

            if (targetMagnitude > 0.25f)//Only normalize and multiply when bigger than X amount because of inprecision
                inputDir = inputDir.normalized * targetMagnitude;

            inputDir -= rigidbody.velocity;

            inputDir = Vector3.ClampMagnitude(inputDir, settings.maxVelocityChange);
            rigidbody.AddForce(inputDir, ForceMode.VelocityChange);

            if (isRunning && inputDir.z > 0.5f)
                runTime += Time.deltaTime;

            if (!isRunning)
                runTime = 0;
        }
        else
        {
            isRunning = false;
        }
    }

    private float EnduranceLevel(float walkSpeed, float runSpeed)
    {
        float endurance = staminaEndurance.enduranceLevel.Evaluate(runTime);
        return endurance;
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
        if (Physics.SphereCast(transform.position + Vector3.up, capsule.radius, Vector3.down, out hitInfo, settings.groundCheckDistance + 1, settings.groundLayer))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 60)
            {
                rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, hitInfo.normal);
            }
        }
    }

    private void GroundCheck()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position + Vector3.up, capsule.radius, Vector3.down, out hitInfo, settings.groundCheckDistance + 1, settings.groundLayer) && Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 60)
        {
            isGrounded = true;
            groundContactNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            groundContactNormal = Vector3.up;
        }

        rigidbody.useGravity = !isGrounded;
    }

    void OnGUI()
    {
        GUILayout.Label("isGrounded: " + isGrounded);
        GUILayout.Label("velocity: " + velocity);
        GUILayout.Label("velocityMagnitude: " + velocityMagnitude);
        GUILayout.Label("inputDir: " + inputDir);
        GUILayout.Label("groundVector: " + groundContactNormal);
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

    #region Crouching

    bool isHandlingCrouch = false;

    public void SetCrouch(bool crouch, bool playAnim, bool direct)
    {
        if (!isHandlingCrouch)
            StartCoroutine(HandleCrouch(crouch, playAnim, direct));
    }

    private IEnumerator HandleCrouch(bool crouch, bool playAnim, bool direct)
    {
        float targetCamHeight = crouch ? 1.1f : 1.65f;
        float targetCapHeight = crouch ? 1.2f : 1.8f;

        isHandlingCrouch = true;
        capsule.height = targetCapHeight;

        if (!direct)
        {
            float startTime = Time.time;
            Vector3 startPos = playerController.cameraHeight.localPosition;

            while ((Time.time - startTime) * 2 < 1)
            {
                yield return new WaitForFixedUpdate();
                playerController.cameraHeight.localPosition = Vector3.Lerp(startPos, new Vector3(0, targetCamHeight, 0), (Time.time - startTime) * 2);
            }
        }

        playerController.cameraHeight.localPosition = new Vector3(0, targetCamHeight, 0);
        isHandlingCrouch = false;
    }

    #endregion

    [System.Serializable]
    public class Settings
    {
        public float walkSpeed = 5;
        public float runSpeed = 10;

        public AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

        public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float stickToGroundHelperDistance = 0.5f; // stops the character
        public float maxVelocityChange = 0.25f;

        public LayerMask groundLayer;
    }
}
