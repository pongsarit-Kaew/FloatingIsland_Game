using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    public Color stunnedColor = Color.red;

    private Rigidbody rb;
    private GameObject player;
    private bool isStunned;
    private Coroutine stunRoutine;
    private Renderer[] renderers;
    private Color[] originalColors;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        CacheOriginalColors();
    }

    void FixedUpdate()
    {
        if (!WaveSpawnManager.GameHasStarted) return;
        if (isStunned) return;

        Vector3 dir = player.transform.position - transform.position;
        dir.Normalize();
        rb.AddForce(dir * speed);
    }

    void Update()
    {
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    public void ApplyStun(float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            if (stunRoutine != null)
            {
                StopCoroutine(stunRoutine);
            }

            stunRoutine = StartCoroutine(StunRoutine(duration));
        }
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        SetEnemyColor(stunnedColor);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(duration);

        isStunned = false;
        RestoreOriginalColors();
        stunRoutine = null;
    }

    void CacheOriginalColors()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    void SetEnemyColor(Color color)
    {
        if (renderers == null) return;

        foreach (Renderer enemyRenderer in renderers)
        {
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = color;
            }
        }
    }

    void RestoreOriginalColors()
    {
        if (renderers == null || originalColors == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
}
