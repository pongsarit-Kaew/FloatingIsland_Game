using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public Transform focalPoint;
    public bool hasPowerUp = false;
    public GameObject powerupIndicator;

    private Rigidbody rb;

    private InputAction moveAction;
    private InputAction smashAction;
    private InputAction breakAction;
    private Coroutine countdownRoutine; // เปลี่ยนจาก object เป็น Coroutine เพื่อให้หยุดเวลาได้ถูกต้อง


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        smashAction = InputSystem.actions.FindAction("Smash");
        breakAction = InputSystem.actions.FindAction("Break");
    }

    // Update จัดการเรื่องภาพและเอฟเฟกต์ (ทำงานตามเฟรมเรตคอมพิวเตอร์)
    void Update()
    {
        // อัปเดตตำแหน่งวงแหวน PowerUp ให้ตามตัวผู้เล่น
        if (powerupIndicator != null && powerupIndicator.activeSelf)
        {
            powerupIndicator.transform.position = transform.position + new Vector3(0, -0.5f, 0);
        }
    }

    // FixedUpdate จัดการเรื่องแรงและฟิสิกส์ (ทำงานด้วยความเร็วคงที่เสมอ เท่ากันทุกเครื่อง!)
    void FixedUpdate()
    {
        // 1. อ่านค่าปุ่มเดิน
        var move = moveAction.ReadValue<Vector2>();

        // 2. ออกแรงผลักลูกบอล
        rb.AddForce(move.y * speed * focalPoint.forward);

        // 3. ระบบเบรก
        if (breakAction.IsPressed())
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (hasPowerUp == true)
            {
                var enemyRb = collision.gameObject.GetComponent<Rigidbody>();
                var dir = collision.transform.position - transform.position;
                enemyRb.AddForce(100 * dir.normalized, ForceMode.Impulse);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp"))
        {
            hasPowerUp = true;
            powerupIndicator.SetActive(true);
            Destroy(other.gameObject);

            // ถ้าระบบกำลังนับเวลา PowerUp อันเก่าอยู่ ให้สั่งหยุดก่อน แล้วค่อยเริ่มนับ 10 วิใหม่
            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
            }
            countdownRoutine = StartCoroutine(PowerUpCountDown());
        }
    }

    IEnumerator PowerUpCountDown()
    {
        yield return new WaitForSeconds(10f);
        hasPowerUp = false;
        powerupIndicator.SetActive(false);
    }
}