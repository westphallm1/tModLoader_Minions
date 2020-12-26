using System;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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
		public static void DrawBlade(ISpinningBladeMinion self, SpriteBatch spriteBatch, Color lightColor, float rotation)
		{
			Vector2 pos = self.projectile.Center;
			Texture2D texture = GetTexture(self.Texture);
			Rectangle bounds = texture.Bounds;
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, rotation,
				origin, 1, 0, 0);
		}
		public static void DrawGlow(ISpinningBladeMinion self, SpriteBatch spriteBatch)
		{
			Vector2 pos = self.projectile.Center;
			Texture2D texture = GetTexture(self.Texture + "_Glow");
			Rectangle bounds = texture.Bounds;
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, Color.White, 0,
				origin, 1, 0, 0);
		}
	}

	public abstract class SpinningBladeMinion<T> : SurferMinion<T>, ISpinningBladeMinion where T: ModBuff
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

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			attackFrames = 60;
			idleInertia = 8;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(!isSpinning)
			{
				projectile.rotation = projectile.velocity.X * 0.05f;
			} else
			{
				projectile.rotation += 0.1f;
			}
		}

		protected abstract void DoDrift(Vector2 driftVelocity);
		protected abstract void DoSpin(Vector2 spinVelocity);
		protected abstract void StopSpin();

		protected abstract void SummonSecondBlade(Vector2 vectorToTargetPosition);

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(!isSpinning && vectorToTargetPosition.Length() < SpinStartDistance)
			{
				isSpinning = true;
				spinAnimationCounter = 0;
				if (Main.myPlayer == player.whoAmI)
				{
					SummonSecondBlade(vectorToTargetPosition);
				}
			}
			vectorToTargetPosition.SafeNormalize();
			if(isSpinning)
			{
				if(spinAnimationCounter++ > SpinAnimationLength)
				{
					isSpinning = false;
					StopSpin();
				} else if(spinAnimationCounter > SpinAnimationLength - SpinTravelLength)
				{
					DoSpin(spinVector);
				} else
				{
					DoDrift(npcVelocity);
				}
			} 
			else
			{
				vectorToTargetPosition *= 8;
				int inertia = 18;
				projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// always finish the spin
			if(isSpinning)
			{
				TargetedMovement(Vector2.Zero);
				projectile.tileCollide = true; // can phase through walls like this otherwise
				return;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// draw the back blade
			float bladeAngle = (3 * MathHelper.Pi * animationFrame) / animationFrames;
			if(!isSpinning)
			{
				SpinningBladeDrawer.DrawBlade(this, spriteBatch, lightColor, -bladeAngle);
				SpinningBladeDrawer.DrawBlade(this, spriteBatch, lightColor, bladeAngle);
			} else
			{
				SpinningBladeDrawer.DrawBlade(this, spriteBatch, lightColor, projectile.rotation);
			}
			SpinningBladeDrawer.DrawGlow(this, spriteBatch);
			return false;
		}

	}
}
