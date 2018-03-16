using System;

namespace GameServer
{
	public class Types
	{
		public enum ItemTypes { 
			Weapon = 1,
		}

		public enum WeaponTypes {
			Pistol = 1,
			Rifle,
			Bow
		}

		public enum PlayerType: int {
			MainPlayer = 1,
			NetPlayer
		}
	}
}

