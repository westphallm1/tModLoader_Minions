using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.SlimeTrain
{
	/// <summary>
	/// Uses ai[1] to track which color it is
	/// ai[0] used by GroupAwareMinion
	/// </summary>
	public class SlimeTrainSlimeMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<SlimeTrainMinionBuff>();
		private float intendedX = 0;
		private Projectile parent;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slime Train Passenger");
			Main.projFrames[projectile.type] = 6;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.minion = false;
			projectile.minionSlots = 0;
			projectile.width = 20;
			projectile.height = 20;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
			searchDistance = 700;
			maxSpeed = 20;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();

			noLOSPursuitTime = 15; // no long pursuit like regular grounded minions
			parent = default;
			int parentType = ProjectileType<SlimeTrainMinion>();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == player.whoAmI && p.type == parentType)
				{
					parent = p;
					break;
				}
			}
			if(parent == default)
			{
				projectile.Kill();
			}
			return parent.Center - projectile.Center;
		}

		protected override bool DoPreStuckCheckGroundedMovement()
		{
			if (!gHelper.didJustLand)
			{
				projectile.velocity.X = intendedX;
				// only path after landing
				return false;
			}
			return true;
		}

		public override bool ShouldIgnoreNPC(NPC npc)
		{
			// ignore any npc with a marker actively placed on it
			return base.ShouldIgnoreNPC(npc) || Main.projectile.Any(p =>
				p.active && p.owner == player.whoAmI &&
				p.type == ProjectileType<SlimeTrainMarkerProjectile>() && (int)p.ai[0] == npc.whoAmI);
		}

		protected override bool CheckForStuckness() => true;

		protected override void DoGroundedMovement(Vector2 vector)
		{
			// always jump "long" if we're far away from the enemy
			if (Math.Abs(vector.X) > startFlyingAtTargetDist && vector.Y < -32)
			{
				vector.Y = -32;
			}
			gHelper.DoJump(vector);
			int maxHorizontalSpeed = vector.Y < -64 ? 8 : 12;
			if(targetNPCIndex is int idx && vector.Length() < 64)
			{
				// go fast enough to hit the enemy while chasing them
				Vector2 targetVelocity = Main.npc[idx].velocity;
				projectile.velocity.X = Math.Max(4, Math.Min(maxHorizontalSpeed, Math.Abs(targetVelocity.X) * 1.25f)) * Math.Sign(vector.X);
			} else
			{
				// try to match the player's speed while not chasing an enemy
				projectile.velocity.X = Math.Max(1, Math.Min(maxHorizontalSpeed, Math.Abs(vector.X) / 16)) * Math.Sign(vector.X);
			}
			intendedX = projectile.velocity.X;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// if we get too close to the parent, get back onto the train (die)
			if(Vector2.DistanceSquared(parent.Center, projectile.Center) < 32 * 32)
			{
				projectile.Kill();
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			lightColor = Color.White * 0.85f;
			lightColor.A = 128;
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int nColors = 7;
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			int frameWidth = texture.Width / nColors;
			Rectangle bounds = new Rectangle(frameWidth * (int)(projectile.ai[1] % nColors), projectile.frame * frameHeight, frameWidth, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = gHelper.isFlying ? 2 : 0;
			maxFrame = gHelper.isFlying ? 6 : 2;
			base.Animate(minFrame, maxFrame);
		}
	}
}
