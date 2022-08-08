using AmuletOfManyMinions.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses
{
	public interface ISpinningBladeMinion
	{
		Projectile projectile { get; }
		string Texture { get; }
	}

	public static class SpinningBladeDrawer
	{
		public static void DrawBlade(ISpinningBladeMinion self, Color lightColor, float rotation)
		{
			Vector2 pos = self.projectile.Center;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[self.projectile.type].Value;
			Rectangle bounds = texture.Bounds;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, rotation, bounds.GetOrigin(), 1, 0, 0);
		}
		public static void DrawGlow(ISpinningBladeMinion self)
		{
			Vector2 pos = self.projectile.Center;
			Texture2D texture = Request<Texture2D>(self.Texture + "_Glow").Value;
			Rectangle bounds = texture.Bounds;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, Color.White, 0, bounds.GetOrigin(), 1, 0, 0);
		}
	}

	public abstract class SpinningBladeMinion : HeadCirclingGroupAwareMinion, ISpinningBladeMinion
	{
		internal bool isSpinning = false;
		internal Vector2 spinVector = default;
		internal Vector2 npcVelocity = default;
		internal float SpinStartDistance = 64f;
		internal int SpinAnimationLength = 40;
		internal int SpinTravelLength = 10;
		internal int spinAnimationCounter;
		internal int SpinVelocity = 8;

		protected abstract int bladeType { get; }

		public Projectile projectile => this.projectile;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			targetSearchDistance = 800;
			Projectile.width = 16;
			Projectile.height = 16;
			attackFrames = 60;
			idleInertia = 8;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (!isSpinning)
			{
				Projectile.rotation = Projectile.velocity.X * 0.05f;
			}
			else
			{
				Projectile.rotation += 0.15f;
			}
		}

		protected virtual void DoDrift(Vector2 driftVelocity)
		{
			Projectile.velocity = driftVelocity;
		}

		protected virtual void DoSpin(Vector2 spinVelocity)
		{
			Projectile.velocity = spinVelocity;
		}

		protected virtual void StopSpin()
		{
			Projectile.velocity = Vector2.Zero;
		}

		protected virtual void SummonSecondBlade(Vector2 vectorToTargetPosition)
		{
			Vector2 launchVelocity = vectorToTargetPosition;
			launchVelocity.SafeNormalize();
			launchVelocity *= SpinVelocity;
			npcVelocity = Main.npc[(int)TargetNPCIndex].velocity;
			launchVelocity += launchVelocity;
			spinVector = launchVelocity;
			if (Main.myPlayer == Player.whoAmI)
			{
				int projId = Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					launchVelocity,
					bladeType,
					Projectile.damage,
					Projectile.knockBack,
					Player.whoAmI,
					ai1: SpinAnimationLength - SpinTravelLength);
				Main.projectile[projId].timeLeft = SpinAnimationLength + 1;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (!isSpinning && vectorToTargetPosition.Length() < SpinStartDistance)
			{
				isSpinning = true;
				spinAnimationCounter = 0;
				if (Main.myPlayer == Player.whoAmI)
				{
					SummonSecondBlade(vectorToTargetPosition);
				}
			}
			vectorToTargetPosition.SafeNormalize();
			if (isSpinning)
			{
				if (spinAnimationCounter++ > SpinAnimationLength)
				{
					isSpinning = false;
					StopSpin();
				}
				else if (spinAnimationCounter > SpinAnimationLength - SpinTravelLength)
				{
					DoSpin(spinVector);
				}
				else
				{
					DoDrift(npcVelocity);
				}
			}
			else
			{
				vectorToTargetPosition *= 8;
				int inertia = 18;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// always finish the spin
			if (isSpinning)
			{
				TargetedMovement(Vector2.Zero);
				Projectile.tileCollide = true; // can phase through walls like this otherwise
				return;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		protected virtual float GetBackBladeAngle()
		{
			return (6 * MathHelper.Pi * AnimationFrame) / GroupAnimationFrames;
		}

		protected virtual float GetFrontBladeAngle()
		{
			return -GetBackBladeAngle();
		}

		protected virtual float GetSpinningBladeAngle()
		{
			return Projectile.rotation;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!isSpinning)
			{
				SpinningBladeDrawer.DrawBlade(this, lightColor, GetBackBladeAngle());
				SpinningBladeDrawer.DrawBlade(this, lightColor, GetFrontBladeAngle());
			}
			else
			{
				SpinningBladeDrawer.DrawBlade(this, lightColor, GetSpinningBladeAngle());
			}
			SpinningBladeDrawer.DrawGlow(this);
			return false;
		}

	}
}
