using System;
using System.Collections.Generic;
namespace GameServer
{
	public class Player
	{
		protected String name;
		protected float x, y, z;
		public float rx, ry, rz, rw;
		public float getX {get {return x;}}
		public float getY {get {return y;}}
		public bool isWalking = false;
		public bool isRunning = false;
		public string getName { get{ return name;}} 
		public float getZ { get { return z; } }
		public Messages.ClientActions playerAction;
		public Inventory playerInventory = new Inventory();
		List<Item> EquippedItems = new List<Item>();
		public Player (String name)
		{
			this.name = name;
			x = 0;
			y = 0;
			z = 0;
		}

		public void SetLocation(float x, float y, float z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public void SetRotation(float rx, float ry, float rz, float rw) {
			this.rx = rx;
			this.ry = ry;
			this.rz = rz;
			this.rw = rw;
		}

		public byte[] GetPacket() {
			byte[] packet = new byte[28];
			byte[] bx = BitConverter.GetBytes (x);
			byte[] by = BitConverter.GetBytes (y);
			byte[] bz = BitConverter.GetBytes (z);
			byte[] brx = BitConverter.GetBytes (rx);
			byte[] bry = BitConverter.GetBytes (ry);
			byte[] brz = BitConverter.GetBytes (rz);
			int offset = 0;
			for (int i = 0; i < bx.Length; i++) {
				packet [offset] = bx [i];
				offset++;

			}
			for (int i = 0; i < by.Length; i++) {
				packet [offset] = by [i];
				offset++;

			}
			for (int i = 0; i < bz.Length; i++) {
				packet [offset] = bz [i];
				offset++;
			}
			for (int i = 0; i < brx.Length; i++) {
				packet [offset] = brx [i];
				offset++;
			}
			for (int i = 0; i < bry.Length; i++) {
				packet [offset] = bry [i];
				offset++;
			}
			for (int i = 0; i < brz.Length; i++) {
				packet [offset] = brz [i];
				offset++;
			}
			return packet;
		}
	}
}

