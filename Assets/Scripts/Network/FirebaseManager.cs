using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")] public DependencyStatus dependencyStatus;
    public static FirebaseAuth Auth;
    public static FirebaseUser User;
    public static DatabaseReference DBreference;

    //Login variables
    [Header("Login")] public GameObject loginUI;
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")] public GameObject RegisterUI;
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;


    //User Data variables
    [Header("UserData")] public TMP_InputField usernameField;

    private bool dataLoaded = false;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
        if (PlayerPrefs.GetInt("loggedin", 0) == 1)
        {
            var email = PlayerPrefs.GetString("email");
            var password = PlayerPrefs.GetString("password");
            StartCoroutine(Login(email, password));
            loginUI.SetActive(false);
            RegisterUI.SetActive(false);

        }
        else if (PlayerPrefs.GetString("player_status") == "Registered")
        {
            loginUI.SetActive(true);
            RegisterUI.SetActive(false);
        }
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        Auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    //Function for the sign out button
    public void SignOutButton()
    {
        Auth.SignOut();
        UIManager.instance.LoginScreen();
        ClearRegisterFeilds();
        ClearLoginFeilds();
    }

    //Function for the save button
    public void SaveDataButton()
    {
        StartCoroutine(UpdateUsernameAuth(usernameField.text));
        StartCoroutine(UpdateUsernameDatabase(usernameField.text));
    }

    //Function for the scoreboard button
    private IEnumerator Login(string _email, string _password)
    {
        yield return new WaitForSeconds(1f);
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = Auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }

            warningLoginText.text = message;
        }
        else
        {
            PlayerPrefs.SetInt("loggedin", 1);
            PlayerPrefs.SetString("email", _email);
            PlayerPrefs.SetString("password", _password);
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";
            StartCoroutine(LoadUserData()); //TODO

            PlayerPrefs.SetString("player_status", "Registered");

            yield return new WaitUntil(predicate: () => dataLoaded);
            Global.instance.LoadLevel("Menu");
            confirmLoginText.text = "";
            ClearLoginFeilds();
            ClearRegisterFeilds();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if (_username.Contains(" "))
        {
            warningRegisterText.text = "Username Must Not Contain Space";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Check if user is already in DB
            var DBTask = DBreference.Child("users").OrderByChild("username").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            DataSnapshot snapshot = DBTask.Result;
            bool exists = false;
            foreach (DataSnapshot res in snapshot.Children.Reverse<DataSnapshot>())
            {
                if (res.Child("username").Value.ToString() == _username)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                warningRegisterText.text = "User Available";
            }
            else
            {
                //Call the Firebase auth signin function passing the email and password
                var RegisterTask = Auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
                //Wait until the task completes
                yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

                if (RegisterTask.Exception != null)
                {
                    //If there are errors handle them
                    Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                    FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                    string message = "Register Failed!";
                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            message = "Missing Email";
                            break;
                        case AuthError.MissingPassword:
                            message = "Missing Password";
                            break;
                        case AuthError.WeakPassword:
                            message = "Weak Password";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            message = "Email Already In Use";
                            break;
                    }

                    warningRegisterText.text = message;
                }
                else
                {
                    //User has now been created
                    //Now get the result
                    User = RegisterTask.Result;

                    if (User != null)
                    {
                        //Create a user profile and set the username
                        UserProfile profile = new UserProfile { DisplayName = _username };

                        //Call the Firebase auth update user profile function passing the profile with the username
                        var ProfileTask = User.UpdateUserProfileAsync(profile);
                        //Wait until the task completes
                        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                        if (ProfileTask.Exception != null)
                        {
                            //If there are errors handle them
                            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                            warningRegisterText.text = "Username Set Failed!";
                        }
                        else
                        {
                            //Username is now set
                            //Now return to login screen
                            PlayerPrefs.SetString("player_status", "Registered");
                            warningRegisterText.text = "";
                            StartCoroutine(Login(_email, _password));
                            ClearRegisterFeilds();
                            ClearLoginFeilds();
                        }
                    }
                }
            }
        }
    }

    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username };

        //Call the Firebase auth update user profile function passing the profile with the username
        var ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }
    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    public static IEnumerator UpdateUsercarDatabase(string _usercar, int updateLevel)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("ownedCars").Child(_usercar)
            .SetValueAsync(updateLevel.ToString());

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public static IEnumerator UpdateSelectedCarDatabase(string _selectedCar)
    {
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("selectedCar").SetValueAsync(_selectedCar);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public static IEnumerator UpdateUserRankDatabase(string userRank)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("rank").SetValueAsync(userRank);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public static IEnumerator UpdateMoneyDatabase(String money, string uid = "")
    {
        //Set the currently logged in user username in the database
        var DBTask = DBreference.Child("users").Child(uid == "" ? User.UserId : uid).Child("money")
            .SetValueAsync(money);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public static IEnumerator UpdateUserProfilePicDatabase(String profilePicIndex)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("profilePic").SetValueAsync(profilePicIndex);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public static IEnumerator UpdateUserfriendsDatabase(string[] _userfriends, string userId = "")
    {
        //Set the currently logged in user username in the database
        var DBTask = DBreference.Child("users").Child(userId == "" ? User.UserId : userId).Child("userfriends")
            .SetValueAsync(String.Join(" ", _userfriends));

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    public static IEnumerator UpdateMatchInvitationsDatabase(string[] _matchInvivations, string userId)
    {
        // Logging
        var uid = userId == "" ? User.UserId : userId;
        Debug.Log("UID = " + uid);

        // Sync with Server
        var DBTask = DBreference.Child("users").Child(userId).Child("matchInvitations")
            .SetValueAsync(String.Join(" ", _matchInvivations));
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    public static IEnumerator UpdateUserfriendRequestsDatabase(string[] _userfriendRequests, string userId = "")
    {
        //Set the currently logged in user username in the database
        var uid = userId == "" ? User.UserId : userId;
        Debug.Log("UID = " + uid);
        var DBTask = DBreference.Child("users").Child(userId == "" ? User.UserId : userId).Child("userFriendRequests")
            .SetValueAsync(String.Join(" ", _userfriendRequests));

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }


    public static IEnumerator UpdateMessages(string sender, string receiver, string text)
    {
        //get random GUID
        //var uid = Guid.NewGuid();
        //Set the currently logged in user xp
        var date = System.DateTime.UtcNow.ToString("HH:mm:ss dd MMMM, yyyy");
        var messageid = "message-id-" + date;
        var DBTask = DBreference.Child("messages").Child(messageid).Child("sender").SetValueAsync(sender);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DBTask = DBreference.Child("messages").Child(messageid).Child("receiver").SetValueAsync(receiver);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DBTask = DBreference.Child("messages").Child(messageid).Child("text").SetValueAsync(text);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        DBTask = DBreference.Child("messages").Child(messageid).Child("sent_time")
            .SetValueAsync(date);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Xp is now updated
        }
    }

    private IEnumerator LoadUserData()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();


        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        var username = User.DisplayName;
        IEnumerable<DataSnapshot> ownedCars;
        var carUpdateLevel = 0;
        var userRank = "0";
        var money = "1000";
        String selectedCar = Global.instance.cars[0].name;
        String profilePicIndex = "0";
        string[]
            userfriends =
                { }; //Friends will be in this format e.g: "Lisa Roman Elias" we will split them into an array of string
        string[]
            userfriendRequests =
            {
            }; //FriendRequests will be in this format e.g: "Lisa Roman Elias" we will split them into an array of string
        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            StartCoroutine(UpdateUsernameDatabase(username));
            StartCoroutine(UpdateUserRankDatabase(userRank));
            StartCoroutine(UpdateMoneyDatabase(money));
            StartCoroutine(UpdateUsercarDatabase(selectedCar, carUpdateLevel));
            StartCoroutine(UpdateUserfriendsDatabase(userfriends));
            StartCoroutine(UpdateUserfriendRequestsDatabase(userfriendRequests));
            StartCoroutine(UpdateUserProfilePicDatabase(profilePicIndex.ToString()));
            StartCoroutine(UpdateSelectedCarDatabase(selectedCar));
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            username = snapshot.Child("username").Value.ToString();
            ownedCars = snapshot.Child("ownedCars").Children;
            selectedCar = snapshot.Child("selectedCar").Value.ToString();
            updateCarsAndSelectedCar(ownedCars, snapshot.Child("selectedCar").Value.ToString());
            userRank = snapshot.Child("rank").Value.ToString();
            money = snapshot.Child("money").Value.ToString();
            userfriends = snapshot.Child("userfriends").Value.ToString().Split();
            userfriendRequests = snapshot.Child("userFriendRequests").Value.ToString().Split();
            profilePicIndex = snapshot.Child("profilePic").Value.ToString();
        }

        //TODO: SAVE THE PLAYER DETAILS IN A PLAYER OBJECT OR GLOBAL AFTER INITIALIZING THE MAIN MENU
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("userRank", userRank);
        PlayerPrefs.SetString("selectedCar", selectedCar);
        PlayerPrefs.SetString("money", money);
        PlayerPrefs.SetString("userfriends", String.Join(" ", userfriends));
        PlayerPrefs.SetString("userfreindRequests", String.Join(" ", userfriendRequests));
        PlayerPrefs.SetString("profilePic", profilePicIndex);
        Debug.Log($"username: {PlayerPrefs.GetString("username")}");
        Debug.Log($"money: {PlayerPrefs.GetString("money")}");

        int picIndex;
        int.TryParse(profilePicIndex, out picIndex);
        Global.instance.selectedPic = picIndex;

        dataLoaded = true;
    }


    private void updateCarsAndSelectedCar(IEnumerable<DataSnapshot> ownedCars, string selectedCar)
    {
        foreach (var dataSnapshot in ownedCars)
        {
            Global.instance.cars.ToList().ForEach(car =>
            {
                if (car.name == dataSnapshot.Key)
                {
                    car.owned = true;
                    car.updateLevel = int.Parse(dataSnapshot.Value.ToString());
                    if (car.name == selectedCar)
                        Global.selectedCar = car;
                }
            });
        }
    }

    //INGAME-STUFF
    public static IEnumerator CreateMatchStateForPlayer(string _player, string matchId)
    {
        var DBTask2 = DBreference.Child("matches").Child(matchId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
        //CHECK PLAYER POSITION 1 OR 2
        if (DBTask2.Result.Value == null)
        {
            PlayerPrefs.SetString("player1", _player);
            var DBTask = DBreference.Child("matches").Child(matchId).Child("player1").SetValueAsync(_player);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            DBTask = DBreference.Child("matches").Child(matchId).Child(_player + "_time").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        }
        else
        {
            PlayerPrefs.SetString("player2", _player);
            var DBTask = DBreference.Child("matches").Child(matchId).Child("player2").SetValueAsync(_player);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            DBTask = DBreference.Child("matches").Child(matchId).Child(_player + "_time").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        }
    }

    public static IEnumerator UpdateMatchState(string _player, string time, string matchId)
    {
        //var date = System.DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy");
        //ar matchId = "match-id-" + date;
        Debug.Log(_player + " will update the time");
        var DBTask = DBreference.Child("matches").Child(matchId).Child(_player + "_time").SetValueAsync(time);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }
}