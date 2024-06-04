using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PredictableEggs;

public static class Assets
{
	public static AudioClip EggReadyToExplode { get; private set; }

	public static void Load()
	{
		var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		var bundle = AssetBundle.LoadFromFile(Path.Combine(path, "predictableeggs.bundle"));

		EggReadyToExplode = bundle.LoadAsset<AudioClip>("EggReadyToExplode");
	}
}
