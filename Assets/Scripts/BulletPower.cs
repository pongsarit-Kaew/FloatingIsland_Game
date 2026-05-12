using UnityEngine;

public class BulletPower : MonoBehaviour
{
    public float duration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ActivateBulletPower(duration);
        }

        Destroy(gameObject);
    }
}
