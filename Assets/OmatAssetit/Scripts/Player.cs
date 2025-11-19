using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed = 13f;
    public float turnSpeed = 50f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.Phase != RacePhase.Racing)
        {
            return;
        }

        float move = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;
        //Debug.Log(move);

        transform.Translate(Vector3.forward * move);
        transform.Rotate(Vector3.up * turn);
    }
}
