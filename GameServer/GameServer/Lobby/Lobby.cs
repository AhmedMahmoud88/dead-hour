using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
namespace GameServer
{
	public class Lobby
	{
		public GameClient[] Clients = new GameClient[2];
		public int Id;
		public bool IsGameStarted = false;
		public Item[] floorItems;
		public Zombie[] Zombies = new Zombie[100];
		System.Random random = new System.Random();
		bool InitingItems = false;
		public void StartGame() {
			if (IsGameStarted)
				return;
			for (int i = 0; i < Clients.Length; i++) {
				Clients [i].SendSpawn ();
			}
			InitFloorItems (); 
			InitZombies ();
			SendFloorItems ();
			SendAIs ();
			IsGameStarted = true;
			Console.WriteLine ("Lobby " + Id + " Started the game ");
			Server.serverInstance.UpdateTitle ();
		}
		public bool IsEmpty() {
			for (int i = 0; i < Clients.Length; i++) {
				if (Clients [i] == null)
					return true;
			}
			return false;
		}

		public void AddPlayer(TcpClient socket) {
			for (int i = 0; i < Clients.Length; i++) {
				if (Clients[i] == null) {
					Clients[i] = new GameClient (socket, Id);
					Clients[i].inLobby = true;
					Clients [i].clientID = i;
					Console.WriteLine("Lobby " + Id + " Player Entered " + PlayerCount() + " " + IsEmpty());
					break;
				}
			}
		}
		public int PlayerCount() {
			int count = 0;
			for (int i = 0; i < Clients.Length; i++) {
				if (Clients [i] != null)
					count++;
			}

			return count;
		}
		public void ClientDisconnected(GameClient client) {

			for (int i = 0; i < Clients.Length; i++) {
				if (Clients [i] == null)
					continue;
				Clients [i].inGame = false;
				Clients [i].inLobby = true;
				if (Clients [i] == client) {
					Clients [i] = null;
					Console.WriteLine ("Lobby " + Id + " Player Disconnected");
					continue;
				} else {
					Clients [i].SendToLobby ();
				}
				IsGameStarted = false;
				Server.serverInstance.UpdateTitle ();

			}
			//Send to the player that the game is no longer active
		}

		public void SendFloorItems() {
			for(int i = 0; i < floorItems.Length; i++) {
				if (floorItems [i] != null) {
					SendFloorItem (floorItems [i]);
					//Console.WriteLine ("Sent Floor items");
				}
			}
		}
		void InitFloorItems() {
			if (InitingItems)
				return;
			InitingItems = true;
			floorItems = new Item[500];
			for (int i = 0; i < 3; i++) {
				floorItems [i] = new Pistol ();
				floorItems [i].x = 10f;
				floorItems [i].y = 0f;
				floorItems [i].z = 5f * (i + 1);
				floorItems [i].Id = i;
			}
			for (int i = 3; i < 6; i++) {
				floorItems [i] = new M4M1 ();
				floorItems [i].x = 15f;
				floorItems [i].y = 0f;
				floorItems [i].z = 5f * (i + 1);
				floorItems [i].Id = i;
			}
			InitingItems = false;
		}

		public void AddFloorItem(Item item) {
			for (int i = 0; i < floorItems.Length; i++) {
				if (floorItems [i] == null) {
					item.Id = i;
					floorItems [i] = item;
					SendFloorItem (floorItems [i]);
					break;
				}
			}
		}
		void SendFloorItem(Item item) {
			for (int i = 0; i < Clients.Length; i++) {
				NetworkPacket packet = new NetworkPacket (32);
				packet.WriteInt (packet.GetLength ());
				packet.WriteInt ((int)Messages.ServerPackets.FLOOR_ITEM);
				packet.WriteInt (item.Id);
				packet.WriteInt ((int)item.ItemType);
				if (floorItems [i] is Weapons) {
					packet.WriteInt ((int)((Weapons)item).WeaponType);
				}
				packet.WriteFloat (item.x);
				packet.WriteFloat (item.y);
				packet.WriteFloat (item.z);
				Clients [i].Write (packet.ToArray ());
			}
		}

		public Item GetItem(int id) {
			for(int i = 0; i < floorItems.Length; i++) {
				if (floorItems [i] != null) {
					if (floorItems [i].Id == id)
						return floorItems [i];
				}
			}
			return null;
		}

		public async void SendAIs() {
			for (int i = 0; i < Zombies.Length; i++) {
				await Task.Delay (50);
				SendAI (Zombies [i]);
				Zombies [i].isAlive = true;
				//Zombies [i].SearchForPlayer ();
			}
		}

		public void SendAI(Zombie zombie) {
            try { 
			    for (int i = 0; i < Clients.Length; i++) {
				    NetworkPacket packet = new NetworkPacket (40);
				    packet.WriteInt (packet.GetLength ());
				    packet.WriteInt ((int)Messages.ServerPackets.SPAWN_AI);
				    packet.WriteInt (zombie.ZombieID);
				    packet.WriteFloat (zombie.getX);
				    packet.WriteFloat (zombie.getY);
				    packet.WriteFloat (zombie.getZ);
				    packet.WriteFloat (zombie.rx);
				    packet.WriteFloat (zombie.ry);
				    packet.WriteFloat (zombie.rz);
				    packet.WriteFloat (zombie.rw);
				    Clients [i].Write (packet.ToArray ());
			    }
            }
            catch (Exception)
            {

            }

        }

		public void UpdateAI(int zombieId, float x, float y, float z, float rx, float ry, float rz, float rw) {
			Zombie zombie = Zombies [zombieId];
			zombie.SetLocation (x, y, z);
			zombie.SetRotation (rx, ry, rz, rw);
		}

		void InitZombies() {
			for (int i = 0; i < Zombies.Length; i++) {
				Zombies [i] = new Zombie ("Zombie " + i);
				Zombies [i].ZombieID = i;
				Zombies [i].SetLocation ((float)random.Next(0,100), 0, (float)random.Next(0,200));
				Zombies [i].SetRotation (0, 0, 0, 0);
				Zombies [i].isAlive = true;
				Zombies [i].LobbyID = Id;
			}
		}

		public void SendAIToMove(Zombie zombie, Vector3 location) {
			NetworkPacket packet2 = new NetworkPacket (24);
			packet2.WriteInt (packet2.GetLength ());
			packet2.WriteInt ((int)Messages.ServerPackets.AI_LOCATION);
			packet2.WriteInt (zombie.ZombieID);
			packet2.WriteFloat (location.x);
			packet2.WriteFloat (location.y);
			packet2.WriteFloat (location.z);
			for (int i = 0; i < Clients.Length; i++) {
				try {
				 Clients [i].Write (packet2.ToArray ());
				} catch (Exception) {
					//when client exit while sending data, ignore
				}
			}
		}
			
		public void SendAIAttack(Zombie zombie, bool isAttacking = true) {
			NetworkPacket packet = new NetworkPacket (12);
			packet.WriteInt (packet.GetLength ());
			if (isAttacking)
				packet.WriteInt ((int)Messages.ServerPackets.AI_ATTACK);
			else
				packet.WriteInt ((int)Messages.ServerPackets.AI_STOPPED);
			packet.WriteInt (zombie.ZombieID);
			foreach (GameClient client in Clients) {
				client.Write (packet.ToArray ());
			}
		}
	}



}

