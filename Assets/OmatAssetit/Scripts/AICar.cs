using UnityEngine;

public class AICar : MonoBehaviour
{

    public Transform[] waypoints;
    private int currentWaypointIndex = 0;

    public float speed = 10f;

    // Update is called once per frame
    void Update()
    {

        Transform target = waypoints[currentWaypointIndex];
        Vector3 targetXZ = new Vector3(target.position.x, transform.position.y, target.position.z);
        Vector3 direction = (targetXZ - transform.position).normalized;

        Quaternion lookRotation = Quaternion.LookRotation(direction);

        transform.rotation = lookRotation;

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, targetXZ) < 2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}
