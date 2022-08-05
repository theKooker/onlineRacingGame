using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Global : MonoBehaviour
{
    public static Global instance;
    public Car[] cars;
    public static Car selectedCar;
    [HideInInspector] public int selectedPic; // selected profilePic
    public Sprite[] profilePics; // Array of all possible profilePics
    public string[] levels = { "level1", "level2", "level3", "level4" };
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    #region Scene Loading
    [Header("Scene Loading")]
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private GameObject loadingCanvas;

    public void LoadLevel(string name)
    {
        StartCoroutine(LoadAsynchronously(SceneManager.LoadSceneAsync(name)));
        if ((name.Contains("Level") && name != "LevelSelect") || name == "Login") // this is a bit ugly but it should work
        {
            AudioManager.instance.StopSound("Theme");
        }
        else
        {
            AudioManager.instance.PlaySound("Theme", true);
        }
    }

    public void LoadLevel(int sceneIndex)
    {
        LoadLevel(SceneManager.GetSceneAt(sceneIndex).name);
    }



    private IEnumerator LoadAsynchronously(AsyncOperation async)
    {
        async.allowSceneActivation = false;

        loadingCanvas.SetActive(true);

        while (loadingBarFill.fillAmount < 1f)
        {
            loadingBarFill.fillAmount = Mathf.Clamp01(async.progress / .9f);

            yield return null;
        }

        async.allowSceneActivation = true;

        loadingCanvas.SetActive(false);
    }
    #endregion
}