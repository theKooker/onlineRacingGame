

public class CustomUser
{
    private readonly string _username;
    private int _profilePictureIndex;
    private bool _isOnlineOrReady;

    // private IEnumerable<WaitUntil> DownloadUserDataFromServer()
    // {
    //     // get Firebase User-Block
    //     var temp = FirebaseManager.DBreference.Child("users").Child(FirebaseManager.User.UserId).GetValueAsync();
    //     yield return new WaitUntil(predicate: () => temp.IsCompleted);
    //     DataSnapshot snapshot = temp.Result;
    //
    //     // setupAllFetchedData
    //     this._profilePictureIndex = int.Parse(snapshot.Child("profilePic").Value.ToString());
    //     this._isOnlineOrReady = snapshot.Child("isOnline").Value.Equals(true);
    // }


    public CustomUser(string username, int profilePictureIndex)
    {
        this._username = username;
        this._profilePictureIndex = profilePictureIndex;
        // DownloadUserDataFromServer();
    }

    // void SetOnlineOrReady(bool onlineOrReady)
    // {
    //     this.isOnlineOrReady = onlineOrReady;
    // }

    public bool GetOnlineOrReady()
    {
        return this._isOnlineOrReady;
    }

    public string GetName()
    {
        return this._username;
    }

    public int GetProfilePictureIndex()
    {
        return this._profilePictureIndex;
    }
}