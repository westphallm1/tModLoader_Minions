using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.CrossModClient.SummonersShine;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets;
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
		internal override int[] ProjectileTypes => new int[] { ProjectileType<ImpMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault(Language.GetTextValue("BuffName.ImpMinion") + " (AoMM Version)");
			// Description.SetDefault(Language.GetTextValue("BuffDescription.ImpMinion"));
		}
	}

	public class ImpMinionItem : VanillaCloneMinionItem<ImpMinionBuff, ImpMinion>
	{
		internal override int VanillaItemID => ItemID.ImpStaff;

		internal override string VanillaItemName => "ImpStaff";

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.UseSound = SoundID.Item77;
		}
	}

	public abstract class BaseImpFireball : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ImpFireball;

		internal virtual int DustType => 6;
		internal virtual float DustScale => 2f;
		
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			// Projectile.CloneDefaults(ProjectileID.ImpFireball);
			base.SetDefaults();
			Projectile.penetrate = 3;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}

		public override void PostAI()
		{
			for (int i = 0; i < 2; i++)
			{
				AddDust(Projectile.position, Projectile.width, Projectile.height);
			}
		}

		internal void AddDust(Vector2 position, int width, int height)
		{
				int dustId = Dust.NewDust(
					position - new Vector2(width, height) /2 , width, height, DustType, 
					Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 
					100, default, DustScale);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity.X *= 0.3f;
				Main.dust[dustId].velocity.Y *= 0.3f;
				Main.dust[dustId].noLight = true;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 240);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}

	public class ImpFireball : BaseImpFireball
	{
	}

	public abstract class BaseMinionUnholyTrident: ModProjectile
	{

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.UnholyTridentFriendly);
			base.SetDefaults();
			Projectile.timeLeft = 45;
			Projectile.localNPCHitCooldown = 30;
			Projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			base.AI();
			Dust dust = Dust.NewDustDirect(Projectile.position, 8, 8, 6, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1f);
			dust.velocity *= -0.25f;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			for (int i = 4; i < 31; i++)
			{
				for(int j = 0; j < 2; j++)
				{
					float xOffset = Projectile.oldVelocity.X * (30f / i);
					float yOffset = Projectile.oldVelocity.Y * (30f / i);
					Dust dust = Dust.NewDustDirect(Projectile.position - new Vector2(xOffset, yOffset), 8, 8, 6, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 2f - j);
					dust.noGravity = j == 0;
					dust.velocity *= 0.5f;
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 240);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color translucentColor = new Color(164, 164, 164, 128);
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);

			// regular
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				bounds.GetOrigin(), 1, 0, 0);
			return false;
		}
	}
	public class ImpPortalUnholyTrident: BaseMinionUnholyTrident
	{

	}

	public class ImpMinion : HoverShooterMinion
	{
		public override int BuffId => BuffType<ImpMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FlyingImp;

		internal override int? FiredProjectileId => ProjectileType<ImpFireball>();

		internal override SoundStyle? ShootSound => SoundID.Item20;

		internal Projectile flameRing;

		// if more than 4 copies are spawned, coordinate as a circle
		internal bool isBeingUsedAsToken;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void ApplyCrossModChanges()
		{
			Imp.CrossModChanges(Type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 26;
			targetSearchDistance = 750;
			DrawOffsetX = (Projectile.width - 44) / 2;
			attackFrames = 60;
			hsHelper.attackFrames = attackFrames;
			hsHelper.targetShootProximityRadius = 128;

			Imp.SetDeaults_Imp(this);
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= 4)
				{
					Projectile.frame = 0;
				}
			}
			if(VectorToTarget is Vector2 target)
			{
				Projectile.spriteDirection = -Math.Sign(target.X);
			} else if(Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = -1;
			}
			else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = 1;
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			// vanilla code for sparkly dust
			if (Main.rand.NextBool(6))
			{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100, default, 2f);
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			}
		}
		public override Vector2 IdleBehavior()
		{
			AttackThroughWalls = isBeingUsedAsToken;
			if(isBeingUsedAsToken)
			{
				int minionType = ProjectileType<ImpPortalMinion>();
				flameRing = Main.projectile.Where(p => p.active && p.owner == Projectile.owner && p.type == minionType).FirstOrDefault();
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

			if (CrossModSetup.SummonersShineLoaded)
			{
				Imp.ImpTargetedMovement(vectorToTargetPosition, Projectile, this);
			}

			if (isBeingUsedAsToken)
			{
				IdleMovement(VectorToIdle);	
			}
			else
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}
		internal void DoCircleFlameRing()
		{
			List<Projectile> otherImps = GetMinionsOfType(Projectile.type);
			if(otherImps.Count == 0)
			{
				return;
			}
			int myIndex = otherImps.FindIndex(p => p.whoAmI == Projectile.whoAmI);
			float startAngle = -2f * (float)Math.PI * GroupAnimationFrame / 120;
			float angle = startAngle + myIndex * 2 * (float)Math.PI / otherImps.Count;
			Vector2 rotation = angle.ToRotationVector2();
			rotation.X *= 0.75f;
			Vector2 pos = flameRing.Center + 48 * rotation;
			Vector2 vectorToTarget = pos - Projectile.Center;
			if(vectorToTarget.Length() > hsHelper.travelSpeed)
			{
				vectorToTarget.Normalize();
				vectorToTarget *= hsHelper.travelSpeed;
			}
			int inertia = hsHelper.inertia;
			if(vectorToTarget.Length() < 4)
			{
				Projectile.velocity = vectorToTarget;
			} else
			{
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTarget) / inertia;
			}
		}

		public override void AfterMoving()
		{
			int minionType = ProjectileType<ImpPortalMinion>();
			bool lastBeingUsedAsToken = isBeingUsedAsToken;
			isBeingUsedAsToken = Player.ownedProjectileCounts[Projectile.type] > 3;
			if(isBeingUsedAsToken != lastBeingUsedAsToken)
			{
				for(int i = 0; i < 3; i++)
				{
					int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100, default, 2f);
					Main.dust[dustId].velocity *= 0.3f;
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].noLight = true;
				}
			}
			if (Player.whoAmI == Main.myPlayer && Player.ownedProjectileCounts[Projectile.type] > 3 && Player.ownedProjectileCounts[minionType] == 0 && IsPrimaryFrame)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(Projectile.type)[0].whoAmI == Projectile.whoAmI)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Player.Top, Vector2.Zero, minionType, Projectile.damage, Projectile.knockBack, Main.myPlayer);
				}
			}
		}
	}

	/// <summary>
	/// Uses localAI[0] to signal to imps whether it's attacking
	/// </summary>
	public class ImpPortalMinion : EmpoweredMinion
	{
		public override int BuffId => BuffType<ImpMinionBuff>();
		public override int CounterType => ProjectileType<ImpMinion>();
		protected override int dustType => 6;

		protected int BabyImpCount => Player.ownedProjectileCounts[ProjectileType<BabyImpMinion>()];
		protected override int EmpowerCount => base.EmpowerCount + BabyImpCount;

		public override string Texture => "Terraria/Images/NPC_" + NPCID.RedDevil;

		internal bool isCloseToTarget = false;
		internal HoverShooterHelper hsHelper;
		internal int stayInPlaceFrames = 0;

		internal bool IsAttacking
		{
			get => Projectile.localAI[0] > 0;
			set => Projectile.localAI[0] = value ? 1 : 0;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = true;
			AttackThroughWalls = false;
			Projectile.width = 32;
			Projectile.height = 32;
			FrameSpeed = 5;
			// can hit many npcs at once, so give it a relatively high on hit cooldown
			Projectile.localNPCHitCooldown = 20;
			hsHelper = new HoverShooterHelper(this, ProjectileType<ImpPortalUnholyTrident>())
			{
				attackFrames = 30,
				projectileVelocity = 14,
				targetShootProximityRadius = 256,
				CustomFireProjectile = FireTridents,
				ModifyTargetVector = HandleTargetProximity,
				AfterFiringProjectile = () => SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.5f }, Projectile.position)
			};
		}


		private void FireTridents(Vector2 lineOfFire, int projId, float ai0)
		{
			int fireCount = 1 + Main.rand.Next(3);
			int fireRadius = Main.rand.Next(8, 24);
			bool hasBaby = BabyImpCount > 0;
			for(int i = 0; i < fireCount; i ++)
			{
				float startAngle = -2f * (float)Math.PI * AnimationFrame / 120;
				float angle = startAngle + i * MathHelper.TwoPi / fireCount;
				Vector2 fireOffset = angle.ToRotationVector2() * fireRadius;
				fireOffset.X *= 0.75f;
				if(!Collision.CanHit(Projectile.Center, 1, 1, Projectile.Center + fireOffset, 1, 1))
				{
					fireOffset = Vector2.Zero;
				}
				Vector2 randomShoot = lineOfFire + Main.rand.NextFloatDirection().ToRotationVector2();
				int damage = Projectile.damage;
				if(hasBaby && Main.rand.NextBool(EmpowerCount))
				{
					projId = ProjectileType<BabyImpFireBall>();
					damage = Player.GetModPlayer<LeveledCombatPetModPlayer>().PetDamage;
				}
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center + fireOffset,
					VaryLaunchVelocity(randomShoot),
					projId,
					damage,
					Projectile.knockBack,
					Main.myPlayer,
					ai0: ai0);
			}
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			hsHelper.attackFrames = Math.Max(12, 24 - EmpowerCount);
			Vector2 vectorToIdlePosition = Player.Top - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}


		public override void AfterMoving()
		{
			base.AfterMoving();
			if(EmpowerCount > 0 && EmpowerCount < 4)
			{
				Projectile.Kill();
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
				Projectile.velocity = Vector2.Zero;
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
			return (int)(baseDamage * ( 1 + EmpowerCountWithFalloff()) / 5);
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
			float startAngle = -2f * (float)Math.PI * AnimationFrame / 120;
			bool hasBaby = BabyImpCount > 0;
			for (int i = 0; i < 20; i++)
			{
				float angle = startAngle + i * 2 * (float)Math.PI / 20;
				Vector2 rotation = angle.ToRotationVector2();
				rotation.X *= 0.75f;
				Vector2 pos = Projectile.Center + 28 * rotation;
				Dust dust;
				if(Main.rand.NextBool() || !hasBaby)
				{
					dust = Dust.NewDustDirect(pos, 1, 1, 6, 0f, 0f, 100, default, 1.25f);
				} else
				{
					dust = Dust.NewDustDirect(pos, 1, 1, DustID.Shadowflame, 0f, 0f, 100, default, 1f);
				}
				dust.noGravity = Main.rand.Next(120) > 0;
				dust.noLight = true;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}


	}
}
