using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
namespace GameServer
{
	public class Server
	{
		TcpListener gameSocket;
		public static List<GameClient> clients = new List<GameClient>();
		public static List<Zombie> GameZombies = new List<Zombie>();
		public static Lobby[] Lobbies = new Lobby[10];
		public static Server serverInstance = new Server();
		public static void Main (string[] args)
		{
			Thread t = new Thread (serverInstance.Connect);
			t.Start ();
			Console.WriteLine ("Server Started");
			while (true) {
				String Command = Console.ReadLine ();
				switch (Command) {
				case "clear":
					Console.Clear ();
					break;
				case "exit":
					Environment.Exit (0);
					break;
				}
			}
		}

		public void Connect() {
			GameZombies.Add (new Zombie ("Zombie 1"));
			gameSocket = new TcpListener (IPAddress.Any, 2550);
			gameSocket.Start ();
			gameSocket.BeginAcceptTcpClient (OnClientConnect, null);
			for (int i = 0; i < Lobbies.Length; i++) {
				Lobbies [i] = new Lobby ();
				Lobbies [i].Id = i;
			}
			UpdateTitle ();
		}

		void OnClientConnect(IAsyncResult rs) {
			TcpClient client = gameSocket.EndAcceptTcpClient (rs);
			foreach (Lobby lobby in Lobbies) {
				if (lobby.IsEmpty ()) {
					lobby.AddPlayer (client);
					break;
				}
			}
			CheckForReadyLobbies ();
			gameSocket.BeginAcceptTcpClient (OnClientConnect, null);

		}
		public void CheckForReadyLobbies() {
			foreach (Lobby lobby in Lobbies) {
				if (lobby.IsEmpty () == false) {
					lobby.StartGame ();
				}
			}
		}

		public void UpdateTitle() {
			int num = 0;
			for (int i = 0; i < Lobbies.Length; i++) {
				if (Lobbies [i].IsGameStarted)
					num++;
			}
			Console.Title = "Server Running [ " + num + " ] Lobbies";
		}
	}
}
