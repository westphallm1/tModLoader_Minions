using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace AmuletOfManyMinions
{
	[Label("Client Config")]
	public class ClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static ClientConfig Instance => ModContent.GetInstance<ClientConfig>();

		public const string AnchorInventory = "Inventory";
		public const string AnchorHealth = "Health";
		public const string AnchorDefault = AnchorHealth;
		public static readonly string[] AnchorOptions = new string[] { AnchorInventory, AnchorHealth };

		[Label("Minion Tactic UI Anchor Position")]
		[Tooltip("Choose between anchoring the UI with the right side of the inventory, or the left side of the health/minimap")]
		[DrawTicks]
		[OptionStrings(new string[] { AnchorInventory, AnchorHealth })]
		[DefaultValue(AnchorDefault)]
		public string TacticsUIAnchorPos;

		[JsonIgnore] //Hides it in UI and file
		public bool AnchorToInventory => TacticsUIAnchorPos == AnchorInventory;

		[JsonIgnore]
		public bool AnchorToHealth => TacticsUIAnchorPos == AnchorHealth;

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			// Correct invalid names
			if (Array.IndexOf(AnchorOptions, TacticsUIAnchorPos) <= -1)
			{
				TacticsUIAnchorPos = AnchorDefault;
			}
		}
	}
}
