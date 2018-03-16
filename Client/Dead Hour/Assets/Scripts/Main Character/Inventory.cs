using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Inventory : MonoBehaviour {
	Slot[] Slots;
	CharacterScript player;
	void Awake() {
		Slots = new Slot[5];
		for (int i = 0; i < Slots.Length; i++) {
			Slots [i] = new Slot ();
			Slots [i].SlotId = i + 1;
		}
		player = GetComponent<CharacterScript> ();
	}
	
	public void AddItem(Item itemToAdd) {
		if (itemToAdd != null) {
			for (int i = 0; i < Slots.Length; i++) {
				if (Slots [i].HasItem () == false) {
					Slots [i].AddItem (itemToAdd);
					GameObject e = GameObject.Find ("Slot " + (i + 1));
					if (e != null) {
						GUISlot image = e.GetComponent<GUISlot> ();
						if (image != null) {
							image.SetImage (itemToAdd.ItemSprite);
						} else {
							Debug.Log (e.tag);
						}
					}
					Debug.Log ("Item is on slot " + i);
					break;
				}
			}
		}
	}

	public void SlotClicked(int i) {
		if (Slots [i - 1].HasItem ()) {
			Item item = Slots [i - 1].slotItem;
			if (item is Weapon) {
				if (player.EquipWeapon (((Weapon)item).WeaponType)) {
                    NetworkManager.instance.UseItem (i - 1);
				} else {
					NetworkManager.instance.UnUseItem (i - 1, item);
				}
			}

		} else {
			Debug.Log ("No item here " + i);
		}
	}

	public void Dropitem(int i) {
		if (Slots [i - 1].HasItem ()) {
			player.NotifyItemDropped (i -1, Slots[i - 1].slotItem);
			Vector3 dropLocation = player.transform.position + new Vector3 (0, 0, 0.5f);
			Item item = Slots [i - 1].slotItem;
			NetworkManager.instance.DropItem (i - 1, item, dropLocation);
			Slots [i - 1].slotItem = null;
			GameObject e = GameObject.Find ("Slot " + (i));
			if (e != null) {
				GUISlot image = e.GetComponent<GUISlot> ();
				if (image != null) {
					image.RemoveImage ();
					Debug.Log ("Removed Image");
				} else {
					Debug.Log (e.tag);
				}
			} 
		}
	}
}
