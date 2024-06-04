using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace PredictableEggs;

public static class Patches
{
	public static void Initialize()
	{
		On.StunGrenadeItem.EquipItem += StunGrenadeItem_EquipItem;
		On.StunGrenadeItem.DiscardItem += StunGrenadeItem_DiscardItem;
		On.StunGrenadeItem.ItemActivate += StunGrenadeItem_ItemActivate;
		On.StunGrenadeItem.OnHitGround += StunGrenadeItem_OnHitGround;
		IL.GrabbableObject.Update += GrabbableObject_Update_IL;

		new Hook(
			typeof(StunGrenadeItem).GetMethod("Awake", BindingFlags.Public | BindingFlags.Instance),
			typeof(Patches).GetMethod(nameof(StunGrenadeItem_Awake), BindingFlags.NonPublic | BindingFlags.Static)
		).Apply();
	}

	private static void GrabbableObject_Update_IL(ILContext il)
	{
		var cursor = new ILCursor(il);

		if (!cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchLdcR4(0.1f)))
		{
			Plugin.Logger.LogError("Failed IL hook for GrabbableObject.Update @ Check fall distance");
			return;
		}

		cursor.EmitDelegate<Func<float, float>>(value => Plugin.Config.FixHitGround.Value ? Math.Abs(value) : value);

		if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<GrabbableObject>("reachedFloorTarget")))
		{
			Plugin.Logger.LogError("Failed IL hook for GrabbableObject.Update @ Reached floor target");
			return;
		}

		cursor.Emit(OpCodes.Ldarg_0);
		cursor.EmitDelegate<Action<GrabbableObject>>(self =>
		{
			if (Plugin.Config.FixHitGround.Value && !self.hasHitGround)
			{
				self.PlayDropSFX();
				self.OnHitGround();
			}
		});
	}

	private static void StunGrenadeItem_OnHitGround(On.StunGrenadeItem.orig_OnHitGround orig, StunGrenadeItem self)
	{
		Plugin.Logger.LogInfo($"Hit ground! Will explode: {self.explodeOnThrow}");
		orig(self);
	}

	private static void StunGrenadeItem_ItemActivate(On.StunGrenadeItem.orig_ItemActivate orig, StunGrenadeItem self, bool used, bool buttonDown)
	{
		if (!self.inPullingPinAnimation && self.pinPulled && self.IsOwner)
		{
			var predictableEgg = self.gameObject.GetComponent<PredictableEggComponent>();
			predictableEgg.WasThrown = true;
		}

		orig(self, used, buttonDown);
	}

	private static void StunGrenadeItem_EquipItem(On.StunGrenadeItem.orig_EquipItem orig, StunGrenadeItem self)
	{
		if (self.chanceToExplode >= 100f)
		{
			// Skip if not easter egg
			orig(self);
			return;
		}

		var lastChanceToExplode = self.chanceToExplode;
		self.chanceToExplode = 100f; // Skip vanilla egg handling

		orig(self);

		self.chanceToExplode = lastChanceToExplode;

		var predictableEgg = self.gameObject.GetComponent<PredictableEggComponent>();
		predictableEgg.SetExplosiveOnEquip();

		predictableEgg.WasThrown = false;
		self.gotExplodeOnThrowRPC = true; // We literally send the RPC that sets this to "false" at the same time, which causes problems, so whatever
	}

	private static void StunGrenadeItem_DiscardItem(On.StunGrenadeItem.orig_DiscardItem orig, StunGrenadeItem self)
	{
		if (Plugin.Config.DontExplodeOnDrop && (self.IsOwner || self.wasOwnerLastFrame) && self.playerHeldBy != null && !self.playerHeldBy.isPlayerDead && self.chanceToExplode < 100f)
		{
			var predictableEgg = self.gameObject.GetComponent<PredictableEggComponent>();

			if (!predictableEgg.WasThrown)
			{
				Plugin.Logger.LogInfo("Dropping safely!");

				self.explodeOnThrow = false;
				predictableEgg.SetExplodeOnThrowDirectServerRpc(false);
			}
		}

		orig(self);
	}

	delegate void StunGrenadeItem_orig_Awake(StunGrenadeItem self);
	private static void StunGrenadeItem_Awake(StunGrenadeItem_orig_Awake orig, StunGrenadeItem self)
	{
		orig(self);

		if (!self.gameObject.GetComponent<PredictableEggComponent>())
		{
			self.gameObject.AddComponent<PredictableEggComponent>();
		}
	}
}
