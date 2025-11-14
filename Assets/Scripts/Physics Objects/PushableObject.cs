using UnityEngine;

public class PushableObject : MonoBehaviour
{

    public bool isBeingMoved = false;
    public float moveSpeed = 3f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Move(Vector3 direction)
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
