using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
public class NetworkManager : MonoBehaviour {

	/// <summary>
	/// Old Code 
	/// </summary>
	public static NetworkManager instance;
	[Header("Player Prefab")]
	public GameObject playerPrefab;
	public GameObject netPlayerPrefab;
	public PrefabsHandler PrefabHandler = new PrefabsHandler();
	public AIHandler AIsHandler = new AIHandler ();
	GameObject currentPlayer;
	GameObject netPlayer;
	TcpClient socketClient;
	NetworkStream socketStream;
	CharacterScript playerScript;
	NetPlayer netPlayerScript;
	byte[] ReadBuffer;
	Vector3 playerPos, player2Pos, floorItemPos;
	Quaternion player2Rot;
	int netPlayerAction;
	bool isConnected = false;
	bool showMessage = false;
	bool isLevelStarted = false;
	bool moveToLobby = false;
	bool canSpawn = false, canSpawnOther = false, otherDced = false;

	Dictionary<int, Item> floorItems = new Dictionary<int, Item>();
	Dictionary<int, AIController> SpawnedAIs = new Dictionary<int, AIController>();
	//bool canWorkoffline = true;
	string messageToShow;
	// Use this for initialization
	[System.Serializable]
	public class PrefabsHandler {
		[Header("Prefabs")]
		public Item[] floorItems;

		public Item GetWeapon(Types.WeaponTypes type) {
			for(int i = 0; i < floorItems.Length; i++) {
				if(floorItems[i].ItemType == Types.ItemTypes.Weapon) {
					Weapon wep = floorItems [i] as Weapon;
					if (wep.WeaponType == type) {
						return wep;
					}
				}
			}
			return null;
		}
	}

	[System.Serializable]
	public class AIHandler {
		[Header("AI Prefabs")]
		public AIController[] AIs;
	}
	delegate void SpawnObject(GameObject objectToSpawn, Vector3 Pos, Quaternion rotation);
	Queue<Action> MethodQueue = new Queue<Action>();
	void Awake() {
		if (instance == null) {
			instance = this;
		} else {
			Destroy (this.gameObject);
			return;
		}
		DontDestroyOnLoad (this);
		socketClient = new TcpClient ();
		socketClient.BeginConnect ("127.0.0.1", 2550, OnConnect, null);
		socketClient.ReceiveBufferSize = 4096;
		socketClient.SendBufferSize = 4096;
		Array.Resize (ref ReadBuffer, socketClient.ReceiveBufferSize);
	}
	void OnConnect(IAsyncResult rs) {
		if (socketClient.Connected) {
			isConnected = true;
			socketStream = socketClient.GetStream ();
			socketStream.BeginRead (ReadBuffer, 0, ReadBuffer.Length, OnReceive, null);
			NetworkPacket packet = new NetworkPacket (4);
			packet.WriteInt ((int)Messages.ClientPackets.USER_LOGGING);
			Send (packet.ToArray ());
			Debug.Log ("Connected");
		} else {
			Debug.Log("Couldn't connect to 127.0.0.1");
		}
	}

	void OnReceive(IAsyncResult rs) {
		int count = socketStream.EndRead (rs);
		if (count == 0) {
			return;
		}
		if (socketClient == null) {
			isConnected = false;
			return;
		}
		byte[] recievedBuffer = new byte[count];
		Buffer.BlockCopy (ReadBuffer, 0, recievedBuffer, 0, count);
		HandlePacket (recievedBuffer);
		socketStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnReceive, null);
	}
	void OnWrite(IAsyncResult rs) {
		try {
			socketStream.EndWrite (rs);
		} catch (Exception e) {
			Debug.Log (e.ToString());
		}
	}
	void Send(byte[] packet) {
		if (isConnected && socketClient != null && socketStream != null) {
			socketStream.BeginWrite (packet, 0, packet.Length, OnWrite, null);
		}
	}
	void OnApplicationQuit() {
		QuitConnection ();
	}
	void HandlePacket(byte[] packet) {
		//Debug.Log ("Recievd Packet");
		int length = BitConverter.ToInt32 (packet, 0);
		int action = BitConverter.ToInt32 (packet, 4);
		//Debug.Log ("Recievd Packet length and action " + action);
		switch (action) {
		case (int)Messages.ServerPackets.LOGGING_ACCEPT:
			//Console.WriteLine ("Shoud load the level");
			SpawnPlayer (packet);
			break;
		case (int)Messages.ServerPackets.MOVE:
		case (int)Messages.ServerPackets.STOP:
		case (int)Messages.ServerPackets.RUN:
			HandleMovement (packet);
			showMessage = true;
			messageToShow = "Moving";
			break;
		case (int)Messages.ServerPackets.NEW_PLAYER:
			Player2Connected (packet);
			break;
		case (int)Messages.ServerPackets.PING_OK:
			SendOK ();
			break;
		case (int)Messages.ServerPackets.UPDATE_PLAYER:
			UpdateNetPlayer (packet);
			break;
		case (int)Messages.ServerPackets.PLAYER_MOVE:
		case (int)Messages.ServerPackets.PLAYER_RUN:
		case (int)Messages.ServerPackets.PLAYER_STOP:
			NetPlayerMovement (packet);
			break;
		case (int)Messages.ServerPackets.PLAYER_ACTION:
			NetPlayerAction (packet);
			break;
		case (int)Messages.ServerPackets.PLAYER_DISCONNECTED:
			NetPlayerLeft ();
			break;
		case (int)Messages.ServerPackets.MOVE_TO_LOBBY:
			moveToLobby = true;
			break;
		case (int)Messages.ServerPackets.FLOOR_ITEM:
			//Debug.Log ("Spawned ");
			SpawnFloorItem (packet);
			break;
		case (int)Messages.ServerPackets.PICK_ITEM_OK:
			PickupItem (packet);
			break;
		case (int)Messages.ServerPackets.FLOOR_ITEM_REMOVE:
			RemoveFloorItem (packet);
			break;
		case (int)Messages.ServerPackets.USE_ITEM_OK:
			UseItem (packet);
			break;
		case (int)Messages.ServerPackets.PLAYER_PICKED_ITEM:
			PlayerPickedItem (packet);
			break;
		case (int)Messages.ServerPackets.PLAYER_USED_ITEM:
			PlayerUsedItem (packet);
			break;
		case (int)Messages.ServerPackets.PLAYER_UNUSED_ITEM:
			UnUseItem (packet);
			break;
		case (int)Messages.ServerPackets.PLAYER_DROPPED_ITEM:
			DropItem (packet);
			break;
		case (int)Messages.ServerPackets.SPAWN_AI:
			SpawnAI (packet);
			break;
		case (int)Messages.ServerPackets.AI_CHASING_PLAYER:
			AIChasing (packet);
			break;
		case (int)Messages.ServerPackets.AI_STOPPED:
			AIStopped (packet);
			break;
		case (int)Messages.ServerPackets.AI_LOCATION:
			UpdateAILocation (packet);
			break;
		case (int)Messages.ServerPackets.AI_ATTACK:
			AIAttack (packet);
			break;
		case (int)Messages.ServerPackets.AI_STOP_ATTACK:
			AIStopAttack (packet);
			break;
		default:
			Debug.Log ("Unkown Message " + action);
			showMessage = true;
			messageToShow = "Unkown Message " + action;
			break;
		}
		if (packet.Length > length) {
			byte[] packet2 = new byte[packet.Length - length];
			Buffer.BlockCopy (packet, length, packet2, 0, packet2.Length);
			HandlePacket (packet2);
			//Debug.Log ("Unpacking the other one too ");
		}
	}
	void QuitConnection() {
		NetworkPacket packet = new NetworkPacket (4);
		packet.WriteInt ((int)Messages.ClientPackets.USER_LEFT);
		Send (packet.ToArray());
		socketClient.Close ();
		socketStream.Close ();
	}

	//Update frames ********* 

	void Update() {
		if (isLevelStarted) {
			SceneManager.LoadScene (1);
			if (canSpawn && currentPlayer == null) {
				currentPlayer = Instantiate (playerPrefab, playerPos, Quaternion.identity);
				playerScript = currentPlayer.GetComponent<CharacterScript> ();
				canSpawn = false;
			}
			isLevelStarted = false;
		}
		if (canSpawnOther && netPlayer == null) {
			netPlayer = Instantiate (netPlayerPrefab, player2Pos, player2Rot);
			netPlayerScript = netPlayer.GetComponent<NetPlayer> ();
			switch (netPlayerAction) {
			case (int)Messages.ClientActions.DANCING:
				netPlayerScript.isDancing = true;
				break;
			case (int)Messages.ClientActions.STOPPED_DANCING:
				netPlayerScript.isDancing = false;
				break;
			case (int)Messages.ClientActions.IDLE:
				netPlayerScript.isDancing = false;
				netPlayerScript.canWalk = false;
				break;
			default:
				Debug.Log ("Unkown Action " + netPlayerAction);
				break;
			}
			canSpawnOther = false;
		}
		if (otherDced) {
			Destroy (netPlayer);
			netPlayer = null;
			netPlayerScript = null;
			otherDced = false;
		}
		if (moveToLobby) {
			Destroy (currentPlayer);
			currentPlayer = null;
			netPlayer = null;
			playerScript = null;
			netPlayerScript = null;
			for (int i = 0; i < floorItems.Count; i++) {
				Item item;
				if (floorItems.TryGetValue (i, out item)) {
					Destroy (item.gameObject);
				}
			}
			floorItems.Clear ();
			SceneManager.LoadScene (0);
			moveToLobby = false;
		}
		if (MethodQueue.Count > 0) {
			MethodQueue.Dequeue ().Invoke();
		}
	}

	void FixedUpdate() {

	}
	void OnGUI() {
		if (showMessage) {
			GUI.Label (new Rect(0,0,150,100), messageToShow);
		}
	}
	void SpawnPlayer(byte[] packet) {
		isLevelStarted = true;
		float x = BitConverter.ToSingle (packet, 8);
		float y = BitConverter.ToSingle (packet, 12);
		float z = BitConverter.ToSingle (packet, 16);
		playerPos = new Vector3 (x, y, z);
		canSpawn = true;
		NetworkPacket e = new NetworkPacket(4);
		e.WriteInt((int)Messages.ClientPackets.USER_SPAWNED);
		Send (e.ToArray ());
	}
	void HandleMovement(byte[] packet) {
		if (currentPlayer != null && playerScript != null) {
			int isMoving = BitConverter.ToInt32 (packet, 4);
			if (isMoving == (int)Messages.ServerPackets.MOVE) {
				playerScript.canWalk = true;
				playerScript.isRunning = false;
			} else if (isMoving == (int)Messages.ServerPackets.STOP) {
				playerScript.canWalk = false;
				playerScript.isRunning = false;
			} else if (isMoving == (int)Messages.ServerPackets.RUN) {
				playerScript.isRunning = true;
				playerScript.canWalk = false;
			}
			else {
				showMessage = true;
				messageToShow = "Walk::Unkown Message";
			}
		}
	}
	public void SendPlayerMovement(Messages.MovementTypes action) {
		NetworkPacket movementPacket = new NetworkPacket(8);
		movementPacket.WriteInt ((int)Messages.ClientPackets.USER_MOVEMENT);
		movementPacket.WriteInt ((int)action);
		Send (movementPacket.ToArray ());
	}

	public void SendLocation() {
		if (currentPlayer != null) {
			Vector3 playerPos = currentPlayer.transform.position;
			Quaternion playerRot = currentPlayer.transform.rotation;
			NetworkPacket packet = new NetworkPacket (32);
			packet.WriteInt ((int)Messages.ClientPackets.UPDATE_LOCATION);
			packet.WriteFloat (playerPos.x);
			packet.WriteFloat (playerPos.y);
			packet.WriteFloat (playerPos.z);
			packet.WriteFloat (playerRot.x);
			packet.WriteFloat (playerRot.y);
			packet.WriteFloat (playerRot.z);
			packet.WriteFloat (playerRot.w);
			Send (packet.ToArray ());
		}
	}

	public void Player2Connected(byte[] packet) {
		Debug.Log ("New Player Connected");
		messageToShow = "Player Connected";
		showMessage = true;
		float x = BitConverter.ToSingle (packet, 8);
		float y = BitConverter.ToSingle (packet, 12);
		float z = BitConverter.ToSingle (packet, 16);
		float rx = BitConverter.ToSingle (packet, 20);
		float ry = BitConverter.ToSingle (packet, 24);
		float rz = BitConverter.ToSingle (packet, 28);
		float rw = BitConverter.ToSingle (packet, 32);
		player2Pos = new Vector3 (x, y, z);
		player2Rot = new Quaternion (rx, ry, rz, rw);
		canSpawnOther = true;
		netPlayerAction = BitConverter.ToInt32 (packet, 36);
	}

	void SendOK() {
		NetworkPacket packet = new NetworkPacket (4);
		packet.WriteInt ((int)Messages.ClientPackets.PING_OK);
		Send (packet.ToArray ());
	}

	void UpdateNetPlayer(byte[] packet) {
		float x = BitConverter.ToSingle (packet, 8);
		float y = BitConverter.ToSingle (packet, 12);
		float z = BitConverter.ToSingle (packet, 16);
		float rx = BitConverter.ToSingle (packet, 20);
		float ry = BitConverter.ToSingle (packet, 24);
		float rz = BitConverter.ToSingle (packet, 28);
		float rw = BitConverter.ToSingle (packet, 32);
		player2Pos = new Vector3 (x, y, z);
		player2Rot = new Quaternion (rx, ry, rz, rw);
		netPlayerScript.UpdateLocation (player2Pos, player2Rot);
	}

	void NetPlayerMovement(byte[] packet) {
		int movementType = BitConverter.ToInt32 (packet, 4);
		switch (movementType) {
		case (int)Messages.ServerPackets.PLAYER_MOVE:
			netPlayerScript.canWalk = true;
			netPlayerScript.isRunning = false;
			break;
		case (int)Messages.ServerPackets.PLAYER_STOP:
			netPlayerScript.canWalk = false;
			netPlayerScript.isRunning = false;
			break;
		case (int)Messages.ServerPackets.PLAYER_RUN:
			netPlayerScript.isRunning = true;
			netPlayerScript.canWalk = false;
			break;
		}
	}

	void NetPlayerAction(byte[] packet) {
		if (netPlayerScript != null) {
			int action = BitConverter.ToInt32 (packet, 8);
			switch (action) {
			case (int)Messages.ClientActions.DANCING:
				netPlayerScript.isDancing = true;
				break;
			case (int)Messages.ClientActions.STOPPED_DANCING:
				netPlayerScript.isDancing = false;
				break;
			default:
				Debug.Log ("Unkown player action " + action);
				break;
			}
		}
	}

	public void SendActions(int actionType) {
		NetworkPacket packet = new NetworkPacket (8);
		packet.WriteInt ((int)Messages.ClientPackets.ACTION);
		packet.WriteInt (actionType);
		Send (packet.ToArray ());
	}
	void NetPlayerLeft() {
		otherDced = true;
	}

	void HandleInventory(byte[] packet) {

	}

	void SpawnFloorItem(byte[] packet) {
		int ID = BitConverter.ToInt32 (packet, 8);
		int ItemType = BitConverter.ToInt32 (packet, 12);
		float x = 0, y = 0, z = 0;
		Item itemToSpawn = null;
		if (ItemType == (int)Types.ItemTypes.Weapon) {
			int WeaponType = BitConverter.ToInt32 (packet, 16);
			itemToSpawn = PrefabHandler.GetWeapon ((Types.WeaponTypes)WeaponType);
			 x = BitConverter.ToSingle (packet, 20);
			 y = BitConverter.ToSingle (packet, 24);
			 z = BitConverter.ToSingle (packet, 28);
		} else {
			x = BitConverter.ToSingle (packet, 16);
			y = BitConverter.ToSingle (packet, 20);
			z = BitConverter.ToSingle (packet, 24);
		}
		floorItemPos = new Vector3 (x, y, z);
		if (itemToSpawn != null) {
			MethodQueue.Enqueue (() => {
				GameObject item = Instantiate (itemToSpawn.gameObject, new Vector3 (x, y, z), Quaternion.identity);
				Item floorItem = item.GetComponent<Item> ();
				floorItem.Id = ID;
				Debug.Log("ID is " + ID);
				floorItems.Add (ID, floorItem);
			});
		} else {
			Debug.Log ("Didn't work");
		}
	}

	public void PickupedItem(int Id) {
		NetworkPacket packet = new NetworkPacket (8);
		packet.WriteInt ((int)Messages.ClientPackets.PICKUP_ITEM);
		packet.WriteInt (Id);
		Send (packet.ToArray ());
	}

	public void PickupItem(byte[] packet) {
		int Id = BitConverter.ToInt32 (packet, 8);
		Item item;
		if (floorItems.ContainsKey (Id)) {
			if (floorItems.TryGetValue (Id, out item)) {
				//playerScript.AddToInventory(item);
				MethodQueue.Enqueue (() => {
					playerScript.AddToInventory (item);
					floorItems.Remove (Id);
				});
			} else {
				Debug.Log ("Couldn't get the item");
			}
		}
	}

	public void RemoveFloorItem(byte[] packet) {
		//Debug.Log ("removed item");
		int Id = BitConverter.ToInt32 (packet, 8);
		Item item;
		if (floorItems.ContainsKey (Id)) {
			if (floorItems.TryGetValue (Id, out item)) {
				MethodQueue.Enqueue (() => {
					Destroy(item.gameObject);
					floorItems.Remove (Id);
				});
			}
		}
	}

	public void PlayerPickedItem(byte[] packet) {
		int itemType = BitConverter.ToInt32 (packet, 8);
		if (itemType == (int)Types.ItemTypes.Weapon) {
			int Weapontype = BitConverter.ToInt32 (packet, 12);
			MethodQueue.Enqueue (() => {
				GameObject inst = Instantiate(PrefabHandler.GetWeapon((Types.WeaponTypes)Weapontype).gameObject, Vector3.zero, Quaternion.identity);
				netPlayerScript.EquipWeapon(inst.GetComponent<Item>() as Weapon);
			});
		}
	}
	public void UseItem(int slotNumber) {
		NetworkPacket packet = new NetworkPacket (8);
		packet.WriteInt ((int)Messages.ClientPackets.USE_ITEM);
		packet.WriteInt (slotNumber);
		Send (packet.ToArray ());
	}

	public void UseItem(byte[] packet) {
		int slotId = BitConverter.ToInt32 (packet, 8);
		Debug.Log("Used the item on " +  slotId);
	}
	void PlayerUsedItem(byte[] packet) {
		int itemType = BitConverter.ToInt32 (packet, 8);
		if (itemType == (int)Types.ItemTypes.Weapon) {
			int WeaponType = BitConverter.ToInt32 (packet, 12);
			MethodQueue.Enqueue (() => {
				netPlayerScript.EquipWeapon((Types.WeaponTypes)WeaponType);
			});
		}
	}

	public void UnUseItem(int Index, Item item) {
		NetworkPacket packet = new NetworkPacket (8);
		packet.WriteInt ((int)Messages.ClientPackets.UNUSE_ITEM);
		packet.WriteInt (Index);
		Send (packet.ToArray ());
	}

	public void UnUseItem(byte[] packet) {
		int itemType = BitConverter.ToInt32 (packet, 8);
		if (itemType == (int)Types.ItemTypes.Weapon) {
			int weaponType = BitConverter.ToInt32 (packet, 12);
			MethodQueue.Enqueue (() => {
				netPlayerScript.UnEquipWeapon ((Types.WeaponTypes)weaponType);
			});
		}
	}
	public void DropItem(int Index, Item itemToDrop, Vector3 dropLocation) {
		NetworkPacket packet = new NetworkPacket (20);
		packet.WriteInt ((int)Messages.ClientPackets.DROPPED_ITEM);
		packet.WriteInt ((int)Index);
		packet.WriteFloat (dropLocation.x);
		packet.WriteFloat (dropLocation.y);
		packet.WriteFloat (dropLocation.z);
		Send (packet.ToArray ());
	}

	public void DropItem(byte[] packet) {
		int itemType = BitConverter.ToInt32 (packet, 8);
		if (itemType == (int)Types.ItemTypes.Weapon) {
			int WeaponType = BitConverter.ToInt32 (packet, 12);
			MethodQueue.Enqueue (() => {
				netPlayerScript.RemoveWeapon((Types.WeaponTypes)WeaponType);
			});
		}
	}

	void SpawnAI(byte[] packet) {
		int zombieId = BitConverter.ToInt32 (packet, 8);
		Vector3 zombielocation = new Vector3 ();
		zombielocation.x = BitConverter.ToSingle (packet, 12);
		zombielocation.y = BitConverter.ToSingle (packet, 16);
		zombielocation.z = BitConverter.ToSingle (packet, 20);
		Quaternion qt = new Quaternion ();
		qt.x = BitConverter.ToSingle (packet, 24);
		qt.y = BitConverter.ToSingle (packet, 28);
		qt.z = BitConverter.ToSingle (packet, 32);
		qt.w = BitConverter.ToSingle (packet, 36);
		MethodQueue.Enqueue (() => {
			GameObject init = Instantiate(AIsHandler.AIs[0].gameObject, zombielocation, qt);
			AIController controller = init.GetComponent<AIController>();
			controller.ZombieID = zombieId;
			SpawnedAIs.Add(zombieId, controller);
		});
		Debug.Log ("Spawned AI");
	}

	void AIChasing(byte[] packet) {
		int zombieID = BitConverter.ToInt32 (packet, 8);
		AIController controller = null;
		if (SpawnedAIs.TryGetValue (zombieID, out controller)) {
			int type = BitConverter.ToInt32 (packet, 12);
			if (type == (int)Types.PlayerType.NetPlayer) {
				MethodQueue.Enqueue (() => {
					controller.ChaseNetPlayer = true;
				});
			}

		}
	}

	void AIStopped(byte[] packet) {
		int zombieID = BitConverter.ToInt32 (packet, 8);
		AIController controller = null;
		if (SpawnedAIs.TryGetValue (zombieID, out controller)) {
			MethodQueue.Enqueue (() => {
				controller.StopMoving();
			});
		}
	}
	void AIAttack(byte[] packet) {
		int zombieID = BitConverter.ToInt32 (packet, 8);
		AIController controller = null;
		if (SpawnedAIs.TryGetValue (zombieID, out controller)) {
			MethodQueue.Enqueue (() => {
				controller.isAttackingPlayer = true;
			});
		}
	}
	void AIStopAttack(byte[] packet) {
		int zombieID = BitConverter.ToInt32 (packet, 8);
		AIController controller = null;
		if (SpawnedAIs.TryGetValue (zombieID, out controller)) {
			MethodQueue.Enqueue (() => {
				controller.isAttackingPlayer = false;
			});
		}
	}
	public void UpdateAI(NetworkPacket packet) {
		Send (packet.ToArray ());
	}

	public void SendAIDetect(int zombieID, Types.PlayerType type) {
		NetworkPacket packet = new NetworkPacket (12);
		packet.WriteInt ((int)Messages.ClientPackets.AI_DETECTED_PLAYER);
		packet.WriteInt (zombieID);
		packet.WriteInt ((int)type);
		Send (packet.ToArray ());
	}

	public void SendAIStopped(int zombieID) {
		NetworkPacket packet = new NetworkPacket (12);
		packet.WriteInt ((int)Messages.ClientPackets.AI_LOST_PLAYER);
		packet.WriteInt (zombieID);
		Send (packet.ToArray ());
	}

	public void SendAIAttacking(int zombieID) {
		NetworkPacket packet = new NetworkPacket (12);
		packet.WriteInt ((int)Messages.ClientPackets.AI_ATTACK_PLAYER);
		packet.WriteInt (zombieID);
		Send (packet.ToArray ());
	}
	public void SendAIStopAttacking(int zombieID) {
		NetworkPacket packet = new NetworkPacket (12);
		packet.WriteInt ((int)Messages.ClientPackets.AI_STOPPED_ATTACK);
		packet.WriteInt (zombieID);
		Send (packet.ToArray ());
	}
	void UpdateAILocation(byte[] packet) {
		int zombieId = BitConverter.ToInt32 (packet, 8);
		AIController controller = null;
		if (SpawnedAIs.TryGetValue (zombieId, out controller)) {
			float x = BitConverter.ToSingle (packet, 12);
			float y = BitConverter.ToSingle (packet, 16);
			float z = BitConverter.ToSingle (packet, 20);
			MethodQueue.Enqueue (() => {
			//	controller.SetMyLocation(new Vector3(x,y,z), new Vector4(rx,ry,rz,rw));
				controller.MoveTo(new Vector3(x,y,z));
			});
		}
	}
}
