using UnityEngine;

public class BOB : MonoBehaviour
{

    Vector3 pos;
    public float speed = 0.5f;

    public float height = 0.5f;
    

    void Start() {
            pos = transform.localPosition;
    }

    void Update() {
            Debug.Log(pos);
            float newY = Mathf.Sin(Time.time * speed);
            transform.localPosition = new Vector3(pos.x, newY* height, pos.z) ;
    }
}
