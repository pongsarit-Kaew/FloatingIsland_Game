using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. เช็คว่าคนที่มาชนเส้นชัย คือผู้เล่น (Player) หรือเปล่า
        if (other.CompareTag("Player"))
        {
            Debug.Log("เข้าเส้นชัยแล้ว! กำลังบันทึกเซฟ...");
            UnlockNextLevel();
        }
    }

    void UnlockNextLevel()
    {
        // หาว่าตอนนี้เราเล่นอยู่ด่านที่เท่าไหร่ (Level 1 คือ Index 1)
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // ด่านต่อไปที่จะปลดล็อก ก็คือเอาด่านปัจจุบัน + 1 (จะเป็นด่าน 2)
        int nextLevelIndex = currentLevelIndex + 1;

        // ดึงข้อมูลเซฟเดิมมาดูว่า เคยปลดล็อกถึงด่านไหนแล้ว (ค่าเริ่มต้นคือ 1)
        int highestUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // ถ้าด่านที่เรากำลังจะปลดล็อก มันสูงกว่าเซฟเดิม ถึงจะยอมให้บันทึกทับลงไป
        if (nextLevelIndex > highestUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevelIndex);
            PlayerPrefs.Save(); // สั่งเซฟลงเครื่อง!
        }

        // โหลดฉาก MainMenu (Index 0) เพื่อให้ผู้เล่นกลับไปเลือกด่าน 2 ที่เพิ่งปลดล็อก
        SceneManager.LoadScene(0);
    }
}