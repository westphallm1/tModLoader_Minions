using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetEmblems
{
	public class CombatPetEmblemReverseLookup: ModSystem
	{
		public static Dictionary<int, int> LevelToTypeLookup;

		public override void Load()
		{
			LevelToTypeLookup = new();
		}

		public override void Unload()
		{
			LevelToTypeLookup = null;
		}
	}

	public abstract class CombatPetEmblem : ModItem
	{
		public abstract int PetLevel { get; }

		public static LocalizedText CommonTooltipText { get; private set; }
		public static LocalizedText MinionSlotsToCombatPetText { get; private set; }
		public override LocalizedText Tooltip => CommonTooltipText;

		public override void SetStaticDefaults()
		{
			string commonKey = "Common.CombatPetEmblems.";
			CommonTooltipText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{commonKey}CommonTooltip"));
			MinionSlotsToCombatPetText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{commonKey}MinionSlotsToCombatPet"));
			CombatPetEmblemReverseLookup.LevelToTypeLookup[PetLevel] = Type;
		}

		public override void SetDefaults()
		{
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.damage = CombatPetLevelTable.PetLevelTable[PetLevel].BaseDamage;
			Item.knockBack = 1f;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			int maxCombatPets = CombatPetLevelTable.PetLevelTable[PetLevel].MaxPets;
			if (ServerConfig.Instance.AllowMultipleCombatPets && maxCombatPets > 1)
			{
				tooltips.Add(new TooltipLine(Mod, nameof(MinionSlotsToCombatPetText),
					MinionSlotsToCombatPetText.Format(maxCombatPets))
				{
					OverrideColor = Color.LimeGreen
				});
			}
		}

		public override bool CanUseItem(Player player) => false;
	}

	public class GoldenCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 1;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.GoldBar, 12).AddTile(TileID.Anvils).Register();
	}

	public class PlatinumCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 1;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.PlatinumBar, 12).AddTile(TileID.Anvils).Register();
	}

	public class CorruptCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 2;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.DemoniteBar, 12).AddTile(TileID.Anvils).Register();
	}

	public class CrimsonCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 2;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.CrimtaneBar, 12).AddTile(TileID.Anvils).Register();
	}

	public class SkeletalCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 3;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() =>
			CreateRecipe(1).AddIngredient(ItemID.Bone, 50).AddTile(TileID.Anvils).Register();
	}

	public class SoulfulCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 4;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.SoulofLight, 8)
			.AddIngredient(ItemID.SoulofNight, 8)
			.AddTile(TileID.Anvils).Register();
	}

	public class HallowedCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 5;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.HallowedBar, 12)
			.AddIngredient(ItemID.SoulofSight, 1)
			.AddIngredient(ItemID.SoulofMight, 1)
			.AddIngredient(ItemID.SoulofFright, 1)
			.AddTile(TileID.MythrilAnvil).Register();
	}

	public class SpectreCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 6;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 11);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.SpectreBar, 12)
			.AddTile(TileID.MythrilAnvil).Register();
	}

	public class StardustCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 7;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Red;
		}
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.FragmentStardust, 12)
			.AddTile(TileID.LunarCraftingStation).Register();
	}

	public class CelestialCombatPetEmblem : CombatPetEmblem
	{
		public override int PetLevel => 8;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 16);
			Item.rare = ItemRarityID.Red;
		}
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemID.LunarBar, 12)
			.AddIngredient(ModContent.ItemType<StardustCombatPetEmblem>(), 1)
			.AddTile(TileID.LunarCraftingStation).Register();
	}
}
