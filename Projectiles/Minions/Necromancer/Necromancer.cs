using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Minions.Necromancer
{
	public class NecromancerMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<NecromancerMinion>(), ProjectileType<NecromancerSkeletonMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Necromancer");
			// Description.SetDefault("A necromancer and his skeletal servants will fight for you!");
		}
	}

	public class NecromancerMinionItem : MinionItem<NecromancerMinionBuff, NecromancerSkeletonMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Necro Doll");
			// Tooltip.SetDefault("Summons a neromancer to fight for you!");
		}
		
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.RANGED);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 60;
			Item.value = Item.buyPrice(0, 15, 0, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.useStyle = ItemUseStyleID.HoldUp;
		}
	}

	public class BoneSphereBoneProjectile : ModProjectile
	{

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bone;
		public override void SetStaticDefaults()
		{
			// this is a bit sneaky, doesn't set any of the SimpleMinion defaults
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bone);
			Projectile.penetrate = 1;
			// projectile.ranged = false;
			//Projectile.minion = true; //TODO 1.4
			Projectile.DamageType = DamageClass.Summon;
		}
	}

	public class BoneSphereProjectile : SimpleMinion
	{
		public override int BuffId => -1;
		static int TimeToLive = 240;
		private Vector2 velocity = default;
		private int lastShotFrame = 0;
		public override void SetStaticDefaults()
		{
			// this is a bit sneaky, doesn't set any of the SimpleMinion defaults
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			// this is a bit sneaky, doesn't set any of the SimpleMinion defaults
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.penetrate = 15;
			Projectile.tileCollide = false;
			Projectile.timeLeft = TimeToLive;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Rectangle bounds = texture.Bounds;
			int timeElapsed = TimeToLive - Projectile.timeLeft;
			float scale = timeElapsed > 20 ? 1 : timeElapsed / 20f;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, lightColor, Projectile.rotation,
				bounds.GetOrigin(), scale, 0, 0);
			return false;
		}
		public override void AI()
		{
			if (velocity == default)
			{
				velocity = Projectile.velocity;
			}
			Projectile.rotation += 0.05f;
			int timeElapsed = TimeToLive - Projectile.timeLeft;
			if (timeElapsed % 2 == 0)
			{
				int dustIdx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 203);
				Main.dust[dustIdx].velocity = Vector2.Zero;
				Main.dust[dustIdx].noGravity = true;
			}
			if (timeElapsed < 30)
			{
				Projectile parent = GetMinionsOfType(ProjectileType<NecromancerMinion>()).FirstOrDefault();
				if (parent != default)
				{
					Projectile.velocity = Vector2.Zero;
					Projectile.Center = parent.Top + new Vector2(0, -8);
				}
			}
			else
			{
				Projectile.tileCollide = true;
				if (Projectile.velocity == Vector2.Zero)
				{
					Projectile.velocity = velocity;
				}
				if (Projectile.owner == Main.myPlayer && timeElapsed - lastShotFrame > 20 && AnyEnemyInRange(400f) is Vector2 target)
				{
					lastShotFrame = timeElapsed;
					Vector2 VectorToTarget = target - Projectile.Center;
					VectorToTarget.Normalize();
					VectorToTarget *= 12;
					VectorToTarget += Main.npc[(int)TargetNPCIndex].velocity;
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						VectorToTarget,
						ProjectileType<BoneSphereBoneProjectile>(),
						3 * Projectile.damage / 4,
						Projectile.knockBack,
						Projectile.owner);
				}
			}
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			for (int i = 0; i < 5; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 203);
			}
			for (int i = 0; i < 4; i++)
			{
				Vector2 angle = Main.rand.NextFloat(MathHelper.Pi).ToRotationVector2();
				angle *= 8;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					angle,
					ProjectileType<BoneSphereBoneProjectile>(),
					3 * Projectile.damage / 4,
					Projectile.knockBack,
					Projectile.owner);
			}
		}

		public override Vector2 IdleBehavior()
		{
			// no op
			return default;
		}

		public override Vector2? FindTarget()
		{
			// no op
			return default;
		}

		public override void IdleMovement(Vector2 VectorToIdlePosition)
		{
			// no op
		}

		public override void TargetedMovement(Vector2 VectorToTargetPosition)
		{
			// no op
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return true;
		}
	}

	public class NecromancerMinion : EmpoweredMinion
	{
		public override int BuffId => BuffType<NecromancerMinionBuff>();
		public override int CounterType => ProjectileType<NecromancerSkeletonMinion>();

		private int framesSinceLastHit;
		protected override int dustType => DustID.Shadowflame;
		int projId;

		int rateOfFire => Math.Max(90, 125 - 5 * EmpowerCount);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Necromancer");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.trailingInAir.Add(Projectile.type);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 36;
			Projectile.height = 46;
			Projectile.tileCollide = false;
			framesSinceLastHit = 0;
			DealsContactDamage = false;
			AttackThroughWalls = true;
			UseBeacon = false;
			FrameSpeed = 15;
		}


		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = Player.Top;
			idlePosition.X += -Player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingInAir, Projectile);
			idlePosition.Y += -24 + 8 * (float)Math.Sin(MathHelper.TwoPi * (AnimationFrame % 120) / 120);
			Vector2 VectorToIdlePosition = idlePosition - Projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, Player.Center, 1, 1))
			{
				idlePosition.X = Player.Top.X;
				idlePosition.Y = Player.Top.Y - 16;
			}
			TeleportToPlayer(ref VectorToIdlePosition, 2000f);
			return VectorToIdlePosition;
		}

		public override void IdleMovement(Vector2 VectorToIdlePosition)
		{
			base.IdleMovement(VectorToIdlePosition);
			if (VectorToTarget is null)
			{
				framesSinceLastHit = rateOfFire;
			}
			Lighting.AddLight(Projectile.position, Color.White.ToVector3() * 0.5f);
		}
		public override void TargetedMovement(Vector2 VectorToTargetPosition)
		{
			// stay floating behind the Player at all times
			IdleMovement(VectorToIdle);
			framesSinceLastHit++;
			int projectileVelocity = 6 + EmpowerCount / 2;
			if (framesSinceLastHit++ > rateOfFire)
			{
				// try to predict the position at the time of impact a bit
				VectorToTargetPosition.SafeNormalize();
				VectorToTargetPosition *= projectileVelocity;
				Vector2 pos = Projectile.Center;
				framesSinceLastHit = 0;
				Projectile.spriteDirection = VectorToTargetPosition.X > 0 ? 1 : -1;
				SoundEngine.PlaySound(SoundID.Item8, Projectile.position);
				if (Main.myPlayer == Player.whoAmI)
				{
					projId = Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						pos,
						VaryLaunchVelocity(VectorToTargetPosition),
						ProjectileType<BoneSphereProjectile>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer);
				}
			}
			else if (Main.myPlayer == Player.whoAmI && framesSinceLastHit == 30 && Main.projectile[projId].active)
			{
				VectorToTargetPosition.SafeNormalize();
				VectorToTargetPosition *= projectileVelocity;
				Main.projectile[projId].velocity = VectorToTargetPosition;
			}
		}

		protected override int ComputeDamage()
		{
			return (int)(baseDamage + (baseDamage / 12) * EmpowerCountWithFalloff()); // only scale up damage a little bit
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, Player.Center) is Vector2 target)
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

		protected override float ComputeInertia() => 5;

		protected override float ComputeTargetedSpeed() => 16;

		protected override float ComputeIdleSpeed() => 16;

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 4;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (VectorToTarget != null && framesSinceLastHit < 75)
			{
				Projectile.frame = 5;
			}
			else
			{
				base.Animate(minFrame, maxFrame);
			}
			if (Math.Abs(Projectile.velocity.X) > 2 && VectorToTarget is null)
			{
				Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
			}
		}
		public override bool CheckActive()
		{
			if (base.CheckActive() && Player.ownedProjectileCounts[CounterType] == 0 && AnimationFrame > 2)
			{
				Projectile.Kill();
				return false;
			}

			return true;
		}
	}

	public class NecromancerSkeletonMinion : SimpleGroundBasedMinion
	{
		public override int BuffId => BuffType<NecromancerMinionBuff>();
		const int explosionRespawnTime = 60;
		const int explosionRadius = 96;
		const int explosionAttackRechargeTime = 96;
		int lastExplosionFrame = 0;
		private Vector2 explosionLocation;
		private bool isDropping = true;

		private bool canAttack => AnimationFrame - lastExplosionFrame >= explosionAttackRechargeTime;
		private bool isRespawning => AnimationFrame - lastExplosionFrame < explosionRespawnTime;

		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (4, 4),
			[GroundAnimationState.JUMPING] = (4, 4),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (0, 4),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Necromancer Skeleton");
			Main.projFrames[Projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 28;
			Projectile.height = 30;
			DrawOriginOffsetY = -10;
			DrawOriginOffsetX = -1;
			attackFrames = 60;
			NoLOSPursuitTime = 300;
			StartFlyingHeight = 96;
			StartFlyingDist = 64;
			DefaultJumpVelocity = 4;
			MaxJumpVelocity = 12;
			searchDistance = 900;
			maxSpeed = 14;
			idleInertia = 6;
			Projectile.timeLeft = 3;
		}

		public override Vector2 IdleBehavior()
		{
			isDropping &= !canAttack;
			return base.IdleBehavior();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			isDropping = false;
			return base.OnTileCollide(oldVelocity);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// use a rectangular hitbox for the explosion. Easier than the alternative
			projHitbox = new Rectangle(
				(int)explosionLocation.X - explosionRadius,
				(int)explosionLocation.Y - explosionRadius,
				2 * explosionRadius,
				2 * explosionRadius);
			if (Vector2.DistanceSquared(explosionLocation, targetHitbox.Center.ToVector2()) < explosionRadius * explosionRadius)
			{
				return true;
			}
			return projHitbox.Intersects(targetHitbox);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextBool(2))
			{
				target.AddBuff(BuffID.ShadowFlame, 180);
			}
		}

		public override void IdleMovement(Vector2 VectorToIdlePosition)
		{
			if (!DoInactiveMovement())
			{
				base.IdleMovement(VectorToIdlePosition);
			}
		}

		private bool DoInactiveMovement()
		{
			if (isRespawning)
			{
				Projectile summoner = GetMinionsOfType(ProjectileType<NecromancerMinion>()).FirstOrDefault();

				// clamp to the summoner while respawning
				if (summoner != default)
				{
					Projectile.Top = summoner.Center;
					Projectile.velocity = summoner.velocity;
				}
				else
				{
					Projectile.position = Player.position;
					Projectile.velocity = Player.velocity;
				}
				return true;
			}
			else if (isDropping)
			{
				if (Projectile.velocity.Y < 16)
				{
					Projectile.velocity.Y += 0.6f;
				}
				Projectile.tileCollide = true;
				return true;
			}
			return false;
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -Projectile.height && Math.Abs(vector.X) < StartFlyingHeight)
			{
				GHelper.DoJump(vector);
			}
			float xInertia = GHelper.stuckInfo.overLedge && !GHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 3f : 8;
			float xMaxSpeed = 14f;
			if (VectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = Player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
		}

		public override void TargetedMovement(Vector2 VectorToTargetPosition)
		{
			if (DoInactiveMovement() || isDropping)
			{
				return;
			}
			else if (VectorToTargetPosition.Length() < explosionRadius / 2 && !UsingBeacon)
			{
				lastExplosionFrame = AnimationFrame;
				explosionLocation = Projectile.Center;
				isDropping = true;
				SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
				DoExplosionEffects();
			}
			else
			{
				base.TargetedMovement(canAttack ? VectorToTargetPosition : VectorToIdle);
			}
		}

		public override Vector2? FindTarget()
		{

			return canAttack ? base.FindTarget() : null;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return !isRespawning;
		}

		private void DoExplosionEffects()
		{
			Vector2 position = Projectile.position;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 30; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, 31, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 1.4f;
			}
			for (int i = 0; i < 20; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 3f;
			}
			var source = Projectile.GetSource_FromThis();
			for (float goreVel = 0.4f; goreVel < 0.8f; goreVel += 0.4f)
			{
				foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
				{
					int goreIdx = Gore.NewGore(source, position, default, Main.rand.Next(61, 64));
					Main.gore[goreIdx].velocity *= goreVel;
					Main.gore[goreIdx].velocity += offset;
				}
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (isDropping)
			{
				Projectile.rotation = 0;
				Projectile.frame = 4;
				return;
			}
			GroundAnimationState state = GHelper.DoGroundAnimation(frameInfo, base.Animate);
			if (state == GroundAnimationState.FLYING && AnimationFrame % 3 == 0)
			{
				int idx = Dust.NewDust(Projectile.Bottom, 8, 8, 16, -Projectile.velocity.X / 2, -Projectile.velocity.Y / 2);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = .9f;
			}
			return;
		}

		public override void AfterMoving()
		{
			Projectile.friendly = isRespawning && AnimationFrame - lastExplosionFrame <= 15;
			// Lifted from EmpoweredMinion.cs
			int minionType = ProjectileType<NecromancerMinion>();
			if (Player.whoAmI == Main.myPlayer && Player.ownedProjectileCounts[minionType] == 0 && IsPrimaryFrame)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(Projectile.type)[0].whoAmI == Projectile.whoAmI)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Player.Top, Vector2.Zero, minionType, Projectile.damage, Projectile.knockBack, Main.myPlayer);
				}
			}
		}
	}
}
