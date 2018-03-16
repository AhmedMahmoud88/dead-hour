using System;
using System.Text;
namespace GameServer
{
	public class NetworkPacket
	{
		byte[] packet;
		int offset;
		public NetworkPacket (int length)
		{
			offset = 0;
			packet = new byte[length];
		}

		public void WriteInt(int number) {
			byte[] integer = BitConverter.GetBytes(number);
			for (int i = 0; i < integer.Length; i++) {
				packet [offset] = integer [i];
				offset++;
			}
		}

		public void WriteFloat(float number) {
			byte[] fNumber = BitConverter.GetBytes(number);
			for (int i = 0; i < fNumber.Length; i++) {
				packet [offset] = fNumber [i];
				offset++;
			}
		}

		public void WriteString(string String) {
			int length = String.Length;
			WriteInt (length);
			byte[] sByte = Encoding.ASCII.GetBytes (String);
			for (int i = 0; i < sByte.Length; i++) {
				packet [offset] = sByte [i];
				offset++;
			}
		}

		public int GetLength() {
			return packet.Length;
		}

		public byte[] ToArray() {
			return packet;
		}
	}
}

