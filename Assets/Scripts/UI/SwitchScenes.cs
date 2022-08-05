using UnityEngine;

public class SwitchScenes : MonoBehaviour
{
    public void LoadScene(int scene)
    {
        Global.instance.LoadLevel(scene);
    }
    
    public void LoadScene(string scene)
    {
        Global.instance.LoadLevel(scene);
    }
}
