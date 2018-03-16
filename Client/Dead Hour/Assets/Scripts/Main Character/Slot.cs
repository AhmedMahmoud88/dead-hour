using System.Collections;
using System.Collections.Generic;

public class Slot {

	public Item slotItem;
	bool itemEquipped;
	public int SlotId;
	public void AddItem(Item item) {
		this.slotItem = item;
	}

	public bool HasItem() {
		if (slotItem != null)
			return true;
		else
			return false;
	}
}
