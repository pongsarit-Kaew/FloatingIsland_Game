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

    private Rigidbody rb;

    private InputAction moveAction;
    private InputAction smashAction;
    private InputAction breakAction;
    private Coroutine countdownRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        smashAction = InputSystem.actions.FindAction("Smash");
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
            powerupIndicator.SetActive(true);
            Destroy(other.gameObject);

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

        Debug.Log($"Coin: {coinCount} Bonus: {bonusCoinCount}");
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
