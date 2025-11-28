using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 75f;
    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == target)
        {
            Destroy(target.gameObject);
            Destroy(gameObject);
        }
    }
}
