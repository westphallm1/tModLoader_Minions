using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class ImpMinionBuff : MinionBuff
	{
		public ImpMinionBuff() : base(ProjectileType<ImpMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.ImpMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.ImpMinion"));
		}
	}

	public class ImpMinionItem : VanillaCloneMinionItem<ImpMinionBuff, ImpMinion>
	{
		internal override int VanillaItemID => ItemID.ImpStaff;

		internal override string VanillaItemName => "ImpStaff";

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.UseSound = new LegacySoundStyle(2, 77);
		}
	}

	public class ImpFireball : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.ImpFireball;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.ImpFireball);
			base.SetDefaults();
			projectile.localNPCHitCooldown = 30;
			projectile.usesLocalNPCImmunity = true;
		}

		public override void PostAI()
		{
			for (int i = 0; i < 2; i++)
			{
				int dustId = Dust.NewDust(
					projectile.position, 
					projectile.width, projectile.height, 6, 
					projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 
					100, default, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity.X *= 0.3f;
				Main.dust[dustId].velocity.Y *= 0.3f;
				Main.dust[dustId].noLight = true;
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 240);
		}
	}

	public class ImpPortalUnholyTrident: ModProjectile
	{

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.UnholyTridentFriendly);
			base.SetDefaults();
			projectile.timeLeft = 45;
			projectile.localNPCHitCooldown = 30;
			projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			base.AI();
			Dust dust = Dust.NewDustDirect(projectile.position, 8, 8, 6, projectile.oldVelocity.X, projectile.oldVelocity.Y, 100, default, 1f);
			dust.velocity *= -0.25f;
		}

		public override void Kill(int timeLeft)
		{
			Main.PlaySound(SoundID.Item10, projectile.position);
			for (int i = 4; i < 31; i++)
			{
				for(int j = 0; j < 2; j++)
				{
					float xOffset = projectile.oldVelocity.X * (30f / i);
					float yOffset = projectile.oldVelocity.Y * (30f / i);
					Dust dust = Dust.NewDustDirect(projectile.position - new Vector2(xOffset, yOffset), 8, 8, 6, projectile.oldVelocity.X, projectile.oldVelocity.Y, 100, default, 2f - j);
					dust.noGravity = j == 0;
					dust.velocity *= 0.5f;
				}
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 240);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Color translucentColor = new Color(164, 164, 164, 128);
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);

			// regular
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1, 0, 0);
			return false;
		}
	}

	public class ImpMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<ImpMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.FlyingImp;

		internal override int? FiredProjectileId => ProjectileType<ImpFireball>();

		internal override LegacySoundStyle ShootSound => SoundID.Item20;

		internal Projectile flameRing;

		// if more than 4 copies are spawned, coordinate as a circle
		internal bool isBeingUsedAsToken;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Imp");
			Main.projFrames[projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 26;
			targetSearchDistance = 700;
			drawOffsetX = (projectile.width - 44) / 2;
			attackFrames = 65;
			hsHelper.attackFrames = attackFrames;
			idleBumble = true;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= 4)
				{
					projectile.frame = 0;
				}
			}
			if(vectorToTarget is Vector2 target)
			{
				projectile.spriteDirection = -Math.Sign(target.X);

			} else if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			}
			else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
			projectile.rotation = projectile.velocity.X * 0.05f;

			// vanilla code for sparkly dust
			if (Main.rand.Next(6) == 0)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100, default, 2f);
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			}
		}
		public override Vector2 IdleBehavior()
		{
			attackThroughWalls = isBeingUsedAsToken;
			if(isBeingUsedAsToken)
			{
				int minionType = ProjectileType<ImpPortalMinion>();
				flameRing = Main.projectile.Where(p => p.active && p.owner == projectile.owner && p.type == minionType).FirstOrDefault();
			} else
			{
				flameRing = default;
			}
			return base.IdleBehavior();
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(isBeingUsedAsToken && flameRing != default && flameRing.localAI[0] > 0)
			{
				DoCircleFlameRing();
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(isBeingUsedAsToken)
			{
				IdleMovement(vectorToIdle);	
			} else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}
		internal void DoCircleFlameRing()
		{
			List<Projectile> otherImps = GetMinionsOfType(projectile.type);
			if(otherImps.Count == 0)
			{
				return;
			}
			int myIndex = otherImps.FindIndex(p => p.whoAmI == projectile.whoAmI);
			float startAngle = -2f * (float)Math.PI * groupAnimationFrame / 120;
			float angle = startAngle + myIndex * 2 * (float)Math.PI / otherImps.Count;
			Vector2 rotation = angle.ToRotationVector2();
			rotation.X *= 0.75f;
			Vector2 pos = flameRing.Center + 48 * rotation;
			Vector2 vectorToTarget = pos - projectile.Center;
			if(vectorToTarget.Length() > hsHelper.travelSpeed)
			{
				vectorToTarget.Normalize();
				vectorToTarget *= hsHelper.travelSpeed;
			}
			int inertia = hsHelper.inertia;
			if(vectorToTarget.Length() < 4)
			{
				projectile.velocity = vectorToTarget;
			} else
			{
				projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTarget) / inertia;
			}
		}

		public override void AfterMoving()
		{
			int minionType = ProjectileType<ImpPortalMinion>();
			bool lastBeingUsedAsToken = isBeingUsedAsToken;
			isBeingUsedAsToken = player.ownedProjectileCounts[projectile.type] > 3;
			if(isBeingUsedAsToken != lastBeingUsedAsToken)
			{
				for(int i = 0; i < 3; i++)
				{
					int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100, default, 2f);
					Main.dust[dustId].velocity *= 0.3f;
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].noLight = true;
				}
			}
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projectile.type] > 3 && player.ownedProjectileCounts[minionType] == 0)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(projectile.type)[0].whoAmI == projectile.whoAmI)
				{
					Projectile.NewProjectile(player.Top, Vector2.Zero, minionType, projectile.damage, projectile.knockBack, Main.myPlayer);
				}
			}
		}
	}

	/// <summary>
	/// Uses localAI[0] to signal to imps whether it's attacking
	/// </summary>
	public class ImpPortalMinion : EmpoweredMinion
	{
		protected override int BuffId => BuffType<ImpMinionBuff>();
		protected override int CounterType => ProjectileType<ImpMinion>();
		protected override int dustType => 6;


		public override string Texture => "Terraria/NPC_"+NPCID.RedDevil;

		internal bool isCloseToTarget = false;
		internal HoverShooterHelper hsHelper;
		internal int stayInPlaceFrames = 0;

		internal bool IsAttacking
		{
			get => projectile.localAI[0] > 0;
			set => projectile.localAI[0] = value ? 1 : 0;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copter-X");
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.tileCollide = true;
			attackThroughWalls = false;
			projectile.width = 32;
			projectile.height = 32;
			frameSpeed = 5;
			// can hit many npcs at once, so give it a relatively high on hit cooldown
			projectile.localNPCHitCooldown = 20;
			hsHelper = new HoverShooterHelper(this, ProjectileType<ImpPortalUnholyTrident>())
			{
				attackFrames = 30,
				projectileVelocity = 14,
				targetShootProximityRadius = 256,
				CustomFireProjectile = FireTridents,
				ModifyTargetVector = HandleTargetProximity,
				AfterFiringProjectile = () => Main.PlaySound(new LegacySoundStyle(2, 8).WithVolume(0.5f), projectile.position)
			};
		}


		private void FireTridents(Vector2 lineOfFire, int projId, float ai0)
		{
			int fireCount = 1 + Main.rand.Next(3);
			int fireRadius = Main.rand.Next(8, 24);
			if(targetNPCIndex is int idx && Main.npc[idx].active)
			{
				lineOfFire += Main.npc[idx].velocity / 6;
			}
			for(int i = 0; i < fireCount; i ++)
			{
				float startAngle = -2f * (float)Math.PI * animationFrame / 120;
				float angle = startAngle + i * MathHelper.TwoPi / fireCount;
				Vector2 fireOffset = angle.ToRotationVector2() * fireRadius;
				fireOffset.X *= 0.75f;
				if(!Collision.CanHit(projectile.Center, 1, 1, projectile.Center + fireOffset, 1, 1))
				{
					fireOffset = Vector2.Zero;
				}
				Vector2 randomShoot = lineOfFire + Main.rand.NextFloatDirection().ToRotationVector2();
				Projectile.NewProjectile(
					projectile.Center + fireOffset,
					randomShoot,
					projId,
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer,
					ai0: ai0);
			}
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			hsHelper.attackFrames = Math.Max(12, 24 - EmpowerCount);
			return player.Top - projectile.Center;
		}


		public override void AfterMoving()
		{
			base.AfterMoving();
			if(EmpowerCount > 0 && EmpowerCount < 4)
			{
				projectile.Kill();
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			IsAttacking = true;
			hsHelper.TargetedMovement(vectorToTargetPosition);
		}
		private void HandleTargetProximity(ref Vector2 target)
		{
			if(target.Length() < hsHelper.targetShootProximityRadius * 0.67f)
			{
				stayInPlaceFrames = 20;
			}
			if(stayInPlaceFrames -- > 0)
			{
				isCloseToTarget = true;
				projectile.velocity = Vector2.Zero;
			} else
			{
				isCloseToTarget = hsHelper.inAttackRange;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			IsAttacking = false;
			isCloseToTarget = false;
			base.IdleMovement(vectorToIdlePosition);
		}

		protected override int ComputeDamage()
		{
			return baseDamage * ( 2 + EmpowerCount) / 5;
		}

		protected override float ComputeSearchDistance()
		{
			return 800;
		}

		protected override float ComputeInertia()
		{
			return 12;
		}

		protected override float ComputeTargetedSpeed()
		{
			return 9 + EmpowerCount;
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			// no op
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			// draw a circle out of sparks dust
			if(!isCloseToTarget)
			{
				return;
			}
			float startAngle = -2f * (float)Math.PI * animationFrame / 120;
			for (int i = 0; i < 20; i++)
			{
				float angle = startAngle + i * 2 * (float)Math.PI / 20;
				Vector2 rotation = angle.ToRotationVector2();
				rotation.X *= 0.75f;
				Vector2 pos = projectile.Center + 28 * rotation;
				Dust dust = Dust.NewDustDirect(pos, 1, 1, 6, 0f, 0f, 100, default, 1.25f);
				dust.noGravity = Main.rand.Next(120) > 0;
				dust.noLight = true;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}


	}
}
