using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Core.Minions.Effects;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class SugarGliderMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SugarGliderMinion>() };
		public override string VanillaBuffName => "SugarGlider";
		public override int VanillaBuffId => BuffID.SugarGlider;
	}

	public class SugarGliderMinionItem : CombatPetMinionItem<SugarGliderMinionBuff, SugarGliderMinion>
	{
		internal override string VanillaItemName => "EucaluptusSap";
		internal override int VanillaItemID => ItemID.EucaluptusSap;
	}

	public class SugarGliderMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SugarGlider;
		internal override int BuffId => BuffType<SugarGliderMinionBuff>();

		internal MotionBlurDrawer blurDrawer;
		internal static int PlayRoughDuration = 120;

		internal NPC playRoughTarget;
		internal int playRoughStartFrame;
		internal Vector2 playRoughOffset;
		internal Vector2 playRoughVelocity;

		internal bool IsPlayingRough => playRoughTarget != null && playRoughTarget.active && 
			animationFrame - playRoughStartFrame < PlayRoughDuration;

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 20, -12, -20, -1);
			ConfigureFrames(10, (0, 0), (0, 5), (6, 6), (7, 9));
			blurDrawer = new MotionBlurDrawer(5);
		}


		public override void OnHitTarget(NPC target)
		{
			base.OnHitTarget(target);
			if(player.whoAmI != Main.myPlayer)
			{
				StartPlayingRough(target);
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			StartPlayingRough(target);
		}

		private void StartPlayingRough(NPC target)
		{
			// this is less accurate than OnHitNPC + MP sync, but it's easier to write
			if(!IsPlayingRough && leveledPetPlayer.PetLevel >= (int)CombatPetTier.Spectre)
			{
				playRoughTarget = target;
				playRoughStartFrame = animationFrame;
				playRoughOffset = target.Center - Projectile.Center;
				playRoughVelocity = playRoughOffset;
				playRoughVelocity.SafeNormalize();
				int targetSize = (target.width + target.height) / 2;
				playRoughVelocity *= Math.Max(8, targetSize / 10f);
			}
		}

		private void DoPlayRoughMovement()
		{
			if(animationFrame - playRoughStartFrame == PlayRoughDuration - 1)
			{
				Projectile.Center = playRoughTarget.Center;
				Projectile.velocity = playRoughVelocity + playRoughTarget.velocity;
				playRoughTarget = null;
				return;
			}
			Projectile.velocity = Vector2.Zero;
			Projectile.tileCollide = false;

			BouncePlayRoughVelocity();
			playRoughOffset += playRoughVelocity;
			Projectile.Center = playRoughTarget.Center + playRoughOffset;

			// smoke
			var source = Projectile.GetSource_FromThis();
			if (Main.rand.NextBool(8))
			{
				float goreVel = 0.25f;
				int goreIdx = Gore.NewGore(source, Projectile.Center, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= goreVel;
				Main.gore[goreIdx].velocity += playRoughTarget.velocity;
			}
			// stars
			if(Main.rand.NextBool(12))
			{
				Vector2 launchVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 4f);
				Gore.NewGore(source, Projectile.Center, launchVelocity, Main.rand.Next(16, 18));
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowStarDust, -launchVelocity * 1.25f);
				dust.noGravity = true;
			}
		}

		// If your name is SOPHIE...
		private void BouncePlayRoughVelocity()
		{
			Rectangle bounceRegion = playRoughTarget.Hitbox;
			bool didBounce = false;
			if((playRoughVelocity.Y < 0 && Projectile.Bottom.Y < bounceRegion.Top) ||
				(playRoughVelocity.Y > 0 && Projectile.Top.Y > bounceRegion.Bottom))
			{
				playRoughVelocity.Y *= -1;
				didBounce = true;
			}
			if((playRoughVelocity.X < 0 && Projectile.Right.X < bounceRegion.Left) ||
				(playRoughVelocity.X > 0 && Projectile.Left.X > bounceRegion.Right))
			{
				playRoughVelocity.X *= -1;
				didBounce = true;
			}

			if(didBounce)
			{
				playRoughVelocity = playRoughVelocity.RotatedByRandom(MathHelper.Pi / 8f);
			}
		}


		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(IsPlayingRough)
			{
				// TODO
				DoPlayRoughMovement();
			} else
			{
				playRoughTarget = null;
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually for some reason
			float r = Projectile.rotation;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			if(IsPlayingRough)
			{
				for (int k = 0; k < blurDrawer.BlurLength; k++)
				{
					if(!blurDrawer.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
					Main.EntitySpriteDraw(texture, blurPos - Main.screenPosition, bounds, blurColor, r, origin, 1, 0, 0);
				}
				// regular version
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
					bounds, lightColor, r, origin, 1, 0, 0);
			}
			return !IsPlayingRough;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurDrawer.Update(Projectile.Center, IsPlayingRough);
			Projectile.tileCollide &= !IsPlayingRough;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(IsPlayingRough)
			{
				(int, int?) walkFrames = frameInfo[GroundAnimationState.WALKING];
				base.Animate(walkFrames.Item1, walkFrames.Item2);
				Projectile.frame = 0;
				Projectile.spriteDirection = forwardDir;
				Projectile.rotation = playRoughVelocity.ToRotation();
			} else
			{
				base.Animate(minFrame, maxFrame);
			}
		}
	}
}
