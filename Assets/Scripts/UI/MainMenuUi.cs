using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUi : MonoBehaviour
{
    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI scoreText;
    public Image profilePic;
    // Start is called before the first frame update
    void Start()
    {

        userNameText.text = PlayerPrefs.GetString("username", "UNKOWN");
        scoreText.text = PlayerPrefs.GetString("money", "0") + "xP";
        profilePic.sprite = Global.instance.profilePics[Global.instance.selectedPic];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("loggedin");
        PlayerPrefs.DeleteKey("email");
        PlayerPrefs.DeleteKey("password");
        Global.instance.LoadLevel("Login");
    }
}
