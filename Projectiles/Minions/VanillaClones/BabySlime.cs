using AmuletOfManyMinions.Core;
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
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabySlimeMinion>() };

		public override LocalizedText DisplayName => AoMMSystem.AppendAoMMVersion(Language.GetText("BuffName.BabySlime"));

		public override LocalizedText Description => Language.GetText("BuffDescription.BabySlime");
	}

	public class BabySlimeMinionItem : VanillaCloneMinionItem<BabySlimeMinionBuff, BabySlimeMinion>
	{
		internal override int VanillaItemID => ItemID.SlimeStaff;

		internal override string VanillaItemName => "SlimeStaff";
	}

	public class BabySlimeMinion : SimpleGroundBasedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabySlime;

		public override LocalizedText DisplayName => AoMMSystem.AppendAoMMVersion(Language.GetText("ProjectileName.BabySlime"));

		public override int BuffId => BuffType<BabySlimeMinionBuff>();
		private float intendedX = 0;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 20;
			Projectile.height = 20;
			DrawOffsetX = (Projectile.width - 44) / 2;
			DrawOriginOffsetY = 0;
			attackFrames = 60;
			NoLOSPursuitTime = 300;
			StartFlyingHeight = 96;
			StartFlyingDist = 64;
			DefaultJumpVelocity = 4;
			MaxJumpVelocity = 12;
			searchDistance = 600;
		}

		protected override bool DoPreStuckCheckGroundedMovement()
		{
			if (!GHelper.didJustLand)
			{
				Projectile.velocity.X = intendedX;
				// only path after landing
				return false;
			}
			return true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r, bounds.GetOrigin(), 1, effects, 0);
			return false;
		}

		protected override bool CheckForStuckness()
		{
			return true;
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{
			// always jump "long" if we're far away from the enemy
			if (Math.Abs(vector.X) > StartFlyingDist && vector.Y < -32)
			{
				vector.Y = -32;
			}
			GHelper.DoJump(vector);
			int maxHorizontalSpeed = vector.Y < -64 ? 4 : 6;
			if(TargetNPCIndex is int idx && vector.Length() < 64)
			{
				// go fast enough to hit the enemy while chasing them
				Vector2 targetVelocity = Main.npc[idx].velocity;
				Projectile.velocity.X = Math.Max(4, Math.Min(maxHorizontalSpeed, Math.Abs(targetVelocity.X) * 1.25f)) * Math.Sign(vector.X);
			} else
			{
				maxHorizontalSpeed = vector.Y < -64 ? 4 : 8;
				// try to match the player's speed while not chasing an enemy
				Projectile.velocity.X = Math.Max(1, Math.Min(maxHorizontalSpeed, Math.Abs(vector.X) / 16)) * Math.Sign(vector.X);
			}
			intendedX = Projectile.velocity.X;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = GHelper.isFlying ? 2 : 0;
			maxFrame = GHelper.isFlying ? 6 : 2;
			base.Animate(minFrame, maxFrame);
		}
	}
}
