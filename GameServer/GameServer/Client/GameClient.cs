using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
namespace GameServer
{
	public class GameClient
	{
		TcpClient client;
		public Player player;
		NetworkStream streamer;
		byte[] buffer = new byte[8000];
		bool isConnected = false;
		public bool inLobby = false, inGame = false;
		int LobbyID;
		public int clientID;
		public GameClient (TcpClient client, int lobbyId)
		{
			this.client = client;
			this.streamer = client.GetStream ();
			this.LobbyID = lobbyId;
			streamer.BeginRead (buffer, 0, buffer.Length, ReadCallback, null);
			isConnected = true;
			new Thread (PingConnection).Start ();
		}

		void ReadCallback(IAsyncResult rs) {
			try {
				int count;
				NetworkStream st = client.GetStream();
				count = st.EndRead (rs);
				if (count != 0) {
					byte[] packet = new byte[count];
					Buffer.BlockCopy(buffer, 0, packet,0, count);
					HandleMessage (packet);
				}
				st.BeginRead (buffer, 0, buffer.Length, ReadCallback, buffer);
			} catch(Exception e) {
				isConnected = false;
				Server.Lobbies [LobbyID].ClientDisconnected (this);
				//Console.WriteLine (e.StackTrace);
			}
		}

		void WriteCallback(IAsyncResult rs) {
			try {
				NetworkStream st = client.GetStream();
				st.WriteTimeout = 10;
				st.EndWrite(rs);
				st.Flush();
			} catch (Exception e) {
				isConnected = false;
				Server.Lobbies [LobbyID].ClientDisconnected (this);
				//Console.WriteLine (e.StackTrace);
			}
		}

		public async void Write(byte[] packet) {
			try {
				await Task.Delay(75);
				NetworkStream st = client.GetStream();
				st.BeginWrite (packet, 0, packet.Length, WriteCallback, packet);
			} catch(Exception e) {
				isConnected = false;
				Server.Lobbies [LobbyID].ClientDisconnected (this);
				//Console.WriteLine (e.StackTrace);
			}
		}
		void HandleMessage(byte[] packet) {
			int action = BitConverter.ToInt32 (packet, 0);
			switch (action) {
			case (int)Messages.ClientPackets.USER_LOGGING:
				Console.WriteLine ("New Player has connected");
				break;
			case (int)Messages.ClientPackets.USER_SPAWNED:
				SendToPlayers ();
				SendPlayers ();
				break;
			case (int)Messages.ClientPackets.USER_MOVEMENT:
				SendMovement (packet);
				break;
			case (int)Messages.ClientPackets.UPDATE_LOCATION:
				UpdateLocation (packet);
				break;
			case (int)Messages.ClientPackets.USER_LEFT:
				Disconnect ();
				break;
			case (int)Messages.ClientPackets.ACTION:
				HandleAction (packet);
				break;
			case (int)Messages.ClientPackets.PING_OK:
				break;
			case (int)Messages.ClientPackets.PICKUP_ITEM:
				PlayerPickingItem (packet);
				break;
			case (int)Messages.ClientPackets.USE_ITEM:
				HandleUseItem (packet);
				break;
			case (int)Messages.ClientPackets.DROPPED_ITEM:
				HandleDroppedItem (packet);
				break;
			case (int)Messages.ClientPackets.UNUSE_ITEM:
				HandleUnUseItem (packet);
				break;
			case (int)Messages.ClientPackets.UPDATE_AI_LOCATION:
				UpdateAILocation (packet);
				break;
			case (int)Messages.ClientPackets.AI_DETECTED_PLAYER:
				GetDetection (packet);
				break;
			case (int)Messages.ClientPackets.AI_LOST_PLAYER:
				StopAI (packet);
				break;
			case (int)Messages.ClientPackets.AI_ATTACK_PLAYER:
				AIAttack (packet);
				break;
			case (int)Messages.ClientPackets.AI_STOPPED_ATTACK:
				AIStopAttack (packet);
				break;
			default:
				Console.WriteLine ("Unkown message recieved " + action);
				break;
			}
		}
		void PingConnection() {
			while (isConnected) {
				Thread.Sleep (3000);
				NetworkPacket packet = new NetworkPacket (8);
				packet.WriteInt (packet.GetLength ());
				packet.WriteInt ((int)Messages.ServerPackets.PING_OK);
				Write (packet.ToArray());
			}
		}
		void Disconnect() {
			SendDisconnect();
			try {
				client.Close ();
				streamer.Close ();
				client = null;
				streamer = null;
				isConnected = false;
			} catch (Exception) {
				Console.WriteLine ("Couldn't close the connection");
			}
		}
		public void SendSpawn() {
			this.player = new Player ("test");
			NetworkPacket acceptPacket = new NetworkPacket (32);
			acceptPacket.WriteInt (acceptPacket.GetLength ());
			acceptPacket.WriteInt ((int)Messages.ServerPackets.LOGGING_ACCEPT);
			acceptPacket.WriteFloat (player.getX);
			acceptPacket.WriteFloat (player.getY);
			acceptPacket.WriteFloat (player.getZ);
			Write (acceptPacket.ToArray());
		}
		void SendDisconnect() {
			NetworkPacket packet = new NetworkPacket (8);
			packet.WriteInt (packet.GetLength ());
			packet.WriteInt ((int)Messages.ServerPackets.PLAYER_DISCONNECTED);
			if (Server.Lobbies[LobbyID].Clients.Length > 1) {
				foreach (GameClient client in Server.Lobbies[LobbyID].Clients) {
					if (client == this)
						continue;
					client.Write (packet.ToArray ());
				}
			}
			Server.Lobbies [LobbyID].ClientDisconnected (this);
		}
		public void SendToPlayers() {
			if (Server.Lobbies[LobbyID].Clients.Length > 1) {
				NetworkPacket packet = new NetworkPacket (40);
				packet.WriteInt (packet.GetLength ());
				packet.WriteInt ((int)Messages.ServerPackets.NEW_PLAYER);
				packet.WriteFloat (player.getX);
				packet.WriteFloat (player.getY);
				packet.WriteFloat (player.getZ);
				packet.WriteFloat (player.rx);
				packet.WriteFloat (player.ry);
				packet.WriteFloat (player.rz);
				packet.WriteFloat (player.rw);
				packet.WriteInt (0);
				foreach(GameClient client in Server.Lobbies[LobbyID].Clients) {
					if (client == this) {
						continue;
					}
					client.Write (packet.ToArray ());
				}
			}

		}
		public void SendPlayers() {
			if (Server.Lobbies[LobbyID].Clients.Length > 1) {
				foreach (GameClient client in Server.Lobbies[LobbyID].Clients) {
					if (client == this)
						continue;
					float x = client.player.getX;
					float y = client.player.getY;
					float z = client.player.getZ;
					float rx = client.player.rx;
					float ry = client.player.ry;
					float rz = client.player.rz;
					float rw = client.player.rw;
					NetworkPacket packet = new NetworkPacket (40);
					packet.WriteInt (packet.GetLength ());
					packet.WriteInt ((int)Messages.ServerPackets.NEW_PLAYER);
					packet.WriteFloat (x);
					packet.WriteFloat (y);
					packet.WriteFloat (z);
					packet.WriteFloat (rx);
					packet.WriteFloat (ry);
					packet.WriteFloat (rz);
					packet.WriteFloat (rw);
					packet.WriteInt ((int)client.player.playerAction);
					Write(packet.ToArray());
				}
			}
		}
		void SendMovement(byte[] packet) {
			int movementType = BitConverter.ToInt32 (packet, 4);
			NetworkPacket movePacket = new NetworkPacket (8);
			NetworkPacket move2Packet = new NetworkPacket (8);
			movePacket.WriteInt (movePacket.GetLength ());
			move2Packet.WriteInt (move2Packet.GetLength ());
			switch (movementType) {
			case (int)Messages.MovementTypes.WALKING:
				movePacket.WriteInt ((int)Messages.ServerPackets.MOVE);
				move2Packet.WriteInt ((int)Messages.ServerPackets.PLAYER_MOVE);
				player.isWalking = true;
				player.isRunning = false;
				break;
			case (int)Messages.MovementTypes.STOPPED:
				movePacket.WriteInt ((int)Messages.ServerPackets.STOP);
				move2Packet.WriteInt ((int)Messages.ServerPackets.PLAYER_STOP);
				player.isWalking = false;
				player.isRunning = false;
				break;
			case (int)Messages.MovementTypes.RUNNING:
				movePacket.WriteInt ((int)Messages.ServerPackets.RUN);
				move2Packet.WriteInt ((int)Messages.ServerPackets.PLAYER_RUN);
				player.isRunning = true;
				player.isWalking = false;
				break;
			}
			Write (movePacket.ToArray ());
			SendToAll (move2Packet.ToArray ());
		}
		void UpdateLocation(byte[] packet) {
			if(player != null) {
				float x = BitConverter.ToSingle (packet, 4);
				float y = BitConverter.ToSingle (packet, 8);
				float z = BitConverter.ToSingle (packet, 12);
				float rx = BitConverter.ToSingle (packet, 16);
				float ry = BitConverter.ToSingle (packet, 20);
				float rz = BitConverter.ToSingle (packet, 24);
				float rw = BitConverter.ToSingle (packet, 28);
				player.SetLocation (x, y, z);
				player.SetRotation (rz, ry, rz, rw);
				SendUpdateToPlayers ();
			}
		}
		void SendUpdateToPlayers () {
			if (Server.Lobbies[LobbyID].Clients.Length > 1) {
				NetworkPacket packet = new NetworkPacket (36);
				packet.WriteInt (packet.GetLength());
				packet.WriteInt ((int)Messages.ServerPackets.UPDATE_PLAYER);
				packet.WriteFloat (player.getX);
				packet.WriteFloat (player.getY);
				packet.WriteFloat (player.getZ);
				packet.WriteFloat (player.rx);
				packet.WriteFloat (player.ry);
				packet.WriteFloat (player.rz);
				packet.WriteFloat (player.rw);
				SendToAll (packet.ToArray ());
			}
		}
		void HandleAction(byte[] actions) {
			NetworkPacket packet = new NetworkPacket(12);
			packet.WriteInt (packet.GetLength());
			packet.WriteInt ((int)Messages.ServerPackets.PLAYER_ACTION);
			packet.WriteInt (BitConverter.ToInt32 (actions, 4));
			player.playerAction = (Messages.ClientActions)BitConverter.ToInt32 (actions, 4);
			SendToAll (packet.ToArray ());
		}
		public void SendToLobby() {
			NetworkPacket packet = new NetworkPacket (8);
			packet.WriteInt (packet.GetLength());
			packet.WriteInt ((int)Messages.ServerPackets.MOVE_TO_LOBBY);
			Write (packet.ToArray ());
		}
		void SendToAll(byte[] packet) {
			if(Server.Lobbies[LobbyID].Clients.Length > 1) {
				foreach (GameClient client in Server.Lobbies[LobbyID].Clients) {
					if (client == this)
						continue;
					if (client == null)
						break;
					client.Write (packet);
				}
			}
		}

		public void PlayerPickingItem(byte[] packet) {
			int itemId = BitConverter.ToInt32 (packet, 4);
			if (itemId < 0 || itemId > Server.Lobbies[LobbyID].floorItems.Length)
				return;
			Item item = Server.Lobbies[LobbyID].GetItem (itemId);
			if (item != null) {
				if (item.isItemPicked)
					return;
				NetworkPacket net = new NetworkPacket (12);
				net.WriteInt (net.GetLength ());
				net.WriteInt ((int)Messages.ServerPackets.PICK_ITEM_OK);
				net.WriteInt (itemId);
				item.isItemPicked = true;
				player.playerInventory.AddItem (item);
				Server.Lobbies [LobbyID].floorItems [itemId] = null;
				Write (net.ToArray ());
				NetworkPacket net2 = new NetworkPacket (12);
				net2.WriteInt (net.GetLength ());
				net2.WriteInt ((int)Messages.ServerPackets.FLOOR_ITEM_REMOVE);
				net2.WriteInt (itemId);
				SendToAll (net2.ToArray ());
				if (item is Weapons) {
					NetworkPacket net3 = new NetworkPacket (16);
					net3.WriteInt (net3.GetLength ());
					net3.WriteInt ((int)Messages.ServerPackets.PLAYER_PICKED_ITEM);
					net3.WriteInt ((int)item.ItemType);
					net3.WriteInt ((int)((Weapons)item).WeaponType);
					SendToAll (net3.ToArray ());
				}
				Console.WriteLine ("Player Picked the item");
			} else {
				Console.WriteLine ("Invalid item ID " + itemId);
			}
		}

		void HandleUseItem(byte[] packet) {
			int slotId = BitConverter.ToInt32 (packet, 4);
			Item item;
			if ((item = player.playerInventory.UseItem (slotId)) != null ) {
				Console.WriteLine ("Item Used on slot " + slotId);
				NetworkPacket sendPacket = new NetworkPacket (12);
				sendPacket.WriteInt (sendPacket.GetLength ());
				sendPacket.WriteInt ((int)Messages.ServerPackets.USE_ITEM_OK);
				sendPacket.WriteInt (slotId);
				Write (sendPacket.ToArray ());
				NetworkPacket sendPacket2 = new NetworkPacket (16);
				sendPacket2.WriteInt (sendPacket2.GetLength ());
				sendPacket2.WriteInt ((int)Messages.ServerPackets.PLAYER_USED_ITEM);
				sendPacket2.WriteInt ((int)item.ItemType);
				if (item.ItemType == Types.ItemTypes.Weapon) {
					sendPacket2.WriteInt ((int)((Weapons)item).WeaponType);
				}
				SendToAll (sendPacket2.ToArray ());
			} else {
				Console.WriteLine ("Player tried to use an item on in a empty slot");
			}
		}

		void HandleDroppedItem(byte[] packet) {
			int Index = BitConverter.ToInt32 (packet, 4);
			if (player.playerInventory.HasItem (Index)) {
				float x, y, z;
				Item item = player.playerInventory.GetItem(Index);
				x = BitConverter.ToSingle (packet, 8);
				y = BitConverter.ToSingle (packet, 12);
				z = BitConverter.ToSingle (packet, 16);
				item.x = x;
				item.y = y;
				item.z = z;
				Server.Lobbies [LobbyID].AddFloorItem (item);
				player.playerInventory.RemoveItem (Index);
				NetworkPacket packet2 = new NetworkPacket(16);
				packet2.WriteInt (packet2.GetLength ());
				packet2.WriteInt ((int)Messages.ServerPackets.PLAYER_DROPPED_ITEM);
				packet2.WriteInt ((int)item.ItemType);
				if (item is Weapons) {
					packet2.WriteInt ((int)((Weapons)item).WeaponType);
				}
				SendToAll (packet2.ToArray ());
				Console.WriteLine ("Player dropped an item");
			}
		}

		void HandleUnUseItem(byte[] packet) {
			int index = BitConverter.ToInt32 (packet, 4);
			if (player.playerInventory.HasItem (index)) {
				NetworkPacket packet2 = new NetworkPacket(16);
				packet2.WriteInt (packet2.GetLength ());
				packet2.WriteInt ((int)Messages.ServerPackets.PLAYER_UNUSED_ITEM);
				Item item = player.playerInventory.GetItem (index);
				packet2.WriteInt ((int)item.ItemType);
				if (item is Weapons) {
					packet2.WriteInt ((int)((Weapons)item).WeaponType);
				}
				SendToAll (packet2.ToArray ());
				Console.WriteLine ("Player unused an item");
			}
		}
		public void SendDetection(int zombieID) {
			if (Server.Lobbies [LobbyID].Zombies [zombieID].isChasing) {
				return;
			}
			if (!Server.Lobbies [LobbyID].Zombies [zombieID].isAlive) {
				Console.WriteLine ("Zombie is not alive");
				return;
			}
			NetworkPacket packet2 = new NetworkPacket (16);
			packet2.WriteInt (packet2.GetLength ());
			packet2.WriteInt ((int)Messages.ServerPackets.AI_CHASING_PLAYER);
			packet2.WriteInt (zombieID);
			packet2.WriteInt ((int)Types.PlayerType.NetPlayer);
			SendToAll (packet2.ToArray ());
			Server.Lobbies [LobbyID].Zombies [zombieID].isChasing = true;
			Server.Lobbies [LobbyID].Zombies [zombieID].ChasingClient = this;
			Console.WriteLine ("Zombie is chasing player " + zombieID);
		}

		void UpdateAILocation(byte[] packet) {
			int zombieID = BitConverter.ToInt32 (packet, 4);
			float x = BitConverter.ToSingle (packet, 8);
			float y = BitConverter.ToSingle (packet, 12);
			float z = BitConverter.ToSingle (packet, 16);
			float rx = BitConverter.ToSingle (packet, 20);
			float ry = BitConverter.ToSingle(packet, 24);
			float rz = BitConverter.ToSingle(packet, 28);
			float rw = BitConverter.ToSingle (packet, 32);
			Server.Lobbies [LobbyID].UpdateAI (zombieID, x, y, z, rx, ry, rz, rw);
			//SendToAll (packet2.ToArray ());
		}

		void GetDetection(byte[] packet) {
			int zombieId = BitConverter.ToInt32 (packet, 4);
			SendDetection (zombieId);
		}

		void StopAI(byte[] packet) { 
			int zombieId = BitConverter.ToInt32 (packet, 4);
			Server.Lobbies [LobbyID].Zombies [zombieId].SendStopChasing ();
		}

		void AIAttack(byte[] packet) {
			int zombieId = BitConverter.ToInt32 (packet, 4);
			Server.Lobbies [LobbyID].Zombies [zombieId].isAttacking = true;
		}

		void AIStopAttack(byte[] packet) {
			int zombieId = BitConverter.ToInt32 (packet, 4);
			Server.Lobbies [LobbyID].Zombies [zombieId].isAttacking = false;
		}
	}
		
}