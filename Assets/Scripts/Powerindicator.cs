using UnityEngine;

public class Powerindicator : MonoBehaviour
{
    public GameObject powerupIndicator;


    void Start()
    {
        powerupIndicator.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RotateAnimation"))
        {
            Destroy(other.gameObject);
            StartCoroutine(PowerupCountdownRoutine());
        }

        Debug.Log(other.gameObject.name);

        if (other.CompareTag("RotateAnimation"))
        {
            Destroy(other.gameObject);
            StartCoroutine(PowerupCountdownRoutine());
        }
    }

    System.Collections.IEnumerator PowerupCountdownRoutine()
    {
        powerupIndicator.SetActive(true);
        yield return new WaitForSeconds(5.0f);
        powerupIndicator.SetActive(false);
    }

    void Update()
    {
        if (powerupIndicator.activeSelf)
        {
            powerupIndicator.transform.position = transform.position + new Vector3(0, -0.5f, 0);
        }
    }
}