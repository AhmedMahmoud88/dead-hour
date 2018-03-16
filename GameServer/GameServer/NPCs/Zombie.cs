using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace GameServer
{
	public class Zombie : Player
	{
		public bool isAlive = true;
		public int ZombieID;
		public int LobbyID;
		float detectDistance = 20f;
		float attackRange = 1.3f;
		public bool isChasing = false;
		public bool isAttacking = false;
		public GameClient ChasingClient;

		public Zombie(String Name) : base(Name) {
			x = 40.02f;
			y = 0.5f;
			z = 15f;
			rx = 0;
			ry = 0;
			rz = 0;
		}


		public async void SearchForPlayer() {
			while (isAlive) {
				await Task.Delay (200);
				float dist = Mathf.Infinity;
				GameClient closest = null;
				if (isAttacking) {
					if (ChasingClient != null) {
						float d = Vector3.Distance(new Vector3(x,y,z) , new Vector3(ChasingClient.player.getX, ChasingClient.player.getY, ChasingClient.player.getZ));
						if(d > attackRange) {
							isAttacking = false;
							ChasingClient = null;
							Server.Lobbies[LobbyID].SendAIAttack(this, false);
						} else {
							continue;
						}
					} else {
						isAttacking = false;
						Server.Lobbies[LobbyID].SendAIAttack(this, false);
					}
				}
				for (int i = 0; i < Server.Lobbies [LobbyID].Clients.Length; i++) {
					if (Server.Lobbies [LobbyID].Clients [i] != null) {
						GameClient current = Server.Lobbies [LobbyID].Clients [i];
						float d = Vector3.Distance (new Vector3 (x, y, z), new Vector3 (current.player.getX, current.player.getY, current.player.getZ));
						if (d <= detectDistance) {
							if (d < dist) {
								dist = d;
								closest = current;
							}
							if (d <= attackRange) {
								SendStopChasing ();
								closest = null;
								Server.Lobbies [LobbyID].SendAIAttack (this, true);
								isAttacking = true;
								ChasingClient = current;
								continue;
							}
							//Console.WriteLine (d);

						}
					}
				}
				if (closest != null) {
					Vector3 myLocation = new Vector3 (x, 0, z);
					Vector3 playerLocation = new Vector3 (closest.player.getX, 0, closest.player.getZ);
					Vector3 moveTO = playerLocation - myLocation;
					Server.Lobbies [LobbyID].SendAIToMove (this, moveTO);
				} else {
					//Make the zombie takes a decicion how he should move
				}
			}
		}

		public void SendStopChasing() {
			for (int i = 0; i < Server.Lobbies [LobbyID].Clients.Length; i++) {
				try {
					NetworkPacket packet = new NetworkPacket (12);
					packet.WriteInt (packet.GetLength ());
					packet.WriteInt ((int)Messages.ServerPackets.AI_STOPPED);
					//Console.WriteLine("Stopped");
					packet.WriteInt (ZombieID);
					if (Server.Lobbies [LobbyID].Clients == null)
						break;
					Server.Lobbies [LobbyID].Clients [i].Write (packet.ToArray ());
				} catch (Exception) {
						//just so the server won't crash when player disconnected while trying to send him a packet;
				}
			}
			isChasing = false;
			ChasingClient = null;
		}
	}
}

