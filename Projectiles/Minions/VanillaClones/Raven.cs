using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class RavenMinionBuff : MinionBuff
	{
		public RavenMinionBuff() : base(ProjectileType<RavenMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
		}

	}

	public class RavenMinionItem : MinionItem<RavenMinionBuff, RavenMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.RavenStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ItemName.SlimeStaff"));
			Tooltip.SetDefault("Summons a vampire slime to fight for you!\nIgnores 10 enemy defense");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.RavenStaff);
			base.SetDefaults();
		}
	}

	public class RavenMinion : HeadCirclingGroupAwareMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Raven;
		private int framesSinceLastHit;
		private int cooldownAfterHitFrames = 16;

		protected override int BuffId => BuffType<RavenMinionBuff>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Acorn");
			Main.projFrames[projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			attackFrames = 60;
			targetSearchDistance = 900;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			minFrame = vectorToTarget == null ? 0 : 4;
			maxFrame = vectorToTarget == null ? 4 : 8;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= (int)maxFrame)
				{
					projectile.frame = minFrame;
				}
			}
			if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			} else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float inertia = 18;
			float speed = 13;
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			framesSinceLastHit++;
			if (framesSinceLastHit < cooldownAfterHitFrames && framesSinceLastHit > cooldownAfterHitFrames / 2)
			{
				// start turning so we don't double directly back
				Vector2 turnVelocity = new Vector2(-projectile.velocity.Y, projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(projectile.velocity.X);
				projectile.velocity += turnVelocity;
			}
			else if (framesSinceLastHit++ > cooldownAfterHitFrames)
			{
				projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				projectile.velocity.SafeNormalize();
				projectile.velocity *= speed; // kick it away from enemies that it's just hit
			}
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}
	}
}
