using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    private float currentSpeed;
    private Rigidbody rb;
    private AudioSource carAudio;

    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;

    private float pitchFromCar;

    private void Start()
    {
        carAudio = GetComponent<AudioSource>();
        carAudio.pitch = minPitch;
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        EngineSound();
    }

    void EngineSound() {
        //Object Speed from velocity magnitude
        currentSpeed = rb.velocity.magnitude;
        pitchFromCar = rb.velocity.magnitude / 50f;

        if(currentSpeed < minSpeed) {
            carAudio.pitch = minPitch;
        }
        if(currentSpeed > minSpeed && currentSpeed < maxSpeed) {
            carAudio.pitch = minPitch + pitchFromCar;
        }
        if(currentSpeed > maxSpeed) {
            carAudio.pitch = maxPitch;
        }

    }
}
