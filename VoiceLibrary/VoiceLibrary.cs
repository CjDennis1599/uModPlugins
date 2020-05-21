using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Network;

namespace Oxide.Plugins
{
	[Info("VoiceLibrary", "rostov114", "0.1.3")]
	public class VoiceLibrary : RustPlugin
	{
		#region Variables and Classes
		private Dictionary<string, CacheVoiceData> CacheVoice = new Dictionary<string, CacheVoiceData>();
		private class CacheVoiceData
		{
			public List<byte[]> data = new List<byte[]>();

			public CacheVoiceData(List<byte[]> data)
			{
				this.data = data;
			}

			public List<byte[]> GetData()
			{
				return this.data;
			}
		}

		private class VoiceFile
		{
			public List<byte[]> data = new List<byte[]>();
		}
		#endregion

		#region API
		[HookMethod("API_SendVoiceDistance")]
		public bool API_SendVoiceDistance(string name, uint netid, float distance = 100f)
		{
			BaseEntity entity = BaseNetworkable.serverEntities.Find(netid) as BaseEntity;
			if (entity != null)
			{
				List<byte[]> voiceData = this.LoadVoiceFile(name);
				if (voiceData != null)
				{
					List<Connection> connections = BaseNetworkable.GetConnectionsWithin(entity.transform.position, distance);
					foreach (var data in voiceData)
					{
						this.SendVoiceData(netid, data, connections);
					}

					return true;
				}
			}

			return false;
		}

		[HookMethod("API_SendVoicePlayer")]
		public bool API_SendVoicePlayer(string name, uint netid, BasePlayer player)
		{
			if (player != null && player.Connection != null)
			{
				List<byte[]> voiceData = this.LoadVoiceFile(name);
				if (voiceData != null)
				{
					List<Connection> connection = new List<Connection>() { player.Connection };
					foreach (var data in voiceData)
					{
						this.SendVoiceData(netid, data, connection);
					}

					return true;
				}
			}

			return false;
		}
		#endregion

		#region Helpers
		private List<byte[]> LoadVoiceFile(string name)
		{
			if (name == null)
			{
				PrintError($"File name is NULL!");
				return null;
			}
			
			if (!this.CacheVoice.ContainsKey(name))
			{
				if (!Interface.Oxide.DataFileSystem.ExistsDatafile($"{this.Title}/{name}"))
				{
					PrintError($"Not load voice file: {this.Title}/{name}");
					return null;
				}

				VoiceFile file =  Interface.Oxide.DataFileSystem.ReadObject<VoiceFile>($"{this.Title}/{name}") as VoiceFile;
				this.CacheVoice.Add(name, new CacheVoiceData(file.data));
			}

			return this.CacheVoice[name].GetData();
		}

		private void SendVoiceData(uint netid, byte[] data, List<Connection> connections)
		{
			if (Network.Net.sv.write.Start())
			{
				Network.Net.sv.write.PacketID(Network.Message.Type.VoiceData);
				Network.Net.sv.write.UInt32(netid);
				Network.Net.sv.write.BytesWithSize(data);
				Network.Net.sv.write.Send(new Network.SendInfo(connections) {priority = Network.Priority.Immediate});
			}
		}
		#endregion
	}
}