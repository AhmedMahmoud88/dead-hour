using System;

namespace GameServer
{
	public class Slot
	{
		public Item SlotItem;
		public bool HasItem() {
			if (SlotItem == null)
				return false;
			else
				return true;
		}
	}
}

