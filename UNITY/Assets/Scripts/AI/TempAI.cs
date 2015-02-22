using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class TempAI : MonoBehaviour 
{

    private Transform target;
    private NavMeshAgent navMesh;
    private Rigidbody enemyPhysics;
    private bool canJump;
    private OffMeshLinkData link;
    private Vector3 jumpVector;
    private Vector3 enemyVelocity;

    [Range(0.0f, 10.0f)]
    public float jumpDistance = 5.0f;
    [Range(0.0f, 10.0f)]
    public float jumpHeight = 5.0f;
	
    private void Awake()
    {
        target = GameObject.Find("Player").transform;
        navMesh = GetComponent<NavMeshAgent>();
        navMesh.autoTraverseOffMeshLink = false;
        enemyPhysics = GetComponent<Rigidbody>();
        enemyPhysics.detectCollisions = false;
        jumpVector = new Vector3(0.0f, jumpHeight * 0.25f, jumpDistance * 0.25f);
    }

    IEnumerator Jump()
    {
        navMesh.Stop();
        enemyPhysics.isKinematic = false;
        enemyPhysics.useGravity = true;
        enemyPhysics.detectCollisions = true;
        Vector3 endPos = new Vector3(navMesh.currentOffMeshLinkData.endPos.x, navMesh.currentOffMeshLinkData.endPos.y + 2, navMesh.currentOffMeshLinkData.endPos.z);
        transform.position = Vector3.SmoothDamp(transform.position, endPos, ref enemyVelocity, Time.fixedDeltaTime * 20.0f);
        yield return new WaitForSeconds(0.75f);
        navMesh.CompleteOffMeshLink();
        enemyPhysics.isKinematic = true;
        enemyPhysics.useGravity = false;
        enemyPhysics.detectCollisions = false;
        navMesh.Resume();
    }

	// Update is called once per frame
	private void FixedUpdate () 
    {
        if (!navMesh.enabled)
            return;

        if (navMesh.isOnOffMeshLink)
        {
            StartCoroutine("Jump");
        }

        else
        {
            enemyPhysics.isKinematic = true;
            enemyPhysics.useGravity = false;
        }
	}

    private void Update()
    {
        navMesh.destination = target.position;
    }
}
