using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class BabySlimeMinionBuff : MinionBuff
	{
		public BabySlimeMinionBuff() : base(ProjectileType<BabySlimeMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.BabySlime") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.BabySlime"));
		}

	}

	public class BabySlimeMinionItem : MinionItem<BabySlimeMinionBuff, BabySlimeMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.SlimeStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ItemName.SlimeStaff") + " (AoMM Version)");
			Tooltip.SetDefault(Language.GetTextValue("ItemTooltip.SlimeStaff"));
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.SlimeStaff);
			base.SetDefaults();
		}
	}

	public class BabySlimeMinion : SimpleGroundBasedMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.BabySlime;
		protected override int BuffId => BuffType<BabySlimeMinionBuff>();
		private float intendedX = 0;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.BabySlime") + " (AoMM Version)");
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 20;
			projectile.height = 20;
			drawOffsetX = (projectile.width - 44) / 2;
			drawOriginOffsetY = 0;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
			searchDistance = 600;
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

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1, effects, 0);
			return false;
		}

		protected override bool CheckForStuckness()
		{
			return true;
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{
			// always jump "long" if we're far away from the enemy
			if (Math.Abs(vector.X) > startFlyingAtTargetDist && vector.Y < -32)
			{
				vector.Y = -32;
			}
			gHelper.DoJump(vector);
			int maxHorizontalSpeed = vector.Y < -64 ? 4 : 6;
			if(targetNPCIndex is int idx && vector.Length() < 64)
			{
				// go fast enough to hit the enemy while chasing them
				Vector2 targetVelocity = Main.npc[idx].velocity;
				projectile.velocity.X = Math.Max(4, Math.Min(maxHorizontalSpeed, Math.Abs(targetVelocity.X) * 1.25f)) * Math.Sign(vector.X);
			} else
			{
				maxHorizontalSpeed = vector.Y < -64 ? 4 : 8;
				// try to match the player's speed while not chasing an enemy
				projectile.velocity.X = Math.Max(1, Math.Min(maxHorizontalSpeed, Math.Abs(vector.X) / 16)) * Math.Sign(vector.X);
			}
			intendedX = projectile.velocity.X;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = gHelper.isFlying ? 2 : 0;
			maxFrame = gHelper.isFlying ? 6 : 2;
			base.Animate(minFrame, maxFrame);
		}
	}
}
