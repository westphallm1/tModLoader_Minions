using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class HornetMinionBuff : MinionBuff
	{
		public HornetMinionBuff() : base(ProjectileType<HornetMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Hornet");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class HornetMinionItem : MinionItem<HornetMinionBuff, HornetMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.HornetStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Hornet Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.HornetStaff);
			base.SetDefaults();
			item.damage = 11;
		}
	}

	public class HornetStinger : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.HornetStinger;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.HornetStinger);
			base.SetDefaults();
		}

		public override void PostAI()
		{
			if (Main.rand.Next(2) == 0)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 18, 0f, 0f, 0, default, 0.9f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.5f;
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Poisoned, 300);
		}
	}

	public class HornetMinion : HeadCirclingGroupAwareMinion
	{
		int lastShootFrame = 0;
		// used to gently bob back and forth between 2 set points from the enemy
		int distanceCyle = 1;
		protected override int BuffId => BuffType<HornetMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.Hornet;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Hornet");
			Main.projFrames[projectile.type] = 3;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			drawOffsetX = (projectile.width - 44) / 2;
			attackFrames = 60;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type])
				{
					projectile.frame = 0;
				}
			}
			if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			}
			else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
			projectile.rotation = projectile.velocity.X * 0.05f;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int travelSpeed = 9;
			int projectileVelocity = 12;
			int inertia = 10;
			Vector2 lineOfFire = vectorToTargetPosition;
			Vector2 oppositeVector = -vectorToTargetPosition;
			oppositeVector.SafeNormalize();
			float targetDistanceFromFoe = distanceCyle == 1 ? 128 : 180;
			if (targetNPCIndex is int targetIdx && Main.npc[targetIdx].active)
			{
				// use the average of the width and height to get an approximate "radius" for the enemy
				NPC npc = Main.npc[targetIdx];
				Rectangle hitbox = npc.Hitbox;
				targetDistanceFromFoe += (hitbox.Width + hitbox.Height) / 4;
			}
			vectorToTargetPosition += targetDistanceFromFoe * oppositeVector;
			// slowly bob back and forth between two radii from the target
			if(vectorToTargetPosition.LengthSquared() < 16 * 16)
			{
				distanceCyle *= -1;
			}
			if(vectorToTargetPosition.LengthSquared() < 64 * 64)
			{
				travelSpeed = 3;
			} 
			if (player.whoAmI == Main.myPlayer && IsMyTurn() &&
				animationFrame - lastShootFrame >= attackFrames &&
				vectorToTargetPosition.LengthSquared() < 256 * 256)
			{
				lineOfFire.SafeNormalize();
				lineOfFire *= projectileVelocity;
				lastShootFrame = animationFrame;
				Projectile.NewProjectile(
					projectile.Center,
					lineOfFire,
					ProjectileType<HornetStinger>(),
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer);
				Main.PlaySound(SoundID.Item17, projectile.Center);
			}
			DistanceFromGroup(ref vectorToTargetPosition);
			if (vectorToTargetPosition.Length() > travelSpeed)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= travelSpeed;
			}
			projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}
	}
}
