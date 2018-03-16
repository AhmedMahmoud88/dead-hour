using System;

namespace GameServer
{
	public class Item
	{
		public Types.ItemTypes ItemType;
		public float x, y, z;
		public int Id;
		public bool isItemPicked = false;
		public virtual byte[] GetPackets() {
			NetworkPacket packet = new NetworkPacket (4);
			switch (ItemType) {
			case Types.ItemTypes.Weapon:
				packet.WriteInt ((int)Messages.InventoryItem.Weapon);
				break;
			}
			return packet.ToArray ();
		}
	}
}

