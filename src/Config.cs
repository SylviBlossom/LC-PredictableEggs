using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;

namespace PredictableEggs;

public class Config : SyncedConfig2<Config>
{
	[SyncedEntryField] public SyncedEntry<float> EggExplodeChance;
	[SyncedEntryField] public SyncedEntry<bool> WarningSound; 
	[SyncedEntryField] public SyncedEntry<bool> DontExplodeOnDrop;

	[SyncedEntryField] public SyncedEntry<bool> FixHitGround;

	public Config(ConfigFile cfg) : base(PluginInfo.PLUGIN_GUID)
	{
		EggExplodeChance = cfg.BindSyncedEntry("General", "EggExplodeChance", 16f, "Percentage chance for the egg to explode, determined when the egg is selected in the inventory. (Vanilla value is 16%)");
		WarningSound = cfg.BindSyncedEntry("General", "WarningSound", true, "Plays a unique sound (fizzling) if the egg will explode on its next throw.");
		DontExplodeOnDrop = cfg.BindSyncedEntry("General", "DontExplodeOnDrop", true, "Prevents eggs from exploding when dropped, as opposed to being thrown.");

		FixHitGround = cfg.BindSyncedEntry("Technical", "FixHitGround", true, "Fixes a bug where items hitting the ground might not register properly, making no sound and not exploding eggs. Keep enabled unless any issues occur.");

		ConfigManager.Register(this);
	}
}
