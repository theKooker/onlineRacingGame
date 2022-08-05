using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class GarageBuyCar : MonoBehaviour
{
    public GarageUI garage;
    private string car;

    private int myMoney;

    private int carPrice;
    private void Start()
    {
        int.TryParse(PlayerPrefs.GetString("money", "0"), out myMoney);
    }

    public void triggerEventOnCar()
    {
        car = Global.instance.cars[garage.currCar].name;
        Debug.Log(garage.buyBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "Buy");
        if (garage.buyBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == "Buy")
        {
            
            carPrice = Global.instance.cars[garage.currCar]._price;
            Debug.Log("carPrice is "+ carPrice + " you have "+ myMoney + " money");
            if (myMoney >= carPrice)
            {
                PlayerPrefs.SetString("selectedCar", car);
                myMoney = myMoney - carPrice;
                PlayerPrefs.SetString("money", myMoney.ToString());
                //TODO : UPDATE FIRE BASE
                Global.instance.cars[garage.currCar].owned = true;
                Global.selectedCar = Global.instance.cars[garage.currCar];
                garage.handleUIUpdates();
                
                StartCoroutine(FirebaseManager.UpdateUsercarDatabase(car,0));
                StartCoroutine(FirebaseManager.UpdateMoneyDatabase(myMoney.ToString()));
                StartCoroutine(FirebaseManager.UpdateSelectedCarDatabase(car));
            }
        }
        else
        {
            //HANDLE SELECT
            var currentCarInPLayerPrefs = PlayerPrefs.GetString("selectedCar", "");
            if (car == currentCarInPLayerPrefs)
            {
                //NOTHING
            }
            else
            {
                PlayerPrefs.SetString("selectedCar", car);
                //TODO: UPDATE FIREBASE
                Global.selectedCar = Global.instance.cars[garage.currCar];
                Global.instance.cars[garage.currCar].owned = true;
                garage.handleUIUpdates();
                StartCoroutine(FirebaseManager.UpdateSelectedCarDatabase(car));
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        car = garage.currCarName;
        carPrice = garage.currCarPrice;
    }
    
}
