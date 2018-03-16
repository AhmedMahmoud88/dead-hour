using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayer : MonoBehaviour {

	Animator animator;
	public bool canWalk;
	public bool isDancing = false;
	public bool isRunning = false;
	float gravity;
	CharacterController controller;
	Vector3 Position;
	Quaternion rotation;
	bool canUpdate = false;
	List<Item> equippedItems;
	public WeaponsHandler weaponsHandler = new WeaponsHandler();
	public bool isPistol = false;
	public bool isRifle = false;
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
		NetPlayer character;
		public void SetMain(NetPlayer chare) {
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
			}
		}

		public void UnEquipWeapon(Types.WeaponTypes type) {
			if (type == Types.WeaponTypes.Pistol) {
				EquippedPistol.gameObject.SetActive (true);
				EquippedPistol.transform.SetParent (pistolHostler.transform);
				EquippedPistol.transform.localPosition = Vector3.zero;
				EquippedPistol.transform.localRotation = Quaternion.Euler (Vector3.zero);
				character.isPistol = false;
			} else if (type == Types.WeaponTypes.Rifle) {
				EquippedRifle.gameObject.SetActive (true);
				EquippedRifle.transform.SetParent (rifleHostler.transform);
				EquippedRifle.transform.localPosition = Vector3.zero;
				EquippedRifle.transform.localRotation = Quaternion.Euler (Vector3.zero);
				character.isRifle = false;
			}
		}
		public void AddWeapon(Weapon weaponToAdd) {
			if (weaponToAdd.WeaponType == Types.WeaponTypes.Pistol) {
				if (EquippedPistol == null) {
					EquippedPistol = weaponToAdd;
					UnEquipWeapon (Types.WeaponTypes.Pistol);
				} else {
					EquippedPistol.AddAmmo (weaponToAdd.CurrentAmmo);
				}
			} else if (weaponToAdd.WeaponType == Types.WeaponTypes.Rifle) {
				if (EquippedRifle == null) {
					EquippedRifle = weaponToAdd;
					UnEquipWeapon (Types.WeaponTypes.Rifle);
				} else {
					EquippedRifle.AddAmmo (weaponToAdd.CurrentAmmo);
				}
			}
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
		animator = GetComponent<Animator> ();
		controller = GetComponent<CharacterController> ();
		equippedItems = new List<Item> ();
		canWalk = false;
		DontDestroyOnLoad (this);
		weaponsHandler.SetMain (this);
	}
	// Update is called once per frame
	void Update () {
		HandleAnimator ();
		gravity -= 14.0f * Time.deltaTime;
		if (controller.isGrounded)
			gravity = 0;
		controller.Move (new Vector3(0, gravity, 0));
		if (canUpdate) {
			transform.position = Position;
			transform.rotation = rotation;
			canUpdate = false;
		}
	}
	void HandleAnimator() {
		animator.SetBool ("isWalking", canWalk);
		animator.SetBool ("isDancing", isDancing);
		animator.SetBool ("isRunning", isRunning);
		animator.SetBool ("isPistol", isPistol);
		animator.SetBool ("isRifle", isRifle);
	}
	public void UpdateLocation(Vector3 pos, Quaternion q) {
		this.Position = pos;
		this.rotation = q;
		canUpdate = true;
	}

	public void EquipWeapon(Weapon item) {
		item.Pickup ();
		weaponsHandler.AddWeapon (item);
		equippedItems.Add (item);
	}

	public void EquipWeapon(Types.WeaponTypes weapon) {
		weaponsHandler.EquipWeapon (weapon);
	}
	public void UnEquipWeapon(Types.WeaponTypes weapon) {
		weaponsHandler.UnEquipWeapon (weapon);
	}
	public void RemoveWeapon(Types.WeaponTypes weapon) {
		Weapon wep = GetWeaponByType (weapon);
		if (wep != null) {
			weaponsHandler.DropWeapon (wep);
			equippedItems.Remove (wep);
		} else {
			Debug.Log ("Didn't find it");
		}
	}

	Weapon GetWeaponByType(Types.WeaponTypes type) {
		foreach (Item item in equippedItems) {
			if (item.ItemType == Types.ItemTypes.Weapon) {
				Weapon wep = item as Weapon;
				if (wep.WeaponType == type) {
					return wep;
				}
			}
		}
		return null;
	}
}
