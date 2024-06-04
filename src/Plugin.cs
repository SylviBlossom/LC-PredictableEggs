using BepInEx;
using BepInEx.Logging;
using CessilCellsCeaChells.CeaChore;
using System.Reflection;
using UnityEngine;

[assembly: RequiresMethod(typeof(StunGrenadeItem), "Awake", typeof(void))]

namespace PredictableEggs;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.sigurd.csync", "5.0.1")]
public class Plugin : BaseUnityPlugin
{
	public static new Config Config { get; private set; }
	public static new ManualLogSource Logger { get; private set; }

	private void Awake()
	{
		Config = new(base.Config);
		Logger = base.Logger;

		Assets.Load();
		Patches.Initialize();

		InitNetcodePatcher();

		// Plugin startup logic
		Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
	}

	private void InitNetcodePatcher()
	{
		var types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (var type in types)
		{
			var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			foreach (var method in methods)
			{
				var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
				if (attributes.Length > 0)
				{
					method.Invoke(null, null);
				}
			}
		}
	}
}