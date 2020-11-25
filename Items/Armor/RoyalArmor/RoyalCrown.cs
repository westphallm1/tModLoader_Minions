using AmuletOfManyMinions.Items.Accessories;
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
			player.minionDamageMult += 0.08f;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Increases your max minions by 1\n"
				+ "Your minions will release damaging fungi while attacking";
			player.maxMinions++;
			player.GetModPlayer<SquireModPlayer>().royalArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}
	}

	public class RoyalCrownProjectile : SquireAccessoryMinion
	{
		private bool returning = false;
		private int? returnedToHeadFrame = -10;
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
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 30;
			attackThroughWalls = true;
		}
		public override Vector2 IdleBehavior()
		{
			if(squire != null)
			{
				projectile.damage = 5 * squire.damage / 6;
			}
			Vector2 crownOffset = new Vector2(0, -14);
			crownOffset.Y += 2 * (float)Math.Sin(2 * Math.PI * animationFrame / 60f);
			return base.IdleBehavior() + crownOffset;
		}

		protected override bool IsEquipped(SquireModPlayer player)
		{
			return player.royalArmorSetEquipped;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (vectorToIdlePosition.Length() > 32)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= 12;
				int inertia = 4;
				projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			} else
			{
				returnedToHeadFrame = returnedToHeadFrame ?? animationFrame;
				projectile.position += vectorToIdlePosition - new Vector2(projectile.width, projectile.height) / 2;
				projectile.velocity = Vector2.Zero;
				returning = false;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			vectorToTargetPosition.Normalize();
			vectorToTargetPosition *= 8;
			int inertia = 4;
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}

		public override Vector2? FindTarget()
		{
			if(SquireAttacking() &&
				returnedToHeadFrame is int frame &&
				animationFrame - frame > 60 && 
				!returning &&
				ClosestEnemyInRange(196, maxRangeFromPlayer: false) is Vector2 target)
			{
				projectile.friendly = true;
				projectile.tileCollide = true;
				return target - projectile.Center;
			}
			projectile.friendly = false;
			projectile.tileCollide = false;
			return null;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			returnedToHeadFrame = null;
			returning = true;
		}

		public override void OnHitTarget(NPC target)
		{
			if(player.whoAmI != Main.myPlayer)
			{
				returnedToHeadFrame = null;
				returning = true;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(vectorToTarget is null || returning)
			{
				projectile.rotation = (float)(Math.PI / 12 * Math.Cos(2 * Math.PI * animationFrame / 60f));
				if(squire.spriteDirection == 1)
				{
					projectile.frame = 0;
				} else
				{
					projectile.frame = 4;
				}
			} else
			{
				minFrame = 0;
				maxFrame = 8;
				base.Animate(minFrame, maxFrame);
			}
		}
	}
}
