using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float move = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        //Debug.Log(move);

        transform.Translate(Vector3.forward * move);
    }
}
