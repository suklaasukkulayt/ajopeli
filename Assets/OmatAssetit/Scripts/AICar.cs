using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AICar : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    public float speed = 10f;
    public float rotationSpeed = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Siirretty Updatesta FixedUpdateen ja transform.Translate/rotation -> rb.MovePosition/MoveRotation.
    // Sama syy kuin Player.cs:ssä aiemmin: suora Transform-asetus ohittaa fysiikkamoottorin,
    // jolloin se riitelee samalla GameObjectilla olevan Rigidbodyn (gravitaatio, törmäykset)
    // kanssa joka framessa -- tästä nykiminen ja osin myös seinien läpimeno tulivat.
    void FixedUpdate()
    {
        if (GameManager.Instance.Phase != RacePhase.Racing)
        {
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 targetXZ = new Vector3(target.position.x, rb.position.y, target.position.z);
        Vector3 direction = (targetXZ - rb.position).normalized;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, lookRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }

        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);

        if (Vector3.Distance(rb.position, targetXZ) < 2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}