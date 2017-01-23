using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Speed;

    private void Update()
    {
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.GetComponent<Enemy>())
        Destroy(gameObject);

    }
}