using System;

namespace GameServer
{
	public class Inventory
	{
		Slot[] Slots;
		public Inventory ()
		{
			Slots = new Slot[20];
			for (int i = 0; i < Slots.Length; i++) {
				Slots [i] = new Slot ();
			}
		}

		public void AddItem(Item itemToAdd) {
			if(itemToAdd != null) {
				for (int i = 0; i < Slots.Length; i++) {
					if (Slots [i].HasItem () == false) {
						Slots [i].SlotItem = itemToAdd;
						break;
					}
				}
			}
		}

		public Item UseItem(int i) {
			if (Slots [i].HasItem ()) {
				return Slots[i].SlotItem;
			} else {
				return null;
			}
		}
		public void DropItem(int i) {
			Slots [i].SlotItem = null;
		}
		public Item GetItem(int i) {
			return Slots [i].SlotItem;
		}
		public bool HasItem(int index){ 
			return Slots [index].HasItem ();
		}
		public void RemoveItem(int i) {
			Slots [i].SlotItem.isItemPicked = false;
			Slots [i].SlotItem = null;
		}
	}
}

