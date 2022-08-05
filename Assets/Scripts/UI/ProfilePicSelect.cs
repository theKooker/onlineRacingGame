using UnityEngine;
using UnityEngine.UI;

public class ProfilePicSelect : MonoBehaviour
{
    public GameObject profilePicPrefab;
    [SerializeField] private Transform scrollContent;
    
    // Start is called before the first frame update
    void Start()
    {
        Sprite[] pics = Global.instance.profilePics;
        for (int i = 0; i < pics.Length; i++)
        {
            int index = i ; // Prevents the closure problem
            GameObject gO = Instantiate(profilePicPrefab, scrollContent);
            gO.GetComponent<Button>().onClick.AddListener( () => UpdateProfilePic( index ) );
            gO.GetComponent<Image>().sprite = pics[i];
        }
    }
    public void UpdateProfilePic( int buttonIndex )
    {
        Sprite[] pics = Global.instance.profilePics;
        Global.instance.selectedPic = buttonIndex;
        Debug.Log("profile pic in menu updated with index" + buttonIndex, pics[buttonIndex]);
        StartCoroutine(FirebaseManager.UpdateUserProfilePicDatabase(buttonIndex.ToString()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
