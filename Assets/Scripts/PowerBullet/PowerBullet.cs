using UnityEngine;

public class PowerBullet : MonoBehaviour
{
    public float knockbackForce = 25f;
    public bool destroyEnemyOnHit = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Launch(Vector3 direction, float speed, float lifeTime)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Rigidbody enemyRb = other.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            Vector3 knockbackDirection = other.transform.position - transform.position;
            knockbackDirection.y = 0f;
            enemyRb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode.Impulse);
        }

        if (destroyEnemyOnHit)
        {
            Destroy(other.gameObject);
        }

        Destroy(gameObject);
    }
}
