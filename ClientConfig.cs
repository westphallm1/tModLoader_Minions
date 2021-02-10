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

		public const string QuickDefendToggle = "Toggle";
		public const string QuickDefendHold = "Hold";
		public static readonly string[] QuickDefendOptions = new string[] { QuickDefendToggle, QuickDefendHold };

		[Label("Minion Tactic UI Anchor Position")]
		[Tooltip("Choose between anchoring the UI with the right side of the inventory, or the left side of the health/minimap")]
		[DrawTicks]
		[OptionStrings(new string[] { AnchorInventory, AnchorHealth })]
		[DefaultValue(AnchorDefault)]
		public string TacticsUIAnchorPos;

		[Label("Minion Quick Defend Hotkey Style")]
		[Tooltip("Choose whether Minion Quick Defend is toggled on/off by the hotkey, or activated while the hotkey is held")]
		[DrawTicks]
		[OptionStrings(new string[] { QuickDefendToggle, QuickDefendHold})]
		[DefaultValue(QuickDefendToggle)]
		public string QuickDefendHotkeyStyle;

		[Label("Show Minion Variety Bonus")]
		[Tooltip("If true, displays the user's current minion variety bonus in a buff tooltip")]
		[DefaultValue(true)]
		public bool ShowMinionVarietyBonus;

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

			// Correct invalid names
			if (Array.IndexOf(QuickDefendOptions, QuickDefendHotkeyStyle) <= -1)
			{
				TacticsUIAnchorPos = QuickDefendToggle;
			}
		}
	}
}
