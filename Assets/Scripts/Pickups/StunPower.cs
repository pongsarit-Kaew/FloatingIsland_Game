using UnityEngine;

public class StunPower : MonoBehaviour
{
    public float stunDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Enemy[] allEnemies = GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            enemy.ApplyStun(stunDuration);
        }

        Destroy(gameObject);
    }
}
