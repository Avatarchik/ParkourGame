using UnityEngine;
using System.Collections;

public class TestAI : MonoBehaviour 
{
    private readonly VectorPid angularVelocityController = new VectorPid(33.7766f, 0, 0.2553191f);
    private readonly VectorPid headingController = new VectorPid(9.244681f, 0, 0.06382979f);

    public Transform target;
    private Rigidbody rigidbody;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        /*var angularVelocityError = rigidbody.angularVelocity * -1;
        Debug.DrawRay(transform.position, rigidbody.angularVelocity * 10, Color.black);

        var angularVelocityCorrection = angularVelocityController.Update(angularVelocityError, Time.deltaTime);
        Debug.DrawRay(transform.position, angularVelocityCorrection, Color.green);

        rigidbody.AddTorque(angularVelocityCorrection);

        var desiredHeading = target.position - transform.position;
        Debug.DrawRay(transform.position, desiredHeading, Color.magenta);

        var currentHeading = transform.up;
        Debug.DrawRay(transform.position, currentHeading * 15, Color.blue);

        var headingError = Vector3.Cross(currentHeading, desiredHeading);
        var headingCorrection = headingController.Update(headingError, Time.deltaTime);

        rigidbody.AddTorque(headingCorrection);*/
    }
}
