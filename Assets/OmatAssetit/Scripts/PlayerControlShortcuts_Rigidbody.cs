using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControlShortcuts_Rigidbody : MonoBehaviour
{
    public Vector3 rotationEuler = new Vector3(0f, 90f, 0f);
    public Vector3 teleportPosition = new Vector3(13f, 0f, 47f);

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rb.MoveRotation(Quaternion.Euler(rotationEuler));
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            // Teleport: asetetaan position ja rotation fysiikan kautta
            rb.position = teleportPosition;
            rb.MoveRotation(Quaternion.Euler(rotationEuler));
            // Jos Rigidbodyllä on velocity, nollaa se halutessasi:
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
