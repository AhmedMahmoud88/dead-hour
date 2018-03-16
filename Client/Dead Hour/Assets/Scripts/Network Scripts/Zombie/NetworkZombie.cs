using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
public class NetworkZombie : MonoBehaviour {
	CharacterController zombieController;
	CharacterScript player;
	float gravity;
	// Use this for initialization
	void Start () {
		zombieController = GetComponent<CharacterController> ();
		player = FindObjectOfType<CharacterScript> ();
		if (player != null) {
			//do some player stuff here
		}
	}
	
	// Update is called once per frame
	void Update () {
		gravity -= 9f * Time.deltaTime;
		if (zombieController.isGrounded) {
			gravity = 0f;
		}

		zombieController.Move (new Vector3 (0, gravity, 0));
		BroadcastMessage ("AIBehavior", SendMessageOptions.DontRequireReceiver);
	}
}
