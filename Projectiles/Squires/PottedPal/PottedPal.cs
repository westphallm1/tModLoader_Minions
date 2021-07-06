using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.PottedPal
{
	public class PottedPalMinionBuff : MinionBuff
	{
		public PottedPalMinionBuff() : base(ProjectileType<PottedPalMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Potted Pal");
			Description.SetDefault("A friendly plant will follow your orders!");
		}
	}

	public class PottedPalMinionItem : SquireMinionItem<PottedPalMinionBuff, PottedPalMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Potted Pal");
			Tooltip.SetDefault("Summons a squire\nA friendly plant will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.width = 24;
			item.height = 38;
			item.damage = 51;
			item.value = Item.sellPrice(0, 5, 0, 0);
			item.rare = ItemRarityID.Pink;
		}
	}

	public class PottedPalMinion : SquireMinion
	{
		internal override int BuffId => BuffType<PottedPalMinionBuff>();
		public PottedPalMinion() : base(ItemType<PottedPalMinionItem>()) { }
		protected int wingFrameCounter = 0;
		static int hitCooldown = 6;
		static int cooldownCounter;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Potted Pal");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			frameSpeed = 15;
			projectile.localNPCHitCooldown = 10;
			cooldownCounter = hitCooldown;
		}


		public override Vector2 IdleBehavior()
		{
			wingFrameCounter++;
			cooldownCounter++;
			return base.IdleBehavior();
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (vectorToIdle.Length() > 32)
			{
				Vector2 vectorFromPlayer = player.Center - projectile.Center;
				projectile.rotation = vectorFromPlayer.ToRotation() - (float)Math.PI / 2;
			}
			else
			{
				projectile.rotation = 0;
			}
			base.IdleMovement(vectorToIdlePosition);
		}


		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			projectile.velocity = -projectile.velocity;
			projectile.velocity.SafeNormalize();
			projectile.velocity *= ModifiedTargetedSpeed();
		}

		public override Vector2? FindTarget()
		{
			if (cooldownCounter >= hitCooldown)
			{
				return base.FindTarget();
			}
			return null;
		}
		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (cooldownCounter < hitCooldown)
			{
				return;
			}
			// Cling to the closest enemy a little bit
			if (vectorToTargetPosition.Length() < 100f && GetClosestEnemyToPosition(syncedMouseWorld, 100f, true) is NPC target)
			{
				base.StandardTargetedMovement(target.Center - projectile.Center);
			}
			else
			{
				base.StandardTargetedMovement(vectorToTargetPosition);
			}
			Vector2 vectorFromPlayer = player.Center - projectile.Center;
			projectile.rotation = vectorFromPlayer.ToRotation() - (float)Math.PI / 2;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Vector2 vineEnd = vectorToIdle + new Vector2(0, 8);
			Vector2 center = projectile.Center;
			Rectangle bounds = new Rectangle(0, 36, 16, 16);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Vector2 pos;
			float r;
			if (vineEnd.Length() > 16)
			{
				Vector2 unitToIdle = vineEnd;
				unitToIdle.Normalize();
				Texture2D vineTexture = GetTexture(Texture);
				r = (float)Math.PI / 2 + vineEnd.ToRotation();
				int i;
				for (i = bounds.Height / 2; i < vineEnd.Length(); i += bounds.Height)
				{
					if (vineEnd.Length() - i < bounds.Height / 2)
					{
						i = (int)(vineEnd.Length() - bounds.Height / 2);
					}
					bounds.Y = bounds.Y == 36 ? 54 : 36;
					pos = center + unitToIdle * i;
					lightColor = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16);
					spriteBatch.Draw(vineTexture, pos - Main.screenPosition,
						bounds, lightColor, r,
						origin, 1, SpriteEffects.None, 0);
				}
			}
			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r;
			Vector2 pos;
			int wingFrame = (wingFrameCounter % 20) / 5;
			Texture2D potTexture = GetTexture(Texture + "_Pot");
			int frameHeight = potTexture.Height / 4;
			Rectangle bounds = new Rectangle(0, wingFrame * frameHeight, potTexture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			if (vectorToIdle.Length() > 16 || vectorToTarget is Vector2 target)
			{
				pos = projectile.Center + vectorToIdle + new Vector2(0, 8); // move pot down a bit;
				r = player.velocity.X * 0.05f;
			}
			else
			{
				pos = projectile.Center + new Vector2(0, 12);
				r = projectile.rotation;
			}
			lightColor = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16);
			spriteBatch.Draw(potTexture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, SpriteEffects.None, 0);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (vectorToTarget is null)
			{
				projectile.frame = 0;
			}
			else
			{
				maxFrame = 2;
				base.Animate(minFrame, maxFrame);
			}
			projectile.spriteDirection = 1;
		}

		public override float ComputeIdleSpeed() => 14;

		public override float ComputeTargetedSpeed() => 15;

		public override float MaxDistanceFromPlayer() => 600f;
	}
}
