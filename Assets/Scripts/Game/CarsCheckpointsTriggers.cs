using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CarsCheckpointsTriggers : MonoBehaviourPun
{


     public int carPosition;
     private float endTime;
     private float startTime;
     public float scoreTimeEndMatch = 0f;





    private void OnTriggerEnter(Collider other)
    {
        if(gameObject.tag == "PLAYER") {
            if (other.gameObject.tag == "StartPosition")
            {
                startTime = Time.time;
            }
            else if (other.gameObject.tag == "EndPosition")
            {
                endTime = Time.time;
                scoreTimeEndMatch = endTime - startTime;
                StartCoroutine(FirebaseManager.UpdateMatchState(PlayerPrefs.GetString("username"), scoreTimeEndMatch.ToString(),
                    PhotonNetwork.CurrentRoom.Name));
                StartCoroutine(WinGame());
                gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
        }


    }

    private IEnumerator WinGame()
    {
        Debug.Log("you will win");
        var DBTask2 = FirebaseManager.DBreference.Child("matches").Child(PhotonNetwork.CurrentRoom.Name).Child("decision").SetValueAsync(PlayerPrefs.GetString("username"));
        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
    }


}
