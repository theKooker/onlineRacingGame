using System;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class RacingCarController : MonoBehaviourPun
{
    private float horizontalInput;
    private float currentBreakForce;
    private float steerAngle;
    private bool isBreaking;
    private int boostTimeCount = 0;
    private int motorScale = 1;


    private GameObject emoji;

    [Header("Car Settings")]
    [SerializeField]
    private bool hasBoost;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;
    [SerializeField] private float downForceValue;

    [Header("Wheels (Required!)")]
    [SerializeField]
    private WheelCollider frontLeftWheelCollider;

    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider;
    [SerializeField] private WheelCollider backRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform backLeftWheelTransform;
    [SerializeField] private Transform backRightWheelTransform;

    [Header("Effects")][SerializeField] private ParticleSystem smokeLeft;
    [SerializeField] private ParticleSystem smokeRight;
    [SerializeField] private ParticleSystem boostFire;
    [Header("Audio")][SerializeField] private AudioSource breakHandler;


    void Awake()
    {
        smokeLeft.Pause();
        smokeRight.Pause();
        boostFire.Pause();

        if (!photonView.IsMine)
        {
            Destroy(GetComponent<RacingCarController>());
            Destroy(transform.GetChild(transform.childCount - 2).gameObject);
        }
    }


    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        AddDownForce();
    }

    private bool gotBreakEventTrigger()
    {
        if (Input.touchCount > 0 || Input.GetKey(KeyCode.B))
        {
            bool isTriggered = Input.touches.Count(element =>
                element.position.x < Screen.width / 2.0f && element.position.y < Screen.height * 1f / 3f) != 0;
            if (GetComponent<Rigidbody>().velocity.magnitude < 1)
            {
                motorScale = -1;
            }
            else
            {
                motorScale = 1;
            }
            return isTriggered && GetComponent<Rigidbody>().velocity.magnitude > 1;

        }
        else
        {
            motorScale = 1;
            return false;
        }
    }

    private bool gotBoostEventTrigger()
    {
        if (Input.touchCount > 0)
        {
            bool isTriggered = Input.touches.Count(element =>
                element.position.x >= Screen.width / 2.0f && element.position.y < Screen.height * 1f / 3f) != 0;
            return isTriggered;
        }
        else
        {
            return false;
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.acceleration.x * 0.5f; //Input.GetAxis("Horizontal");
        isBreaking = gotBreakEventTrigger();
        //we should collect boos items here
        if (gotBoostEventTrigger() && hasBoost && boostTimeCount == 0)
        {
            boostTimeCount = 200;
        }
    }

    private void HandleMotor()
    {
        if (boostTimeCount > 0)
        {
            frontLeftWheelCollider.motorTorque = 5 * motorForce * motorForce;
            frontRightWheelCollider.motorTorque = 5 * motorForce * motorForce;
            boostTimeCount--;
            boostFire.Play();
        }
        else
        {
            //hasBoost = false;
            boostFire.Clear();
            boostFire.Pause();
            frontLeftWheelCollider.motorTorque = motorForce * motorForce * motorScale;
            frontRightWheelCollider.motorTorque = motorForce * motorForce * motorScale;
        }

        currentBreakForce = isBreaking ? breakForce : 0f;
        if (isBreaking && GetComponent<Rigidbody>().velocity.magnitude > 14)
        {
            if (!breakHandler.isPlaying)
            {
                breakHandler.Play();
                smokeLeft.Play();
                smokeRight.Play();
            }
        }
        else
        {
            breakHandler.Stop();
            smokeLeft.Clear();
            smokeRight.Clear();
            smokeLeft.Pause();
            smokeRight.Pause();
        }

        ApplyBreaking(currentBreakForce);
    }

    private void ApplyBreaking(float value)
    {
        frontLeftWheelCollider.brakeTorque = currentBreakForce * currentBreakForce;
        backLeftWheelCollider.brakeTorque = currentBreakForce * currentBreakForce;
        backRightWheelCollider.brakeTorque = currentBreakForce * currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce * currentBreakForce;
    }


    private void HandleSteering()
    {
        steerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(backLeftWheelCollider, backLeftWheelTransform);
        UpdateSingleWheel(backRightWheelCollider, backRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider collider, Transform t)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        t.position = pos;
        t.rotation = rot;
    }

    private void AddDownForce()
    {
        GetComponent<Rigidbody>()
            .AddForce(-transform.up * downForceValue * GetComponent<Rigidbody>().velocity.magnitude);
    }



}