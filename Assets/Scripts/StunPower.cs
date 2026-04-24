using UnityEngine;

public class StunPower : MonoBehaviour
{
    public float stunDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        // 1. ต้องเช็คว่า Player ติด Tag "Player" หรือยัง
        if (other.CompareTag("Player"))
        {
            Debug.Log("เก็บไอเทมได้แล้ว!"); // ใส่ไว้เช็คใน Console

            // 2. หา Enemy ทั้งหมด (ใช้คำสั่งใหม่ของ Unity 6)
            Enemy[] allEnemies = GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

            foreach (Enemy e in allEnemies)
            {
                e.ApplyStun(stunDuration);
            }

            // 3. หายไปเมื่อเก็บ
            Destroy(gameObject);
        }
    }
}