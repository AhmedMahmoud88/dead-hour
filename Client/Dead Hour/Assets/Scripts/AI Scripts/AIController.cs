using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AIController : MonoBehaviour {
	protected CharacterScript player;
	protected NetPlayer netPlayer;
	protected CharacterController controller;
	protected Animator anim;
	protected bool isRunning = false;
	protected bool isChasing;
	public bool isAttackingPlayer = false;
	public bool ChaseNetPlayer;
	public int ZombieID;
	public float MovementSpeed = 6f;
	public float DetectingDistance = 10f;
	CapsuleCollider playerDetector;
	// Use this for initialization
	void Start () {
		player = FindObjectOfType<CharacterScript> ();
		netPlayer = FindObjectOfType<NetPlayer> ();
		controller = GetComponent<CharacterController> ();
		anim = GetComponent<Animator> ();
		playerDetector = GetComponent<CapsuleCollider> ();
	}
	
	protected virtual void AIBehavior() {
		//Debug.Log ("Zombie is behaving");
	}



	void OnTriggerEnter(Collider other) {
		/*if (other.tag == "Player") {
			isAttackingPlayer = true;
			isRunning = false;
			Vector3 lookRotate = Vector3.RotateTowards (transform.forward, other.transform.position - transform.position , 100f * Time.deltaTime, 0.0f);
			transform.rotation = Quaternion.LookRotation (lookRotate);
			if (isChasing) {
				NetworkManager.instance.SendAIAttacking (ZombieID);
			}
		}*/
	}

	void OnTriggerExit(Collider other) {
		/*if (other.tag == "Player") {
			isAttackingPlayer = false;
			NetworkManager.instance.SendAIStopAttacking (ZombieID);
		}*/
	}
	public virtual void MoveTolocation(byte[] packet) {


		//move to the zombie location and sets is running = true
	}
	void Update() {
		anim.SetBool ("isRunning", isRunning);
		anim.SetBool ("isAttacking", isAttackingPlayer);
	}


	void LookForPlayer() {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		float dis = Mathf.Infinity;
		GameObject closest = null;
		for (int i = 0; i < players.Length; i++) {
			float d = Vector3.Distance (transform.position, players [i].transform.position);
			if (d <= DetectingDistance) {
				if (d < dis) {
					dis = d;
					closest = players [i];
				}
			}
		}
		if (closest != null) {
			ChasePlayer (closest);
			isChasing = true;
		} else {
			if (isChasing) {
				isRunning = false;
				isChasing = false;
			}
		}
	}
	public virtual void StopMoving() {
		isRunning = false;
		isChasing = false;
		ChaseNetPlayer = false;
	}

	public virtual void ChasePlayer(GameObject player) {
		Debug.Log ("Starting chasing a plyer " + ZombieID);
		isRunning = true;
		isChasing = true;
		Vector3 dist  = player.transform.position - transform.position;
		Vector3 lookRotate = Vector3.RotateTowards (transform.forward, dist, 10f * Time.deltaTime, 0.0f);
		controller.Move (dist.normalized * MovementSpeed * Time.deltaTime);
		transform.rotation = Quaternion.LookRotation (lookRotate);
		UpdateMyLocation ();
	}

	public virtual void MoveTo(Vector3 pos) {
		isAttackingPlayer = false;
		controller.Move (pos * Time.deltaTime);
		Vector3 lookRotate = Vector3.RotateTowards (transform.forward, pos, 10f * Time.deltaTime, 0.0f);
		transform.rotation = Quaternion.LookRotation (lookRotate);
		UpdateMyLocation ();
		isRunning = true;
		isChasing = true;
	}

	public virtual void SetMyLocation(Vector3 pos, Vector4 rotation) {
		transform.position = pos;
		transform.rotation = new Quaternion (rotation.x, rotation.y, rotation.z, rotation.w);
		isRunning = true;
	}

	void OnDrawGizmos() {
		Gizmos.DrawRay (transform.position + (Vector3.up * 1f), Vector3.forward); 
		Gizmos.DrawRay (transform.position + (Vector3.up * 1f), Vector3.back);
		Gizmos.DrawRay (transform.position + (Vector3.up * 1f), Vector3.left);
		Gizmos.DrawRay (transform.position + (Vector3.up * 1f), Vector3.right);
	}
	protected virtual void UpdateMyLocation() {
		NetworkPacket packet = new NetworkPacket (36);
		packet.WriteInt ((int)Messages.ClientPackets.UPDATE_AI_LOCATION);
		packet.WriteInt (ZombieID);
		packet.WriteFloat (transform.position.x);
		packet.WriteFloat (transform.position.y);
		packet.WriteFloat (transform.position.z);
		packet.WriteFloat (transform.rotation.x);
		packet.WriteFloat (transform.rotation.y);
		packet.WriteFloat (transform.rotation.z);
		packet.WriteFloat (transform.rotation.w);
//		NetworkManager.instance.UpdateAI (packet);
	}

	protected virtual void HurtPlayer() {
		//hurt the player and send it to the server
	}
}
