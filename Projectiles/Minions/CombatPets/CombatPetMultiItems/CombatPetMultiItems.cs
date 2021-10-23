using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetMultiItems
{
	class MasterModeMechanicalBossPetsBuff : CombatPetBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.MinecartLeftMech;
		internal override int MinionSlotsUsed => 3;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// the occasional case where it doesn't work on load
			projectileTypes = new int[] {
				ProjectileType<RezMinion>(),
				ProjectileType<SpazMinion>(),
				ProjectileType<MiniPrimeMinion>(),
				ProjectileType<DestroyerLiteMinion>()
			};
			DisplayName.SetDefault("Mini Mechs");
			Description.SetDefault("The mini mechanical bosses will fight for you");
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<LeveledCombatPetModPlayer>().UsingMultiPets = true;
			base.Update(player, ref buffIndex);
		}
	}

	class MasterModeMechanicalBossPetsMinionItem : MinionItem<MasterModeMechanicalBossPetsBuff, RezMinion>
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.MinecartMech;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mechanical Homonculus");
			Tooltip.SetDefault("Creates Mini Mecha Mayhem!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.DefaultToVanitypet(Item.shoot, Item.buffType);
			Item.rare = ItemRarityID.Master;
			Item.value = Item.sellPrice(gold: 15);
		}
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemType<RezAndSpazMinionItem>(), 1)
			.AddIngredient(ItemType<MiniPrimeMinionItem>(), 1)
			.AddIngredient(ItemType<DestroyerLiteMinionItem>(), 1)
			.AddTile(TileID.DemonAltar).Register();
	}

	class MasterModeSlimePetsBuff : CombatPetBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.WellFed2;
		internal override int MinionSlotsUsed => 2;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// the occasional case where it doesn't work on load
			projectileTypes = new int[] {
				ProjectileType<SlimePrinceMinion>(),
				ProjectileType<SlimePrincessMinion>(),
			};
			DisplayName.SetDefault("Delightful Royal Delicacy");
			Description.SetDefault("The slime prince and princess will fight for you!");
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<LeveledCombatPetModPlayer>().UsingMultiPets = true;
			base.Update(player, ref buffIndex);
		}
	}

	class MasterModeSlimePetsMinionItem : MinionItem<MasterModeSlimePetsBuff, SlimePrinceMinion>
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.QueenSlimeCrystal;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Delightful Royal Delicacy");
			Tooltip.SetDefault("The whole royal family is here!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.DefaultToVanitypet(Item.shoot, Item.buffType);
			Item.rare = ItemRarityID.Master;
			Item.value = Item.sellPrice(gold: 10);
		}
		public override void AddRecipes() => CreateRecipe(1)
			.AddIngredient(ItemType<SlimePrinceMinionItem>(), 1)
			.AddIngredient(ItemType<SlimePrincessMinionItem>(), 1)
			.AddTile(TileID.DemonAltar).Register();
	}
}
