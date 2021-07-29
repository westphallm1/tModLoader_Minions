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

		[Label("Tactics Ignore Vanilla Minion Target Reticle")]
		[Tooltip("If true, minions will ignore the vanilla minion target reticle in favor of the npc selected by the current tactic")]
		[DefaultValue(false)]
		public bool IgnoreVanillaTargetReticle;

		[JsonIgnore] //Hides it in UI and file
		public bool AnchorToInventory => TacticsUIAnchorPos == AnchorInventory;

		[JsonIgnore]
		public bool AnchorToHealth => TacticsUIAnchorPos == AnchorHealth;


		[Range(-80, 100)]
		[Increment(10)]
		[DrawTicks]
		[DefaultValue(0)]
		[Label("Global damage adjustment")]
		[Tooltip("Modify the damage of every item in the mod by a percentage")]
		public int GlobalDamageMultiplier;

		[Range(0, 50)]
		[Increment(10)]
		[DrawTicks]
		[DefaultValue(0)]
		[Label("Minion/Squire damage nerf")]
		[Tooltip("If > 0, minion damage will be reduced by a percentage while a squire is active")]
		public int MinionDamageSquireNerf;

		[Range(0, 15)]
		[Increment(5)]
		[DrawTicks]
		[DefaultValue(0)]
		[Label("Squire/Minion damage nerf")]
		[Tooltip("If > 0, squire damage will be reduced by a percentage for each active minion")]
		public int SquireDamageMinionNerf;

		[DefaultValue(false)]
		[Label("Squires Occupy a minion slot")]
		[Tooltip("If true, squires will occupy a minion slot")]
		public bool SquireMinionSlot;

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
				QuickDefendHotkeyStyle = QuickDefendToggle;
			}
		}
	}
}
