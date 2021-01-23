using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions.GoblinGunner;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.GoblinTechnomancer
{
	public class GoblinTechnomancerMinionBuff : MinionBuff
	{
		public GoblinTechnomancerMinionBuff() : base(ProjectileType<GoblinTechnomancerBombMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Bomb Buddy");
			Description.SetDefault("A bomb buddy will explode for you!");
		}
	}

	public class GoblinTechnomancerMinionItem : MinionItem<GoblinTechnomancerMinionBuff, GoblinTechnomancerBombMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Unstable Detonator");
			Tooltip.SetDefault("Summons a bomb buddy to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 60;
			item.knockBack = 5.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 5, 0);
			item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			foreach(int itemId in new int[] { ItemID.CrimtaneBar, ItemID.DemoniteBar})
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(itemId, 12);
				recipe.AddIngredient(ItemID.Bomb, 20);
				recipe.AddTile(TileID.Anvils);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
		}
	}

	public class GoblinTechnomancerBombMinion : SimpleGroundBasedMinion
	{
		protected override int BuffId => BuffType<GoblinTechnomancerMinionBuff>();
		const int explosionRespawnTime = 60;
		const int explosionRadius = 96;
		const int explosionAttackRechargeTime = 96;
		int lastExplosionFrame = 0;
		private Vector2 explosionLocation;
		private bool isDropping = true;

		private bool didJustRespawn => animationFrame - lastExplosionFrame == explosionRespawnTime;
		private bool canAttack => animationFrame - lastExplosionFrame >= explosionAttackRechargeTime;
		private bool isRespawning => animationFrame - lastExplosionFrame < explosionRespawnTime;

		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (1, 1),
			[GroundAnimationState.JUMPING] = (1, 1),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (2, 9),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Technomancer Bomb");
			Main.projFrames[projectile.type] = 10;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 30;
			projectile.height = 30;
			drawOriginOffsetY = -10;
			drawOriginOffsetX = -2;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
			searchDistance = 900;
			maxSpeed = 14;
			idleInertia = 6;
		}

		public override Vector2 IdleBehavior()
		{
			gHelper.SetIsOnGround();
			// the ground-based slime can sometimes bounce its way around 
			// a corner, but the flying version can't
			noLOSPursuitTime = gHelper.isFlying ? 15 : 300;
			List<Projectile> minions = GetActiveMinions();
			int order = minions.IndexOf(projectile);
			Vector2 idlePosition = player.Center;
			idlePosition.X += (36 + order * 26) * -player.direction;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Center;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			isDropping &= !canAttack;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
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
				2*explosionRadius, 
				2*explosionRadius);
			if(Vector2.DistanceSquared(explosionLocation, targetHitbox.Center.ToVector2()) < explosionRadius * explosionRadius)
			{
				return true;
			}
			return  base.Colliding(projHitbox, targetHitbox);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if(Main.rand.Next(2) == 0)
			{
				target.AddBuff(BuffID.ShadowFlame, 180);
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(!DoInactiveMovement())
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		private bool DoInactiveMovement()
		{
			if(isRespawning)
			{
				Projectile summoner = GetMinionsOfType(ProjectileType<GoblinTechnomancerMinion>()).FirstOrDefault();

				// clamp to the summoner while respawning
				if(summoner != default)
				{
					projectile.Top = summoner.Center;
					projectile.velocity = summoner.velocity;
				}
				else
				{
					projectile.position = player.position;
					projectile.velocity = player.velocity;
				}
				return true;
			} else if (isDropping)
			{
				if(projectile.velocity.Y < 16)
				{
					projectile.velocity.Y += 0.6f;
				}
				projectile.tileCollide = true;
				return true;
			}
			return false;
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{

			if(vector.Y < -projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 3f : 8;
			float xMaxSpeed = 14f;
			if(vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(DoInactiveMovement())
			{
				return;
			} else if (isDropping)
			{

			} else if (vectorToTargetPosition.Length() < explosionRadius/2)
			{
				lastExplosionFrame = animationFrame;
				explosionLocation = projectile.Center;
				isDropping = true;
				Main.PlaySound(SoundID.Item62, projectile.Center);
				DoExplosionEffects();
			} else
			{
				base.TargetedMovement(canAttack ? vectorToTargetPosition : vectorToIdle);
			}
		}

		public override Vector2? FindTarget()
		{

			return canAttack? base.FindTarget() : null;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return !isRespawning;
		}

		private void DoExplosionEffects()
		{
			Vector2 position = projectile.position;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 30; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, 31, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 1.4f;
			}
			for (int i= 0; i < 20; i ++)
			{
				int dustIdx = Dust.NewDust(position, width, height, DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 3f;
			}
			for (float goreVel = 0.4f; goreVel < 0.8f; goreVel += 0.4f)
			{
				foreach(Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1)})
				{
					int goreIdx = Gore.NewGore(position, default, Main.rand.Next(61, 64));
					Main.gore[goreIdx].velocity *= goreVel;
					Main.gore[goreIdx].velocity += offset;
				}
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (isDropping)
			{
				projectile.rotation += 0.05f;
				projectile.frame = 9;
				return;
			}
			GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			if(state == GroundAnimationState.FLYING && animationFrame % 3 == 0)
			{
				int idx = Dust.NewDust(projectile.Bottom, 8, 8, 16, -projectile.velocity.X / 2, -projectile.velocity.Y / 2);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = .9f;
			}
			return;
		}

		public override void AfterMoving()
		{
			projectile.friendly = isRespawning && animationFrame - lastExplosionFrame <= 15;
			// Lifted from EmpoweredMinion.cs
			int minionType = ProjectileType<GoblinTechnomancerMinion>();
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[minionType] == 0)
			{
				// hack to prevent multiple 
				if(GetMinionsOfType(projectile.type)[0].whoAmI == projectile.whoAmI)
				{
					Projectile.NewProjectile(player.Top, Vector2.Zero, minionType, projectile.damage, projectile.knockBack, Main.myPlayer);
				}
			}
		}
	}
	public class GoblinTechnomancerMinion : EmpoweredMinion
	{
		protected override int BuffId => BuffType<GoblinTechnomancerMinionBuff>();
		protected override int CounterType => ProjectileType<GoblinTechnomancerBombMinion>();

		private int framesSinceLastHit;
		protected override int dustType => DustID.Shadowflame;

		private Vector2 lastShotVector;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Technomancer");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 28;
			projectile.height = 42;
			projectile.tileCollide = false;
			framesSinceLastHit = 0;
			projectile.friendly = true;
			attackThroughWalls = true;
			useBeacon = false;
		}



		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture + "_Arms");
			int frame = 0;
			float shootSlope = default;
			if(framesSinceHadTarget < 30 && lastShotVector != default && framesSinceLastHit <= 30)
			{
				float denominator = Math.Max(Math.Abs(lastShotVector.X), 1);
				shootSlope = lastShotVector.Y / denominator;
			}
			if(shootSlope != default)
			{
				if(shootSlope > 0.75f)
				{
					frame = 1;
				} else if (shootSlope > -0.75f)
				{
					frame = 2;
				} else
				{
					frame = 3;
				}
				DrawWeapon(spriteBatch, lightColor);
			}
			Rectangle bounds = new Rectangle(0, frame * texture.Height/4, texture.Width, texture.Height/4);
			Vector2 origin = new Vector2(bounds.Width/2, bounds.Height / 2);
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0;
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, projectile.rotation,
				origin, 1, effects, 0);
		}

		// lifted from WeaponHoldingSquire
		private float GetWeaponAngle(Vector2 attackVector)
		{
			if (projectile.spriteDirection == 1)
			{
				return attackVector.ToRotation();
			}
			else
			{
				// this code is rather unfortunate, but need to normalize
				// everything to [-Math.PI/2, Math.PI/2] for arm drawing to work
				float angle = (float)-Math.PI + attackVector.ToRotation();
				if (angle < -Math.PI / 2)
				{
					angle += 2 * (float)Math.PI;
				}
				return angle;
			}
		}

		private void DrawWeapon(SpriteBatch spriteBatch, Color lightColor)
		{
			Vector2 offset = lastShotVector;
			offset.Y *= -1;
			offset.SafeNormalize();
			Texture2D texture = GetTexture(Texture+"_Gun");
			Rectangle bounds = new Rectangle(0, 0, texture.Width, texture.Height);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2); // origin should hopefully be more or less center of squire
			float r = GetWeaponAngle(offset);
			Vector2 pos = projectile.Center + new Vector2(0, 8) + 16 * offset;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			idlePosition.X += 32 * -player.direction;
			idlePosition.Y += -24 + 8 * (float)Math.Sin(MathHelper.TwoPi * (animationFrame % 120) / 120);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			if (!Collision.CanHitLine(idlePosition, 1, 1, player.Center, 1, 1))
			{
				idlePosition.X = player.Top.X;
				idlePosition.Y = player.Top.Y - 16;
			}
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// stay floating behind the player at all times
			IdleMovement(vectorToIdle);
			framesSinceLastHit++;
			int rateOfFire = Math.Max(50, 90 - 5 * EmpowerCount);
			int projectileVelocity = 40;
			if (framesSinceLastHit++ > rateOfFire && targetNPCIndex is int npcIdx)
			{
				NPC target = Main.npc[npcIdx];
				// try to predict the position at the time of impact a bit
				lastShotVector = vectorToTargetPosition;
				lastShotVector.Y *= -1;
				vectorToTargetPosition += (vectorToTargetPosition.Length() / projectileVelocity) * target.velocity;
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= projectileVelocity;
				Vector2 pos = projectile.Center;
				framesSinceLastHit = 0;
				projectile.spriteDirection = vectorToTargetPosition.X > 0 ? 1 : -1;
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
			}
		}

		protected override int ComputeDamage()
		{
			return baseDamage/2 + (baseDamage / 8) * EmpowerCount; // only scale up damage a little bit
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
			return 800 + 20 * EmpowerCount;
		}

		protected override float ComputeInertia() => 5;

		protected override float ComputeTargetedSpeed() => 16;

		protected override float ComputeIdleSpeed() => 16;

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
				projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;
			}
			projectile.rotation = projectile.velocity.X * 0.025f;
		}
	}
}
