using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Car", menuName = "New Car")]
public class Car : ScriptableObject
{
    [System.Serializable]
    public enum CarType
    {
        rally,
        racing,
        monsterTruck
    }
    
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private CarType carType;
    [SerializeField] private int price = 1000;

    public bool owned = false;
    [Range(0, 10)] public int updateLevel = 0;
    // values between 0 and 1
    [SerializeField][Range(0, 1)] private float speed;
    [SerializeField][Range(0, 1)] private float acceleration;
    [SerializeField][Range(0, 1)] private float weight;
    [SerializeField][Range(0, 1)] private float handling;
    
    //getter for stuff that shouldn't be changed
    public GameObject _carPrefab
    {
        get { return carPrefab; }
    }
    
    public CarType _carType
    {
        get { return carType; }
    }
    public int _price
    {
        get { return price; }
    }
    
    public float _speed
    {
        get { return speed; }
    }
    
    public float _acceleration
    {
        get { return acceleration; }
    }
    
    public float _weight
    {
        get { return weight; }
    }
    
    public float _handling
    {
        get { return handling; }
    }
}