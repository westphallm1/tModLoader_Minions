using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Items.Accessories
{
	class SquireSkullAccessory : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Enchants your squire with a cursed skull!\n" +
				"Adds a rotating debuff to squire damage");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().squireSkullAccessory = true;
		}
	}

	class SquireSkullProjectile : TransientMinion
	{

		int animationFrame;
		int DebuffCycleFrames = 360;
		int AnimationFrames = 120;
		public override void SetDefaults()
		{
			projectile.friendly = false;
			projectile.tileCollide = false;
			projectile.timeLeft = 2;
			animationFrame = 0;
		}

		private int debuffCycle => (animationFrame % DebuffCycleFrames) / (DebuffCycleFrames / 3);
		public override Vector2 IdleBehavior()
		{
			SquireModPlayer squirePlayer = player.GetModPlayer<SquireModPlayer>();
			Projectile squire = squirePlayer.GetSquire();
			if(squire == null)
			{
				return projectile.velocity;
			}
			projectile.timeLeft = 2;
			animationFrame++;
			int cycleFrame = debuffCycle;
			if(cycleFrame == 0)
			{
				squirePlayer.squireDebuffOnHit = -1;
			} else if (cycleFrame == 1)
			{
				squirePlayer.squireDebuffOnHit = BuffID.CursedInferno;
				squirePlayer.squireDebuffTime = 60;
			} else
			{
				squirePlayer.squireDebuffOnHit = BuffID.Ichor;
				squirePlayer.squireDebuffTime = 60;
			}

			int angleFrame = animationFrame % AnimationFrames;
			float angle = 2 * (float)(Math.PI * angleFrame) / AnimationFrames;
			Vector2 angleVector = 32 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			return (squire.Center + angleVector) - projectile.Center;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			projectile.position += vectorToIdlePosition;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(debuffCycle == 0)
			{

				minFrame = 0;
				maxFrame = 2;
			} else if (debuffCycle == 1)
			{
				minFrame = 1;
				maxFrame = 3;
			} else
			{
				minFrame = 2;
				maxFrame = 4;
			}
			base.Animate(minFrame, maxFrame);
		}

	}
}
