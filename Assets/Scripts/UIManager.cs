using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Text bulletText;

    public void SetBulletText(int ammoClip,int ammunition)
    {
        bulletText.text = ammoClip + "/" + ammunition;
    }
}
