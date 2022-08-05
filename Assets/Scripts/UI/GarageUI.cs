using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GarageUI : MonoBehaviour
{
     public GameObject buyBtn;
    [SerializeField] private TextMeshProUGUI buyBtnText;
    [SerializeField] private Transform carTransform;
    public TextMeshProUGUI priceTxt;
    public TextMeshProUGUI scoreText;
    [HideInInspector] public int currCar = 0;
    [HideInInspector] public string currCarName;
    [HideInInspector] public int currCarPrice;
    private GameObject carObj;
    
    [SerializeField] private Image speedFill;
    [SerializeField] private Image accelerationFill;
    [SerializeField] private Image weightFill;
    [SerializeField] private Image handlingFill;
    
    // Start is called before the first frame update
    void Start()
    {
        scoreText.text = PlayerPrefs.GetString("money", "0"); //TODO
        SetCarAndStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetCarAndStats()
    {
        if (carObj)
            Destroy(carObj);
        
        Quaternion rot = Quaternion.Euler(new Vector3(0, 90f, 0));
        carObj = Instantiate(Global.instance.cars[currCar]._carPrefab, 
            carTransform.position + new Vector3(0, Global.instance.cars[currCar]._carType == Car.CarType.monsterTruck ? -1 : 0, 0), 
            rot);
        currCarName = carObj.name;
        currCarPrice = Global.instance.cars[currCar]._price;
        handleUIUpdates();
    }

    public void handleUIUpdates()
    {
        priceTxt.text = Global.instance.cars[currCar].owned ? (Global.instance.cars[currCar] == Global.selectedCar ? "SELECTED" : "OWNED") : Global.instance.cars[currCar]._price.ToString();
        scoreText.text = PlayerPrefs.GetString("money", "0");
        buyBtn.SetActive(Global.instance.cars[currCar] != Global.selectedCar);
        buyBtnText.text = Global.instance.cars[currCar].owned ? "Select" : "Buy";
        speedFill.fillAmount = Global.instance.cars[currCar]._speed;
        accelerationFill.fillAmount = Global.instance.cars[currCar]._acceleration;
        weightFill.fillAmount = Global.instance.cars[currCar]._weight;
        handlingFill.fillAmount = Global.instance.cars[currCar]._handling;
    }
    
    public void NextCar()
    {
        currCar = (currCar + 1) % Global.instance.cars.Length;
        SetCarAndStats();
    }
    
    public void PrevCar()
    {
        currCar = (currCar - 1) < 0 ?  Global.instance.cars.Length - 1 : (currCar - 1);
        SetCarAndStats();
    }
}
