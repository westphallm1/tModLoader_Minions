using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

		//Old data and names for reference
		[JsonExtensionData]
		private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

		public const string AnchorInventory = "Inventory";
		public const string AnchorHealth = "Health";
		public const string AnchorDefault = AnchorHealth;
		public static readonly string[] AnchorOptions = new string[] { AnchorInventory, AnchorHealth };

		public const string QuickDefendToggle = "Toggle";
		public const string QuickDefendHold = "Hold";
		public static readonly string[] QuickDefendOptions = new string[] { QuickDefendToggle, QuickDefendHold };

		public enum TacticsUIAnchorType : byte
		{
			Health = 0,
			Inventory = 1,
		}

		// Miscellaneous config options
		[Header("GeneralConfiguration")]

		[DrawTicks]
		[DefaultValue(TacticsUIAnchorType.Health)]
		public TacticsUIAnchorType TacticsUIAnchor;

		public enum QuickDefendHotkeyStyleType : byte
		{
			Toggle = 0,
			Hold = 1,
		}

		[DrawTicks]
		[DefaultValue(QuickDefendHotkeyStyleType.Toggle)]
		public QuickDefendHotkeyStyleType QuickDefendHotkeyStyleNew;

		[DefaultValue(true)]
		public bool ShowMinionVarietyBonus;

		[Header("TacticsConfiguration")]

		[DefaultValue(false)]
		public bool IgnoreVanillaTargetReticle;

		[DefaultValue(false)]
		public bool WhipRightClickTacticsRadial;

		[JsonIgnore] //Hides it in UI and file
		public bool AnchorToInventory => TacticsUIAnchor == TacticsUIAnchorType.Inventory;

		[JsonIgnore]
		public bool AnchorToHealth => TacticsUIAnchor == TacticsUIAnchorType.Health;

		[JsonIgnore] //Hides it in UI and file
		public bool QuickDefendHotkeyToggle => QuickDefendHotkeyStyleNew == QuickDefendHotkeyStyleType.Toggle;

		[JsonIgnore]
		public bool QuickDefendHotkeyHold => QuickDefendHotkeyStyleNew == QuickDefendHotkeyStyleType.Hold;

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			PortOldMembers();

			//Correct invalid values to default fallback
			EnumFallback(ref TacticsUIAnchor, TacticsUIAnchorType.Health);
			EnumFallback(ref QuickDefendHotkeyStyleNew, QuickDefendHotkeyStyleType.Toggle);
		}

		private void PortOldMembers()
		{
			//port "TacticsUIAnchorPos": "Inventory"
			//port "QuickDefendHotkeyStyle": "Hold"
			//from string to enum, which requires (!) a member rename aswell
			JToken token;
			if (_additionalData.TryGetValue("TacticsUIAnchorPos", out token))
			{
				var tacticsUIAnchorPos = token.ToObject<string>();
				if (tacticsUIAnchorPos == AnchorInventory)
				{
					TacticsUIAnchor = TacticsUIAnchorType.Inventory;
				}
				else
				{
					TacticsUIAnchor = TacticsUIAnchorType.Health;
				}
			}
			if (_additionalData.TryGetValue("QuickDefendHotkeyStyle", out token))
			{
				var quickDefendHotkeyStyle = token.ToObject<string>();
				if (quickDefendHotkeyStyle == QuickDefendHold)
				{
					QuickDefendHotkeyStyleNew = QuickDefendHotkeyStyleType.Hold;
				}
				else
				{
					QuickDefendHotkeyStyleNew = QuickDefendHotkeyStyleType.Toggle;
				}
			}
			_additionalData.Clear(); //Clear this or it'll crash.
		}

		private static void EnumFallback<T>(ref T value, T defaultValue) where T : Enum
		{
			if (!Enum.IsDefined(typeof(T), value))
			{
				value = defaultValue;
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
