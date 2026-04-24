using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody rb;
    private GameObject player;
    private bool isStunned;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
    }

    void Start()
    {

    }

    // FixedUpdate จัดการเรื่องแรงและฟิสิกส์ (ทำงานด้วยความเร็วคงที่เสมอ)
    void FixedUpdate()
    {
        // 1. เช็คก่อนว่าโดน Stun ไหม ถ้าโดนให้หยุดขยับ
        if (isStunned) return;

        // 2. เดินพุ่งเข้าหา Player ด้วยความเร็วที่เสถียร
        Vector3 dir = player.transform.position - transform.position;
        dir.Normalize();
        rb.AddForce(dir * speed);
    }

    // Update จัดการเรื่องทั่วไป (ทำงานตามเฟรมเรตคอมพิวเตอร์)
    void Update()
    {
        // 3. ถ้าศัตรูโดนชนร่วงตกแมพ (ความสูง Y ต่ำกว่า -10) ให้ทำลายตัวเองทิ้งซะ!
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    public void ApplyStun(float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(StunRoutine(duration));
        }
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(duration);

        isStunned = false;
    }
}