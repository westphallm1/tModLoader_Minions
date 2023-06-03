using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace AmuletOfManyMinions
{
	public class ClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static ClientConfig Instance => ModContent.GetInstance<ClientConfig>();

		//TODO localize & enum-ify OptionStrings
		public const string AnchorInventory = "Inventory";
		public const string AnchorHealth = "Health";
		public const string AnchorDefault = AnchorHealth;
		public static readonly string[] AnchorOptions = new string[] { AnchorInventory, AnchorHealth };

		public const string QuickDefendToggle = "Toggle";
		public const string QuickDefendHold = "Hold";
		public static readonly string[] QuickDefendOptions = new string[] { QuickDefendToggle, QuickDefendHold };

		// Miscellaneous config options
		[Header("GeneralConfiguration")]

		[DrawTicks]
		[OptionStrings(new string[] { AnchorInventory, AnchorHealth })]
		[DefaultValue(AnchorDefault)]
		public string TacticsUIAnchorPos;

		[DrawTicks]
		[OptionStrings(new string[] { QuickDefendToggle, QuickDefendHold})]
		[DefaultValue(QuickDefendToggle)]
		public string QuickDefendHotkeyStyle;

		[DefaultValue(true)]
		public bool ShowMinionVarietyBonus;

		[Header("TacticsConfiguration")]

		[DefaultValue(false)]
		public bool IgnoreVanillaTargetReticle;

		[DefaultValue(false)]
		public bool WhipRightClickTacticsRadial;

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
				QuickDefendHotkeyStyle = QuickDefendToggle;
			}
		}
	}

	public class ServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		public static ServerConfig Instance => ModContent.GetInstance<ServerConfig>();

		// tactics config options
		[Header("TacticsConfiguration")]

		[DefaultValue(true)]
		public bool WhipsSetWaypoint;

		[DefaultValue(false)]
		public bool SquiresSetWaypoint;

		[DefaultValue(false)]
		public bool SquireProjSetWaypoint;

		// balance config options
		[Header("BalanceConfiguration")]

		[Range(20, 300)]
		[Increment(5)]
		[DefaultValue(100)]
		[Slider]
		public int GlobalDamageMultiplier;


		[Range(0, 80)]
		[DefaultValue(0)]
		[Slider]
		public int OtherDamageMinionNerf;

		[Range(0, 50)]
		[DefaultValue(0)]
		[Slider]
		public int MinionDamageSquireNerf;

		[Range(0, 15)]
		[DefaultValue(0)]
		[Slider]
		public int SquireDamageMinionNerf;

		[DefaultValue(false)]
		public bool MinionsInnacurate;

		[DefaultValue(false)]
		public bool SquireMinionSlot;

		[DefaultValue(true)]
		public bool SquireSpecialsWithWhips;

		[DefaultValue(true)]
		public bool SquiresDealTagDamage;

		[DefaultValue(true)]
		public bool CombatPetsMinionSlots;

		[DefaultValue(true)]
		public bool AllowMultipleCombatPets;

		[Header("CrossModCompatibility")]
		//[Label("Assorted Crazy Things: Enable Combat Pet AI")]
		//[Tooltip("If true, turns most pets from Assorted Crazy Things into combat pets.")]
		//[DefaultValue(true)]
		//[ReloadRequired]
		//public bool EnableACTCombatPets;

		[DefaultValue(false)]
		[ReloadRequired]
		public bool DisableSummonersShineAI;

		// courtesy of direwolf420
		public static bool IsPlayerLocalServerOwner(int whoAmI)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();
			}

			return NetMessage.DoesPlayerSlotCountAsAHost(whoAmI);
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			else if (!IsPlayerLocalServerOwner(whoAmI))
			{
				message = AoMMSystem.AcceptClientChangesText.ToString();
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}
	}
}
