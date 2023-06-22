﻿using AmuletOfManyMinions.Items.Accessories.SquireSkull;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.TechnoCharm
{
	class TechnoCharmAccessory : ModItem
	{
		public static readonly int SquireDamageIncrease = 12;
		public static readonly int SquireAttackSpeedIncrease = 10;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(SquireDamageIncrease, SquireAttackSpeedIncrease);
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightPurple;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.AvengerEmblem, 1).AddIngredient(ItemType<SquireSkullAccessory>(), 1).AddTile(TileID.TinkerersWorkbench).Register();
		}
		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().squireTechnoSkullAccessory = true;
			player.GetModPlayer<SquireModPlayer>().SquireAttackSpeedMultiplier *= (1f - SquireAttackSpeedIncrease/100f);
			player.GetModPlayer<SquireModPlayer>().squireDamageMultiplierBonus += SquireDamageIncrease / 100f;
		}

		public override bool CanEquipAccessory(Player player, int slot, bool modded)
		{
			// don't allow side by side with squire skull, so their debuffs don't overwrite each other
			int skullType = ItemType<SquireSkullAccessory>();
			return !modded && slot > 9 || !player.armor.Skip(3).Take(5 + player.GetAmountOfExtraAccessorySlotsToShow()).Any(a => !a.IsAir && a.type == skullType);
		}
	}

	class TechnoCharmProjectile : SquireAccessoryMinion
	{

		int DebuffCycleFrames = 360;
		int AnimationFrames = 120;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 20;
			Projectile.height = 16;
		}

		private int debuffCycle => (animationFrame % DebuffCycleFrames) / (DebuffCycleFrames / 3);


		public override Vector2 IdleBehavior()
		{
			Vector2 idleVector = base.IdleBehavior();
			if (debuffCycle == 0)
			{
				squirePlayer.squireDebuffOnHit = BuffID.Frostburn;
				squirePlayer.squireDebuffTime = 180;
				Lighting.AddLight(Projectile.position, Color.Cyan.ToVector3() * 0.33f);
			}
			else if (debuffCycle == 1)
			{
				squirePlayer.squireDebuffOnHit = BuffID.Ichor;
				squirePlayer.squireDebuffTime = 60;
				Lighting.AddLight(Projectile.position, Color.Gold.ToVector3() * 0.33f);
			}
			else
			{
				squirePlayer.squireDebuffOnHit = BuffID.CursedInferno;
				squirePlayer.squireDebuffTime = 180;
				Lighting.AddLight(Projectile.position, Color.LimeGreen.ToVector3() * 0.33f);
			}
			int angleFrame = animationFrame % AnimationFrames;
			float angle = 2 * (float)(Math.PI * angleFrame) / AnimationFrames;
			Vector2 angleVector = 32 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			return idleVector + angleVector;
		}

		protected override bool IsEquipped(SquireModPlayer player)
		{
			return player.squireTechnoSkullAccessory;
		}
	}
}
