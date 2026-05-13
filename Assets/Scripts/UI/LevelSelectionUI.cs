using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("ลากปุ่มด่าน 1, 2, 3, 4 มาใส่เรียงตามลำดับ")]
    public Button[] levelButtons;

    void Start()
    {
        // โหลดข้อมูลเซฟ ว่าผู้เล่นไปถึงด่านไหนแล้ว (ถ้าเพิ่งเล่นเกมครั้งแรก ค่าจะเป็นด่าน 1)
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // วนลูปเช็คปุ่ม UI ทั้งหมดที่เราใส่ไว้
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1; // ปุ่มแรก (i=0) คือด่าน 1

            if (levelIndex > unlockedLevel)
            {
                // ถ้าด่านนี้ยังไม่ปลดล็อก -> ปิดการกดปุ่ม (ปุ่มจะกลายเป็นสีเทาและกดไม่ได้)
                levelButtons[i].interactable = false;
            }
            else
            {
                // ถ้าปลดล็อกแล้ว -> เปิดให้กดได้ตามปกติ
                levelButtons[i].interactable = true;
            }
        }
    }

    // ฟังก์ชันนี้เอาไว้ให้ปุ่มเรียกใช้เวลาถูกคลิก เพื่อโหลดด่านนั้นๆ
    public void LoadLevel(int levelToLoad)
    {
        SceneManager.LoadScene(levelToLoad);
    }

    // (แถม) ฟังก์ชันสำหรับลบเซฟ เริ่มเล่นใหม่ตั้งแต่ด่าน 1 เอาไว้เทสต์เกมครับ
    public void ClearSave()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // รีเฟรชหน้าเมนู
    }

}