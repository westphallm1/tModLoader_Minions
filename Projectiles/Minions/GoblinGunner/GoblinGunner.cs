using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.GoblinGunner
{
	public class GoblinGunnerMinionBuff : MinionBuff
	{
		public GoblinGunnerMinionBuff() : base(ProjectileType<GoblinGunnerMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Goblin Gunner");
			Description.SetDefault("A goblin gunner will fight for you!");
		}
	}

	public class GoblinGunnerMinionItem : EmpoweredMinionItem<GoblinGunnerMinionBuff, GoblinGunnerMinion>
	{
		protected override int dustType => DustID.Shadowflame;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Radio Beacon");
			Tooltip.SetDefault("Summons a goblin gunship to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.damage = 28;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}
	}

	public class GoblinGunnerMinionGuns : ModProjectile { }

	public class GoblinGunnerBullet : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 2;
			projectile.friendly = true;
			projectile.penetrate = 3;
			projectile.tileCollide = true;
			projectile.timeLeft = 60;
		}

		public override void AI()
		{
			projectile.rotation = projectile.velocity.ToRotation();
			Lighting.AddLight(projectile.position, Color.Violet.ToVector3() * 0.5f);
		}
	}
	public class GoblinGunnerMinion : EmpoweredMinion<GoblinGunnerMinionBuff>
	{

		private int framesSinceLastHit;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Gunner");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 44;
			projectile.height = 44;
			projectile.tileCollide = false;
			projectile.type = ProjectileType<GoblinGunnerMinion>();
			projectile.ai[0] = 0;
			projectile.ai[1] = 0;
			framesSinceLastHit = 0;
			projectile.friendly = true;
			attackThroughWalls = true;
			useBeacon = false;
			frameSpeed = 5;
		}



		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[ProjectileType<GoblinGunnerMinionGuns>()];
			Vector2 angle = vectorToTarget ?? new Vector2(-projectile.spriteDirection, 0);
			int frame = Math.Min(4, (int)projectile.minionSlots - 1);
			Rectangle bounds = new Rectangle(0, 14 * frame, 14, 14);
			int distanceFromOrigin = framesSinceLastHit > 3 ? 34 : 32;
			Vector2 origin = new Vector2(distanceFromOrigin, bounds.Height / 2f);
			Vector2 pos = projectile.Center;
			float r = angle.ToRotation() + (float)Math.PI;
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, 0, 0);

			return true;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			idlePosition.X += 48 * -player.direction;
			idlePosition.Y += -32;
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Top.X;
				idlePosition.Y = player.Top.Y - 16;
			}
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			Lighting.AddLight(projectile.position, Color.White.ToVector3() * 0.5f);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// stay floating behind the player at all times
			IdleMovement(vectorToIdle);
			framesSinceLastHit++;
			int rateOfFire = Math.Max(8, 35 - 3 * (int)projectile.minionSlots);
			int projectileVelocity = 40;
			if (framesSinceLastHit++ > rateOfFire && targetNPCIndex is int npcIdx)
			{
				NPC target = Main.npc[npcIdx];
				// try to predict the position at the time of impact a bit
				vectorToTargetPosition += (vectorToTargetPosition.Length() / projectileVelocity) * target.velocity;
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= projectileVelocity;
				Vector2 pos = projectile.Center;
				framesSinceLastHit = 0;
				projectile.spriteDirection = vectorToTargetPosition.X > 0 ? -1 : 1;
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						pos,
						vectorToTargetPosition,
						ProjectileType<GoblinGunnerBullet>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
				Main.PlaySound(SoundID.Item97, pos);
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(target, damage, knockback, crit);
			framesSinceLastHit = 0;
		}
		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 12) * (int)projectile.minionSlots; // only scale up damage a little bit
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (ClosestEnemyInRange(searchDistance, player.Center) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}
		public override Vector2? FindTarget()
		{
			Vector2? target = GetTargetVector();
			return target;
		}

		protected override float ComputeSearchDistance()
		{
			return 800 + 20 * projectile.minionSlots;
		}

		protected override float ComputeInertia()
		{
			return 5;
		}

		protected override float ComputeTargetedSpeed()
		{
			return 16;
		}

		protected override float ComputeIdleSpeed()
		{
			return 16;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 0;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (Math.Abs(projectile.velocity.X) > 2 && vectorToTarget is null)
			{
				projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
			}
			projectile.rotation = projectile.velocity.X * 0.05f;
		}
	}
}
