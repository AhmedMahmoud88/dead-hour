using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.IO;
using System;
using System.Text;
public class CharacterScript : MonoBehaviour {
	public string Name;
	//int playerNumber;
	Animator playerAnimator;
	public float RotationSpeed = 2.0f;
	public bool canWalk = false;
	bool isDancing = false;
	float gravity;
	CharacterController controller;
	bool showMessage = false;
	public bool isRunning = false;
	string messageToShow;
	CameraController c;
	Inventory inventory;
	public Weapon pistol;
	List<Item> pickupItems;
	public bool isPistol = false;
	public bool isRifle = false;
	Weapon equippedPistol;
	public float MovementSpeed;
	public float RunningSpeed;
	PlayerCanvasController CanvasController;
	public WeaponsHandler weaponHandler = new WeaponsHandler();
	public Transform chest;
	float rotationX;
	// Use this for initialization
	[System.Serializable]
	public class WeaponsHandler
	{
		Weapon EquippedPistol;
		Weapon EquippedRifle;
		public GameObject pistolHostler;
		public GameObject pistolHand;
		public GameObject rifleHostler;
		public GameObject rifleHand;
		CharacterScript character;
		public void SetMain(CharacterScript chare) {
			character = chare;
		}
		public void EquipWeapon(Types.WeaponTypes type) {
			if (type == Types.WeaponTypes.Pistol) {
				if (character.isRifle) {
					UnEquipWeapon (Types.WeaponTypes.Rifle);
					character.isRifle = false;
				}
				EquippedPistol.gameObject.SetActive (true);
				EquippedPistol.transform.SetParent (pistolHand.transform);
				EquippedPistol.transform.localPosition = Vector3.zero;
				EquippedPistol.transform.localRotation = Quaternion.Euler (Vector3.zero);
				character.isPistol = true;
				character.UpdateGUI (true, EquippedPistol);
			} else if (type == Types.WeaponTypes.Rifle) {
				if (character.isPistol) {
					UnEquipWeapon (Types.WeaponTypes.Pistol);
					character.isPistol = false;
				}
				EquippedRifle.gameObject.SetActive (true);
				EquippedRifle.transform.SetParent (rifleHand.transform);
				EquippedRifle.transform.localPosition = Vector3.zero;
				EquippedRifle.transform.localRotation = Quaternion.Euler (Vector3.zero);
				character.isRifle = true;
				character.UpdateGUI (true, EquippedRifle);
			}
		}

		public void UnEquipWeapon(Types.WeaponTypes type) {
			if (type == Types.WeaponTypes.Pistol) {
				EquippedPistol.gameObject.SetActive (true);
				EquippedPistol.transform.SetParent (pistolHostler.transform);
				EquippedPistol.transform.localPosition = Vector3.zero;
				EquippedPistol.transform.localRotation = Quaternion.Euler (Vector3.zero);
				character.isPistol = false;
				character.UpdateGUI (false, EquippedPistol);
			} else if (type == Types.WeaponTypes.Rifle) {
				EquippedRifle.gameObject.SetActive (true);
				EquippedRifle.transform.SetParent (rifleHostler.transform);
				EquippedRifle.transform.localPosition = Vector3.zero;
				EquippedRifle.transform.localRotation = Quaternion.Euler (Vector3.zero);
				character.isRifle = false;
				character.UpdateGUI (false, EquippedRifle);
			}
		}
		public bool AddWeapon(Weapon weaponToAdd) {
			if (weaponToAdd.WeaponType == Types.WeaponTypes.Pistol) {
				if (EquippedPistol == null) {
					EquippedPistol = weaponToAdd;
					UnEquipWeapon(Types.WeaponTypes.Pistol);
					return true;
				} else {
					EquippedPistol.AddAmmo (weaponToAdd.CurrentAmmo);
					Destroy (weaponToAdd.gameObject);
					character.UpdateGUI (true, EquippedPistol);
					return false;
				}
			} else if (weaponToAdd.WeaponType == Types.WeaponTypes.Rifle) {
				if (EquippedRifle == null) {
					EquippedRifle = weaponToAdd;
					UnEquipWeapon (Types.WeaponTypes.Rifle);
					return true;
				} else {
					EquippedRifle.AddAmmo (weaponToAdd.CurrentAmmo);
					Destroy (weaponToAdd.gameObject);
					character.UpdateGUI (true, EquippedRifle);
					return false;
				}
			}
			return false;
		}

		public void DropWeapon(Weapon weaponToDrop) {
			if (weaponToDrop.WeaponType == Types.WeaponTypes.Pistol) {
				Destroy (EquippedPistol.gameObject);
				this.character.isPistol = false;
			} else if (weaponToDrop.WeaponType == Types.WeaponTypes.Rifle) {
				Destroy (EquippedRifle.gameObject);
				character.isRifle = false;
			}
		}
	}
	void Start () {
//		playerNumber = 1;
		Name = "Test";
		playerAnimator = GetComponentInChildren<Animator> ();
		controller = GetComponentInChildren<CharacterController> ();
	}
	void Awake() {
		c = FindObjectOfType<CameraController> ();
		inventory = GetComponent<Inventory> ();
		DontDestroyOnLoad (this);
		pickupItems = new List<Item> ();
		CanvasController = GetComponentInChildren<PlayerCanvasController> ();
		Cursor.visible = false;
		weaponHandler.SetMain (this);
	}

		
	// Update is called once per frame
	void Update () {
		HandleMovement ();
		HandleAnimator ();
		gravity -= 9f * Time.deltaTime;
		if (controller.isGrounded)
			gravity = 0f;
		controller.Move (new Vector3 (0, gravity, 0));
		if (Input.GetKeyDown (KeyCode.E)) {
			if (pickupItems.Count > 0) {
				foreach (Item item in pickupItems) {
					NetworkManager.instance.PickupedItem (item.Id);
					pickupItems.Remove (item);
					break;
				}
			}
		}
		if (canWalk) {
			controller.Move (controller.transform.forward * MovementSpeed * Time.deltaTime);
		}
		if (isRunning) {
			controller.Move (controller.transform.forward * RunningSpeed * Time.deltaTime);
		}
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			if (equippedPistol != null) {
				equippedPistol.Fire ();
				Debug.Log ("Firing Bullets");
			}
		}
		if (Input.GetKeyDown (KeyCode.LeftAlt)) {
			if (Cursor.visible) { 
				Cursor.visible = false;
				c.FreezeCamera (false);
			} else {
				Cursor.visible = true;
				c.FreezeCamera (true);
			}
		}
		KeyboardHandle ();
	}
	void OnGUI() {
		if (showMessage) {
			GUI.Label (new Rect (0, 0, 150, 100), messageToShow);
			showMessage = false;
		}
	}
	void HandleAnimator() {
		playerAnimator.SetBool ("isWalking", canWalk);
		playerAnimator.SetBool ("isDancing", isDancing);
		playerAnimator.SetBool ("isRunning", isRunning);
		playerAnimator.SetBool ("isPistol", isPistol);
		playerAnimator.SetBool ("isRifle", isRifle);
	}
	void KeyboardHandle() {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			inventory.SlotClicked (1);
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			inventory.SlotClicked (2);
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			inventory.SlotClicked (3);
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			inventory.SlotClicked (4);
		}
		if (Input.GetKeyDown (KeyCode.Alpha5)) {
			inventory.SlotClicked (5);
		}
	}
	void OnDrawGizmos() {
		Ray r = new Ray (transform.position, transform.forward);
		Gizmos.DrawRay (r);
	}
	public void HandleMovement() {
		if (Input.GetKeyDown (KeyCode.W)) {
			if (Input.GetKey (KeyCode.LeftShift)) {
				NetworkManager.instance.SendPlayerMovement (Messages.MovementTypes.RUNNING);
			} else {
				NetworkManager.instance.SendPlayerMovement (Messages.MovementTypes.WALKING);
			}
		}
		if (Input.GetKeyUp (KeyCode.W)) {
			NetworkManager.instance.SendPlayerMovement (Messages.MovementTypes.STOPPED);
		}
		if (Input.GetAxisRaw ("Mouse X") != 0) {
			if (!c.IsFrozen ()) {
				var cameraDir = c.transform.rotation;
				cameraDir.x = 0;
				cameraDir.z = 0;
				transform.rotation = cameraDir;
			}
			//transform.Rotate (new Vector3 (0, Input.GetAxisRaw("Mouse X") * RotationSpeed, 0));
			NetworkManager.instance.SendLocation ();
		}
		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			if (Input.GetKey (KeyCode.W)) { 
				NetworkManager.instance.SendPlayerMovement (Messages.MovementTypes.RUNNING);
			}
		}
		if (Input.GetKeyUp (KeyCode.LeftShift)) {
			if (Input.GetKey (KeyCode.W)) {
				NetworkManager.instance.SendPlayerMovement (Messages.MovementTypes.WALKING);
			} else {
				NetworkManager.instance.SendPlayerMovement (Messages.MovementTypes.STOPPED);
			}

		}
		if (Input.GetKeyUp (KeyCode.T)) {
			if (canWalk || isRunning)
				return;
			if (isDancing) {
				isDancing = false;
				NetworkManager.instance.SendActions ((int)Messages.ClientActions.STOPPED_DANCING);
			} else { 
				isDancing = true;
				NetworkManager.instance.SendActions ((int)Messages.ClientActions.DANCING);
			}
		}
		if (canWalk || isRunning) {
			NetworkManager.instance.SendLocation ();
		}
	}

	public void ShowMessage(string message) {
		messageToShow = message;
		showMessage = true;
	}

	public void AddPickupItem(Item pickupItem) {
		pickupItems.Add (pickupItem);
	}

	public void RemovePickupitem(Item pickupItem) {
		pickupItems.Remove (pickupItem);
	}

	public void AddToInventory(Item item) {
		item.Pickup ();
		if (item is Weapon) {
			if (weaponHandler.AddWeapon (item as Weapon)) {
				inventory.AddItem (item);
			}
		} else {
			inventory.AddItem (item);
		}
	}
	public bool EquipWeapon(Types.WeaponTypes type) {
		if (type == Types.WeaponTypes.Pistol) {
			if (isPistol) {
				weaponHandler.UnEquipWeapon (Types.WeaponTypes.Pistol);
				isPistol = false;
				return false;
			}
			weaponHandler.EquipWeapon (Types.WeaponTypes.Pistol);
			isPistol = true;
			return true;
		} else if (type == Types.WeaponTypes.Rifle) {
			if (isRifle) {
				weaponHandler.UnEquipWeapon (Types.WeaponTypes.Rifle);
				isRifle = false;
				return false;
			}
			weaponHandler.EquipWeapon (Types.WeaponTypes.Rifle);
			isRifle = true;
			return true;
		}
		return false;
	}

	public void NotifyItemDropped(int Index, Item item) {
		if (item is Weapon) {
			weaponHandler.DropWeapon (item as Weapon);
		}
	}
	public void UpdateGUI(bool show, Weapon wep) {
		if (show) {
			CanvasController.EnableWeaponInfo (true);
			CanvasController.SetWeaponName (wep.name);
			CanvasController.SetWeaponAmmo (wep.CurrentClip, wep.CurrentAmmo);
		} else {
			CanvasController.EnableWeaponInfo (false);
		}
	}

	void OnAnimatorIK(int layerindex) {
		if (!c.IsFrozen()) {
			rotationX += Input.GetAxis ("Mouse Y") * Time.deltaTime;
			rotationX = Mathf.Clamp (rotationX, -0.5f, 0.5f);
			Vector3 lookAt = new Vector3 (transform.position.x, rotationX, transform.position.z + 1f);
			Vector3 directionToLook = lookAt - transform.position;
			Quaternion look = Quaternion.LookRotation (directionToLook);
			playerAnimator.SetBoneLocalRotation (HumanBodyBones.UpperChest, look);
		}
	}
}
