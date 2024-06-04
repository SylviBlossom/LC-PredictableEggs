using System;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace PredictableEggs;

public class PredictableEggComponent : NetworkBehaviour
{
	public bool WasThrown = false;

	private StunGrenadeItem eggItem;

	private void Awake()
	{
		eggItem = gameObject.GetComponent<StunGrenadeItem>();
	}

	public void SetExplosiveOnEquip()
	{
		if (!IsOwner)
		{
			return;
		}

		var explodeRandom = new Random(StartOfRound.Instance.randomMapSeed + 10 + (int)transform.position.x + (int)transform.position.z);
		var shouldExplode = (explodeRandom.NextDouble() * 100f) < Plugin.Config.EggExplodeChance.Value;

		if (shouldExplode && Plugin.Config.WarningSound.Value)
		{
			gameObject.GetComponent<AudioSource>().PlayOneShot(Assets.EggReadyToExplode, 1f);
		}

		Plugin.Logger.LogInfo($"Egg explosion prediction: {shouldExplode}");

		eggItem.explodeOnThrow = shouldExplode;
		SetExplodeOnThrowDirectServerRpc(shouldExplode);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetExplodeOnThrowDirectServerRpc(bool explode)
	{
		SetExplodeOnThrowDirectClientRpc(explode);
	}

	[ClientRpc]
	public void SetExplodeOnThrowDirectClientRpc(bool explode)
	{
		if (IsOwner)
		{
			return;
		}

		Plugin.Logger.LogInfo($"Egg explosion prediction: {explode}");

		eggItem.gotExplodeOnThrowRPC = true;
		eggItem.explodeOnThrow = explode;
	}
}
