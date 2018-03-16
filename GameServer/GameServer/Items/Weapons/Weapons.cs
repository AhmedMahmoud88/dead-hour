using System;

namespace GameServer
{
	public class Weapons : Item
	{
		public Types.WeaponTypes WeaponType;
		public Weapons ()
		{
			ItemType = Types.ItemTypes.Weapon;
		}

		public override byte[] GetPackets ()
		{
			NetworkPacket packet = new NetworkPacket (8);
			packet.WriteInt ((int)Messages.InventoryItem.Weapon);
			switch (WeaponType) {
			case Types.WeaponTypes.Pistol:
				packet.WriteInt ((int)Messages.WeaponType.Pistol);
				break;
			}
			return packet.ToArray ();
		}
	}
}

