using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets;
using Terraria.ModLoader;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class PlantPupMinionBuff : CombatPetBuff
    {
        internal override int[] ProjectileTypes => new int[] { ProjectileType<PlantPupMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Plant Pup");
			Description.SetDefault("A playful plant has joined your adventure!");
		}
	}

	public class PlantPupMinionItem : CombatPetCustomMinionItem<PlantPupMinionBuff, PlantPupMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Calm Bow of Friendship");
			Tooltip.SetDefault("Summons a pet Plant Pup!");
		}
	}

	public class LeafBlade : ModProjectile
	{
		private MotionBlurDrawer blurDrawer;
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Leaf;
		private int TimeToLive = 180;
		private int CircleRadius = 32;
		private int CircleDuration = 40;

		// this is a lot of state to have to store...
		private Vector2 originalVelocity;
		private Vector2 currentVelocity;
		private NPC target;
		private int circleStartFrame;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			blurDrawer = new MotionBlurDrawer(10);
			Projectile.friendly = true;
			Projectile.timeLeft = TimeToLive;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.penetrate = 5;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			blurDrawer.DrawBlur(texture, lightColor, bounds, Projectile.rotation);
			return false;
		}

		public override void AI()
		{
			base.AI();
			int frame = TimeToLive - Projectile.timeLeft;
			Projectile.frame = (frame / 5) % 5;
			if(target != default && !target.active)
			{
				target = default;
			}
			if(circleStartFrame != default && circleStartFrame - Projectile.timeLeft < CircleDuration)
			{
				int currentFrame = circleStartFrame - Projectile.timeLeft;
				float rotationSign = Math.Sign(currentFrame - CircleDuration / 2);
				currentVelocity = currentVelocity.RotatedBy(rotationSign * 2 * MathHelper.TwoPi / CircleDuration);
				Projectile.velocity = (target?.velocity ?? default) + currentVelocity;
				Projectile.tileCollide = false;
			} 
			else if (circleStartFrame != default)
			{
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			ModProjectileExtensions.ClientSideNPCHitCheck(this);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if(circleStartFrame == default)
			{
				circleStartFrame = Projectile.timeLeft;
				originalVelocity = Projectile.velocity;
				currentVelocity = Projectile.velocity;
				this.target = target;
			}
			Projectile.damage = (int) (Projectile.damage * 0.90f);
		}

		public override void PostAI()
		{
			blurDrawer.Update(Projectile.Center, true);
		}
	}

	public class PlantPupMinion : CombatPetGroundedRangedMinion
	{
		public override int BuffId => BuffType<PlantPupMinionBuff>();

		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Spectre ?
			ProjectileType<LeafBlade>() :
			ProjectileType<SaplingMinionLeafProjectile>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 30, -6, -8, -1);
			ConfigureFrames(10, (0, 1), (2, 6), (3, 3), (7, 9));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.GetAnimationState();
			FrameSpeed = (state == GroundAnimationState.STANDING) ? 10 : 5;
			base.Animate(minFrame, maxFrame);
			if(state == GroundAnimationState.JUMPING)
			{
				Projectile.frame = Projectile.velocity.Y > 0 ? 2 : 5;
			}
		}
	}
}
