using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	private GameObject objectToFollow;
	public Vector3 offset;
    private Vector3 initialOffset;
    public float transitionSpeed;
	public float followSpeed = 10;
	public float lookSpeed = 10;

    private void Awake()
    {

        initialOffset = offset;
        objectToFollow = this.transform.parent.gameObject;
    }
    

    private void LookAtTarget()
        {
            var lookDirection = objectToFollow.transform.position - transform.position;
            var rot = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, lookSpeed * Time.deltaTime);
        }

    private void MoveToTarget()
        {
            var objectToFollowTransform = objectToFollow.transform;
            var targetPos = objectToFollowTransform.position + 
                             objectToFollowTransform.forward * offset.z + 
                             objectToFollowTransform.right * offset.x + 
                             objectToFollowTransform.up * offset.y;
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }

        private void FixedUpdate()
        {

            LookAtTarget();
            MoveToTarget();

        }

}
