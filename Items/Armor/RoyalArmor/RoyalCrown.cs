using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Armor.RoyalArmor
{
	[AutoloadEquip(EquipType.Head)]
	class RoyalCrown : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Royal Crown");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 8%\n" +
				"Increaes squire attack speed by 10%\n" +
				"\"You dropped this, king.\"");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(silver: 3);
			item.rare = ItemRarityID.Green;
			item.defense = 3;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == item.type && body.type == ItemType<RoyalGown>();
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier *= 0.9f;
			player.minionDamage += 0.08f;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "A floating crown will assist your squire in combat!";
			player.GetModPlayer<SquireModPlayer>().royalArmorSetEquipped = true;
		}
		public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
		{
			drawAltHair = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Gel, 25);
			recipe.AddRecipeGroup("AmuletOfManyMinions:Golds", 10);
			recipe.AddIngredient(ItemID.Ruby, 4);
			recipe.AddTile(TileID.Solidifier);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class RoyalCrownProjectile : SquireBoomerangMinion
	{

		protected override int idleVelocity => 12;

		protected override int targetedVelocity => 8;

		protected override int inertia => 4;

		protected override int attackRange => 196;

		protected override int attackCooldown => 60;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 20;
			projectile.height = 14;
			frameSpeed = 3;
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 crownOffset = new Vector2(0, -10);
			crownOffset.Y += 2 * (float)Math.Sin(2 * Math.PI * animationFrame / 60f);
			return base.IdleBehavior() + crownOffset;
		}

		protected override bool IsEquipped(SquireModPlayer player)
		{
			return player.royalArmorSetEquipped;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (squire is null)
			{
				return;
			}
			if (vectorToTarget is null || returning)
			{
				projectile.rotation = (float)(Math.PI / 12 * Math.Cos(2 * Math.PI * animationFrame / 60f));
				if (squire.spriteDirection == 1)
				{
					projectile.frame = 0;
				}
				else
				{
					projectile.frame = 4;
				}
			}
			else
			{
				minFrame = 0;
				maxFrame = 8;
				base.Animate(minFrame, maxFrame);
			}
		}
	}
}
