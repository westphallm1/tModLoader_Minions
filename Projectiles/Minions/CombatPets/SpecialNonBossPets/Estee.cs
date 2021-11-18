using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class EsteeMinionBuff : CombatPetVanillaCloneBuff
	{
		public EsteeMinionBuff() : base(ProjectileType<EsteeMinion>()) { }
		public override string VanillaBuffName => "UpbeatStar";
		public override int VanillaBuffId => BuffID.UpbeatStar;
	}

	public class EsteeMinionItem : CombatPetMinionItem<EsteeMinionBuff, EsteeMinion>
	{
		internal override string VanillaItemName => "CelestialWand";
		internal override int VanillaItemID => ItemID.CelestialWand;

		internal override int AttackPatternUpdateTier => 4;
	}

	public struct CometTrailDrawer
	{
		public static void DrawCometTrail(Projectile projectile, Texture2D cometTexture, int animationFrame, ref float cometRotation)
		{
			if(projectile.velocity.LengthSquared() < 16)
			{
				cometRotation = default;
				return;
			} 
			float velocityRotation = projectile.velocity.ToRotation();
			if (cometRotation == default)
			{
				cometRotation = velocityRotation;
			} else
			{
				cometRotation = Utils.AngleLerp(cometRotation, velocityRotation, 0.25f);
			}
			float alphaAdjustment = Math.Min(64, projectile.velocity.LengthSquared()) / 64f;
			Vector2 cometDirection = cometRotation.ToRotationVector2();
			float drawRotation = cometRotation + MathHelper.PiOver2;
			int cometFrame = animationFrame % 60;
			int cometLength = cometTexture.Height;
			float cometAngle = MathHelper.TwoPi * cometFrame / 60f;
			Rectangle cometBounds = cometTexture.Bounds;
			Vector2 cometOrigin = cometBounds.Center.ToVector2();
			Vector2 pos = projectile.Center - Main.screenPosition;
			// maybe should encode this in a struct. There are 4 layers of varying scales, opacities, and positions

			// two dark blue layers at varying offsets
			Vector2 trailPos = pos + 4 * cometAngle.ToRotationVector2() - cometDirection * cometLength * 0.35f;
			Main.EntitySpriteDraw(cometTexture, trailPos, cometBounds, Color.Blue * 0.05f * alphaAdjustment, 
				drawRotation, cometOrigin, 1.25f, 0, 0);
			trailPos = pos + 3 * (MathHelper.PiOver2 - cometAngle).ToRotationVector2() - cometDirection * cometLength * 0.3f;
			Main.EntitySpriteDraw(cometTexture, trailPos, cometBounds, Color.Blue * 0.05f * alphaAdjustment, 
				drawRotation, cometOrigin, 1f, 0, 0);

			// larger pulsing light blue layer
			float pulseScale = MathF.Sin(MathHelper.Pi * (animationFrame % 30) / 30f);
			trailPos = pos - cometDirection * cometLength * (0.15f + pulseScale/10);
			float scale = 0.4f + 0.4f * pulseScale;
			Color drawColor = Color.PaleTurquoise * (0.8f - 0.5f * pulseScale);
			drawColor.A = 0;
			Main.EntitySpriteDraw(cometTexture, trailPos, cometBounds, drawColor * alphaAdjustment, 
				drawRotation, cometOrigin, scale, 0, 0);

			// smaller static light blue layer
			trailPos = pos - cometDirection * cometLength * 0.15f;
			drawColor = Color.PaleTurquoise * 0.8f;
			drawColor.A = 0;
			Main.EntitySpriteDraw(cometTexture, trailPos, cometBounds, drawColor * alphaAdjustment, 
				drawRotation, cometOrigin, 0.5f, 0, 0);
		}

		public static void AddImpactEffects(Projectile projectile)
		{
			for(int i = 0; i < 3; i++)
			{
				Vector2 launchVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(3f, 5f);
				Gore.NewGore(projectile.Center, launchVelocity, Main.rand.Next(16, 18));
				Dust dust = Dust.NewDustPerfect(projectile.Center, DustID.YellowStarDust, -launchVelocity * 1.25f);
				dust.noGravity = true;
			}
		}
	}

	/// <summary>
	/// Uses ai[0] for NPC id to target
	/// </summary>
	public class EsteeFallenStarProjectile : ModProjectile
	{
		public override string Texture => "Terraria/Images/Item_"+ItemID.FallenStar;
		private NPC targetNPC;
		private float maxSpeed = 16;
		private float cometRotation;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 15;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			Projectile.rotation += MathHelper.Pi / 16f;
			Projectile.frameCounter++;
			Projectile.frame = (Projectile.frameCounter / 5) % 15;
			if(targetNPC == null)
			{
				targetNPC = Main.npc[(int)Projectile.ai[0]];
			}
			if(targetNPC.active && targetNPC.Center.Y > Projectile.Center.Y)
			{
				int inertia = 4;
				Vector2 target = targetNPC.Center - Projectile.Center;
				target.Normalize();
				target *= maxSpeed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + target) / inertia;
			} else
			{
				Projectile.tileCollide = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D starTexture = TextureAssets.Item[ItemID.FallenStar].Value;
			Texture2D cometTexture = TextureAssets.Extra[ExtrasID.FallingStar].Value;
			CometTrailDrawer.DrawCometTrail(Projectile, cometTexture, Projectile.timeLeft, ref cometRotation);
			int starFrames = 8;
			int starFrame = Projectile.frame < starFrames ? Projectile.frame : 2 * starFrames - 1 - Projectile.frame;
			int starHeight = starTexture.Height / 8;
			Rectangle starBounds = new Rectangle(0, starFrame * starHeight, starTexture.Width, starHeight);
			Vector2 starOrigin = new Vector2(starBounds.Width, starBounds.Height) / 2;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(starTexture, pos, starBounds, Color.White, 
				Projectile.rotation, starOrigin, 1, 0, 0);
			return false;
		}

		public override void Kill(int timeLeft)
		{
			CometTrailDrawer.AddImpactEffects(Projectile);
		}
	}

	public class EsteeMinion : CombatPetHoverDasherMinion
	{
		internal override int BuffId => BuffType<EsteeMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.UpbeatStar;
		internal override int? FiredProjectileId => null;
		internal override bool DoBumblingMovement => true;

		internal float cometRotation;
		private int lastShootFrame;

		public override void LoadAssets()
		{
			base.LoadAssets();
			Main.instance.LoadItem(ItemID.FallenStar);
		}
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 15;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			resetIdleRotation = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D starTexture = TextureAssets.Item[ItemID.FallenStar].Value;
			Texture2D eyesTexture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D cometTexture = TextureAssets.Extra[ExtrasID.FallingStar].Value;

			CometTrailDrawer.DrawCometTrail(Projectile, cometTexture, animationFrame, ref cometRotation);

			int starFrames = 8;
			int starFrame = Projectile.frame < starFrames ? Projectile.frame : 2 * starFrames - 1 - Projectile.frame;
			int starHeight = starTexture.Height / 8;
			Rectangle starBounds = new Rectangle(0, starFrame * starHeight, starTexture.Width, starHeight);
			Vector2 starOrigin = new Vector2(starBounds.Width, starBounds.Height) / 2;
			Vector2 pos = Projectile.Center - Main.screenPosition;

			// star
			Main.EntitySpriteDraw(starTexture, pos, starBounds, Color.White, 
				Projectile.rotation, starOrigin, 1, 0, 0);
			// eyes
			float eyeRotation = 0.05f * Projectile.velocity.X;
			Main.EntitySpriteDraw(eyesTexture, pos, eyesTexture.Bounds, Color.White, 
				eyeRotation, eyesTexture.Bounds.Center.ToVector2(), 1, 0, 0);
			return false;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			Projectile.frame = (animationFrame / 5) % Main.projFrames[Projectile.type];
			if(Projectile.velocity.LengthSquared() > 16)
			{
				Projectile.rotation += Math.Sign(Projectile.velocity.X) * MathHelper.Pi / 15f;
			} else
			{
				Projectile.rotation = Utils.AngleLerp(Projectile.rotation, 0.05f * Projectile.velocity.X, 0.25f);
			}
			if(Main.rand.Next(24) == 0)
			{
				int dustIdx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust);
				Main.dust[dustIdx].noGravity = true;
				Main.dust[dustIdx].velocity *= 0.25f;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			CometTrailDrawer.AddImpactEffects(Projectile);
			if(Projectile.owner == Main.myPlayer && animationFrame - lastShootFrame > attackFrames / 2 && leveledPetPlayer.PetLevel >= 4)
			{
				lastShootFrame = animationFrame;
				// spawn projectile slightly off the top of the screen
				Vector2 spawnPos = Main.screenPosition + new Vector2(Main.rand.Next(0, Main.screenWidth), -16);
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					spawnPos,
					Projectile.velocity,
					ProjectileType<EsteeFallenStarProjectile>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer,
					ai0: target.whoAmI);
				SoundEngine.PlaySound(new LegacySoundStyle(2, 9), spawnPos);
			}
		}
	}
}
