using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public Transform focalPoint;
    public bool hasPowerUp = false;
    public GameObject powerupIndicator;
    public int coinCount = 0;
    public int bonusCoinCount = 0;
    public float deathY = -10f;
    public bool canMove = true;
    public GameObject powerBulletPrefab;
    public float powerBulletSpeed = 15f;
    public float powerBulletFireInterval = 0.35f;
    public float powerBulletLifeTime = 3f;
    public float powerBulletSpawnDistance = 1.2f;
    public int powerBulletCount = 8;
    public float powerUpDuration = 10f;

    private Rigidbody rb;

    private InputAction moveAction;
    private InputAction breakAction;
    private Coroutine countdownRoutine;
    private Coroutine bulletPowerRoutine;
    private float powerUpEndTime;
    private float bulletPowerEndTime;
    private float stunPowerEndTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        breakAction = InputSystem.actions.FindAction("Break");
    }

    void Update()
    {
        if (powerupIndicator != null && powerupIndicator.activeSelf)
        {
            powerupIndicator.transform.position = transform.position + new Vector3(0, -0.5f, 0);
        }

        if (transform.position.y < deathY)
        {
            RestartLevel();
        }

        UpdatePowerStatus();
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        var move = moveAction.ReadValue<Vector2>();

        rb.AddForce(move.y * speed * focalPoint.forward);

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
            powerUpEndTime = Time.time + powerUpDuration;
            powerupIndicator.SetActive(true);
            Destroy(other.gameObject);

            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
            }

            countdownRoutine = StartCoroutine(PowerUpCountDown());
        }

        if (other.CompareTag("StunPower"))
        {
            StunPower stunPower = other.GetComponent<StunPower>();
            float stunDuration = stunPower != null ? stunPower.stunDuration : 3f;
            stunPowerEndTime = Time.time + stunDuration;
        }
    }

    IEnumerator PowerUpCountDown()
    {
        yield return new WaitForSeconds(powerUpDuration);
        hasPowerUp = false;
        powerupIndicator.SetActive(false);
    }

    public void AddCoin(int amount)
    {
        AddCoin(amount, false);
    }

    public void AddCoin(int amount, bool isBonusCoin)
    {
        if (isBonusCoin)
        {
            bonusCoinCount += amount;
        }
        else
        {
            coinCount += amount;
        }
    }

    void RestartLevel()
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowGameOver();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ActivateBulletPower(float duration)
    {
        bulletPowerEndTime = Time.time + duration;

        if (bulletPowerRoutine != null)
        {
            StopCoroutine(bulletPowerRoutine);
        }

        bulletPowerRoutine = StartCoroutine(BulletPowerRoutine(duration));
    }

    IEnumerator BulletPowerRoutine(float duration)
    {
        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            ShootPowerBulletsAroundPlayer();
            yield return new WaitForSeconds(powerBulletFireInterval);
        }

        bulletPowerRoutine = null;
    }

    void UpdatePowerStatus()
    {
        if (GameUIManager.Instance == null) return;

        string status = "";
        if (hasPowerUp)
        {
            float powerUpTimeLeft = Mathf.Max(0f, powerUpEndTime - Time.time);
            status += $"PowerUp: {powerUpTimeLeft:0.0}s";
        }

        if (bulletPowerRoutine != null)
        {
            float bulletTimeLeft = Mathf.Max(0f, bulletPowerEndTime - Time.time);
            if (!string.IsNullOrEmpty(status))
            {
                status += "\n";
            }

            status += $"BulletPower: {bulletTimeLeft:0.0}s";
        }

        if (Time.time < stunPowerEndTime)
        {
            float stunTimeLeft = Mathf.Max(0f, stunPowerEndTime - Time.time);
            if (!string.IsNullOrEmpty(status))
            {
                status += "\n";
            }

            status += $"StunPower: {stunTimeLeft:0.0}s";
        }

        GameUIManager.Instance.SetPowerStatus(status);
    }

    void ShootPowerBulletsAroundPlayer()
    {
        int bulletCount = Mathf.Max(1, powerBulletCount);
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * 360f / bulletCount;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            ShootPowerBullet(direction);
        }
    }

    void ShootPowerBullet(Vector3 shootDirection)
    {
        Vector3 spawnPosition = transform.position + shootDirection * powerBulletSpawnDistance + Vector3.up * 0.3f;
        GameObject bulletObject = powerBulletPrefab != null
            ? Instantiate(powerBulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection))
            : CreateDefaultPowerBullet(spawnPosition);

        PowerBullet bullet = bulletObject.GetComponent<PowerBullet>();
        if (bullet == null)
        {
            bullet = bulletObject.AddComponent<PowerBullet>();
        }

        bullet.Launch(shootDirection, powerBulletSpeed, powerBulletLifeTime);
    }

    GameObject CreateDefaultPowerBullet(Vector3 spawnPosition)
    {
        GameObject bulletObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bulletObject.name = "PowerBullet";
        bulletObject.transform.position = spawnPosition;
        bulletObject.transform.localScale = Vector3.one * 0.35f;

        Renderer bulletRenderer = bulletObject.GetComponent<Renderer>();
        if (bulletRenderer != null)
        {
            bulletRenderer.material.color = Color.cyan;
        }

        Collider bulletCollider = bulletObject.GetComponent<Collider>();
        if (bulletCollider != null)
        {
            bulletCollider.isTrigger = true;
        }

        Rigidbody bulletRb = bulletObject.AddComponent<Rigidbody>();
        bulletRb.useGravity = false;
        bulletRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        return bulletObject;
    }
}
