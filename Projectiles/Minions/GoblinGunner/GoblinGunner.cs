using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.GoblinGunner
{
	public class GoblinGunnerMinionBuff : MinionBuff
	{
		public GoblinGunnerMinionBuff() : base(ProjectileType<GoblinGunnerCounterMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Gunner");
			Description.SetDefault("A goblin gunner will fight for you!");
		}
	}

	public class GoblinGunnerMinionItem : MinionItem<GoblinGunnerMinionBuff, GoblinGunnerCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Radio Beacon");
			Tooltip.SetDefault("Summons a goblin gunship to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 28;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}
	}

	public class GoblinGunnerMinionGuns : ModProjectile { }

	public class GoblinGunnerBullet : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.CountsAsHoming[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 2;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 60;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Lighting.AddLight(Projectile.position, Color.Violet.ToVector3() * 0.5f);
		}
	}
	public class GoblinGunnerCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<GoblinGunnerMinionBuff>();
		protected override int MinionType => ProjectileType<GoblinGunnerMinion>();
	}
	public class GoblinGunnerMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<GoblinGunnerMinionBuff>();
		public override int CounterType => ProjectileType<GoblinGunnerCounterMinion>();

		private int framesSinceLastHit;
		protected override int dustType => DustID.Shadowflame;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Gunner");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 1;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.tileCollide = false;
			framesSinceLastHit = 0;
			Projectile.friendly = true;
			attackThroughWalls = true;
			useBeacon = false;
			frameSpeed = 5;
		}



		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[ProjectileType<GoblinGunnerMinionGuns>()].Value;
			Vector2 angle = vectorToTarget ?? new Vector2(-Projectile.spriteDirection, 0);
			int frame = Math.Min(4, (int)EmpowerCount - 1);
			Rectangle bounds = new Rectangle(0, 14 * frame, 14, 14);
			int distanceFromOrigin = framesSinceLastHit > 3 ? 34 : 32;
			Vector2 origin = new Vector2(distanceFromOrigin, bounds.Height / 2f);
			Vector2 pos = Projectile.Center;
			float r = angle.ToRotation() + (float)Math.PI;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, 0, 0);

			return true;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -32;
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
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
			Lighting.AddLight(Projectile.position, Color.White.ToVector3() * 0.5f);
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// stay floating behind the player at all times
			IdleMovement(vectorToIdle);
			framesSinceLastHit++;
			int rateOfFire = Math.Max(8, 35 - 3 * (int)EmpowerCount);
			int projectileVelocity = 40;
			if (framesSinceLastHit++ > rateOfFire && targetNPCIndex is int npcIdx)
			{
				NPC target = Main.npc[npcIdx];
				// try to predict the position at the time of impact a bit
				vectorToTargetPosition += (vectorToTargetPosition.Length() / projectileVelocity) * target.velocity;
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= projectileVelocity;
				Vector2 pos = Projectile.Center;
				framesSinceLastHit = 0;
				Projectile.spriteDirection = vectorToTargetPosition.X > 0 ? -1 : 1;
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetProjectileSource_FromThis(),
						pos,
						VaryLaunchVelocity(vectorToTargetPosition),
						ProjectileType<GoblinGunnerBullet>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer);
				}
				SoundEngine.PlaySound(new LegacySoundStyle(2, 11), pos);
			}
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 12) * (int)EmpowerCount; // only scale up damage a little bit
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance) is Vector2 target2)
			{
				return target2 - Projectile.Center;
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
			return 800 + 20 * EmpowerCount;
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
			if (Math.Abs(Projectile.velocity.X) > 2 && vectorToTarget is null)
			{
				Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}
}
