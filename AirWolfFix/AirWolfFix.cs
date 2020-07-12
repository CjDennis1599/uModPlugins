using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries;
using UnityEngine;


namespace Oxide.Plugins
{
	[Info("AirWolfFix", "rostov114", "0.0.1")]
	public class AirWolfFix : RustPlugin
	{
		#region Configuration
		private Configuration _config;
		public class Configuration
		{
			[JsonProperty(PropertyName = "Fuel amount MiniCopter")]
			public int fuelAmountMiniCopter = 100;

			[JsonProperty(PropertyName = "Fuel amount ScrapTransportHelicopter")]
			public int fuelAmountScrapTransportHelicopter = 500;
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

			}
		}

		protected override void LoadDefaultConfig()
		{
			_config = new Configuration();
			SaveConfig();
		}

		protected override void SaveConfig() => Config.WriteObject(_config);
		#endregion

		private void OnEntitySpawned(BaseNetworkable networkable)
		{
			if (networkable == null || !(networkable is MiniCopter))
				return;

			NextTick(() =>
			{
				MiniCopter copter = networkable as MiniCopter;
				if (copter != null)
				{
					Item fuelItem = copter.GetFuelSystem().GetFuelItem();
					if (fuelItem != null)
					{
						int wolfAmount = Mathf.FloorToInt((float)fuelItem.info.stackable * 0.2f);
						if (wolfAmount == fuelItem.amount)
						{
							if (copter.PrefabName.Contains("vehicles/scrap heli carrier/scraptransporthelicopter"))
							{
								fuelItem.amount = _config.fuelAmountScrapTransportHelicopter;
							}

							if (copter.PrefabName.Contains("vehicles/minicopter/minicopter.entity"))
							{
								fuelItem.amount = _config.fuelAmountMiniCopter;
							}
						}
					}
				}
			});
		}
	}
}