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

			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 3);
			Item.rare = ItemRarityID.Green;
			Item.defense = 3;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == Item.type && body.type == ItemType<RoyalGown>();
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().SquireAttackSpeedMultiplier *= 0.9f;
			player.GetDamage<SummonDamageClass>() += 0.08f;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "A floating crown will assist your squire in combat!";
			player.GetModPlayer<SquireModPlayer>().royalArmorSetEquipped = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Gel, 25).AddRecipeGroup("AmuletOfManyMinions:Golds", 10).AddIngredient(ItemID.Ruby, 4).AddTile(TileID.Solidifier).Register();
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
			Main.projFrames[Projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 20;
			Projectile.height = 14;
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
				Projectile.rotation = (float)(Math.PI / 12 * Math.Cos(2 * Math.PI * animationFrame / 60f));
				if (squire.spriteDirection == 1)
				{
					Projectile.frame = 0;
				}
				else
				{
					Projectile.frame = 4;
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
