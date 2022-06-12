using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ModLoader;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class CloudiphantMinionBuff : CombatPetBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<CloudiphantMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Cloudiphant");
			Description.SetDefault("An ethereal elephant has joined your adventure!");
		}
	}

	public class CloudiphantMinionItem : CombatPetCustomMinionItem<CloudiphantMinionBuff, CloudiphantMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Relaxed Bow of Friendship");
			Tooltip.SetDefault("Summons a pet Cloudiphant!");
		}
	}

	public class TwisterProjectile: ModProjectile
	{
		internal int TimeToLive = 300;
		internal int wallBounceCountDown = 0;
		internal NPC target;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = TimeToLive;
			Projectile.penetrate = 3;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			Projectile.width = 16;
			Projectile.height = 16;
		}

		public override void AI()
		{
			Projectile.frame++;
			Projectile.rotation = 0.02f * Projectile.velocity.X;
			if(Main.rand.NextBool(3))
			{
				AddDust();
			}

			if(target != default && !target.active)
			{
				target = default;
			}
			if(target == default)
			{
				ModProjectileExtensions.ClientSideNPCHitCheck(this);
			}
			if(target != default)
			{
				int inertia = 30;
				float speed = 12;
				Vector2 targetOffset = target.Center - Projectile.Center;
				targetOffset.SafeNormalize();
				targetOffset *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + targetOffset) / inertia;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return target == default;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(target, damage, knockback, crit);
			this.target ??= target;
			Projectile.damage = (int)(Projectile.damage * 0.85f);
		}
		
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameIdx = (Projectile.frame / 5) % 6;
			int frameHeight = texture.Height / 6;
			Rectangle bounds = new Rectangle(0, frameIdx * frameHeight, texture.Width, frameHeight);
			lightColor *= 0.75f;
			Main.EntitySpriteDraw(
				texture, Projectile.Center - Main.screenPosition, 
				bounds, lightColor, Projectile.rotation, 
				bounds.GetOrigin(), 1f, 0, 0);
			return false;
		}
		private void AddDust()
		{
			int dustCreated = Dust.NewDust(Projectile.Center, 1, 1, DustID.Cloud, Projectile.velocity.X, Projectile.velocity.Y, 0, Scale: 1.4f);
			Main.dust[dustCreated].noGravity = true;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				AddDust();
			}
		}
	}


	public class CloudPuffProjectile : BaseMinionBubble
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameIdx = (Projectile.frame / 10) % 4;
			int frameHeight = texture.Height / 4;
			Rectangle bounds = new Rectangle(0, frameIdx * frameHeight, texture.Width, frameHeight);
			float scale = 1f + 0.2f * MathF.Sin(MathHelper.TwoPi * Projectile.frame / 90f);
			lightColor *= 0.75f;
			Main.EntitySpriteDraw(
				texture, Projectile.Center - Main.screenPosition, 
				bounds, lightColor, Projectile.rotation, 
				bounds.GetOrigin(), scale, 0, 0);
			return false;
		}

		public override void AI()
		{
			base.AI();
			if(Main.rand.NextBool(5))
			{
				AddDust();
			}
			Projectile.rotation += MathHelper.Pi/30 * Math.Sign(Projectile.velocity.X);
			Projectile.frame++;
			int speed = 12;
			int inertia = 30;
			if(Projectile.timeLeft < 150 && 
				Minion.GetClosestEnemyToPosition(Projectile.Center, 200f, requireLOS: true) is NPC target)
			{
				Vector2 targetVector = target.Center - Projectile.Center;
				targetVector.SafeNormalize();
				targetVector *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + targetVector) / inertia;
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			// no-op
		}

		private void AddDust()
		{
			int dustCreated = Dust.NewDust(Projectile.Center, 1, 1, DustID.Cloud, Projectile.velocity.X, Projectile.velocity.Y, 0, Scale: 1.4f);
			Main.dust[dustCreated].noGravity = true;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				AddDust();
			}
		}
	}

	public class CloudiphantMinion : CombatPetGroundedRangedMinion
	{
		internal override int BuffId => BuffType<CloudiphantMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 24, -6, -10, -1);
			ConfigureFrames(9, (0, 0), (1, 5), (1, 1), (6, 8));
		}

		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => (leveledPetPlayer?.PetLevel ?? 0) >= (int)CombatPetTier.Spectre ?
			ProjectileType<TwisterProjectile>() :
			ProjectileType<CloudPuffProjectile>();

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			if(ProjId == ProjectileType<TwisterProjectile>())
			{
				attackFrames = (int)(attackFrames * 1.5f);
			}
			return target;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.GetAnimationState();
			frameSpeed = (state == GroundAnimationState.WALKING) ? 5 : 10;
			base.Animate(minFrame, maxFrame);
			if(state == GroundAnimationState.JUMPING)
			{
				Projectile.frame = Projectile.velocity.Y > 0 ? 2 : 5;
			} 
		}
	}
}
