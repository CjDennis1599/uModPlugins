using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
	[Info("Radiation Edit", "rostov114", "0.0.1")]
	class RadiationEdit : RustPlugin
	{
		#region Configuration
		private Configuration _config;
		public class Configuration
		{
			[JsonProperty(PropertyName = "Radiation zones")]
			public List<Zone> zones = new List<Zone>();
			
			public class Zone
			{
				[JsonProperty(PropertyName = "Radiation sphere position")]
				public Vector3 position = default(Vector3);

				[JsonProperty(PropertyName = "Radiation Tier ( 0 - MINIMAL, 1 - LOW, 2 - MEDIUM, 3 - HIGH )")]
				public int tier = 1;

				[JsonProperty(PropertyName = "Radiation amount override")]
				public float amountOverride = 0;
			}
		}

		protected override void LoadConfig()
		{
			base.LoadConfig();
			try
			{
				_config = Config.ReadObject<Configuration>();
			}
			catch
			{
				PrintError("Error reading config, please check!");
			}
		}

		protected override void LoadDefaultConfig()
		{
			_config = new Configuration();
			SaveConfig();
		}

		protected override void SaveConfig() => Config.WriteObject(_config);
		#endregion

		#region Initialized
		private void OnServerInitialized()
		{
			if (_config == null || _config.zones == null)
				return;

			NextTick(() =>
			{
				foreach (TriggerRadiation radiation in UnityEngine.Object.FindObjectsOfType<TriggerRadiation>())
				{
					foreach (Configuration.Zone zone in _config.zones)
					{
						if (Vector3.Distance(zone.position, radiation.transform.position) <= 1)
						{
							radiation.radiationTier = (TriggerRadiation.RadiationTier) Mathf.Clamp(zone.tier, 0, 3);
							radiation.RadiationAmountOverride = Mathf.Clamp(zone.amountOverride, 0, 500) ;
						}
					}
				}
			});
		}
		#endregion
	}
}