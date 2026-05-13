using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public bool isBonusCoin = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddCoin(value, isBonusCoin);
        }

        Destroy(gameObject);
    }
}
