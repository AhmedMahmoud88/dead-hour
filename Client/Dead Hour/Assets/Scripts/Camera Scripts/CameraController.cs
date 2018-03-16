using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour {

	public GameObject objectToFollow;
	public Vector3 targetOffest = new Vector3(0, 2f, 0);
	public float LookSmooth = 300f;
	private Vector3 targetPos = Vector3.zero;
	private Vector3 dest = Vector3.zero;
	public float rotationX = 0f, rotationY = -180f, rotateSpeed = 5f;
	private bool isFrozen = false;
	// Use this for initialization


	// Update is called once per frame
	void Update () {
		MoveToTarget ();
		LookAt ();
		OrbitCamera ();
	}
	protected void MoveToTarget() {
		if (objectToFollow != null) {
			if (!isFrozen) {
				targetPos = objectToFollow.transform.position + targetOffest;
				dest = Quaternion.Euler (rotationX, rotationY, 0) * -Vector3.forward * -2f;
				dest += targetPos;
				transform.position = dest;
			}
		}
	}
	protected void LookAt() {
		Quaternion targetRotation = Quaternion.LookRotation (targetPos - transform.position);
		transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, LookSmooth * Time.deltaTime);
	}

	void OrbitCamera() {
		rotationX += Input.GetAxis ("Mouse Y") * rotateSpeed * Time.deltaTime;
		rotationX = Mathf.Clamp (rotationX, -20, 20);
		rotationY += Input.GetAxis ("Mouse X") * rotateSpeed * Time.deltaTime;
	}

	public Vector3 GetFacingDirection(Vector3 direction) {
		return transform.TransformDirection (direction);
	}

	public void FreezeCamera(bool freeze) {
		isFrozen = freeze;
	}

	public bool IsFrozen() {
		return isFrozen;
	}

}

