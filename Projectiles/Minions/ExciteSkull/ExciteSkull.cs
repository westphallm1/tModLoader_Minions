using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.ExciteSkull
{
	public class ExciteSkullMinionBuff : MinionBuff
	{
		public ExciteSkullMinionBuff() : base(ProjectileType<ExciteSkullMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Skull Biker");
			Description.SetDefault("A skeletal motorcyclist is fighting for you!");
		}
	}

	public class ExciteSkullMinionItem : MinionItem<ExciteSkullMinionBuff, ExciteSkullMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Skeletal Keychain");
			Tooltip.SetDefault("Summons a skull biker to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 19;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 1, 50, 0);
			item.rare = ItemRarityID.Green;
		}
	}

	public class ExciteSkullMinion : SimpleGroundBasedMinion
	{
		protected override int BuffId => BuffType<ExciteSkullMinionBuff>();
		private Color flairColor;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("ExciteSkull");
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 28;
			drawOffsetX = -2;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 13;
			searchDistance = 700;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			float brightness = (lightColor.R + lightColor.G + lightColor.B) / (3f * 255f);
			Color flairColor = this.flairColor;
			flairColor.R = (byte)(flairColor.R * brightness);
			flairColor.G = (byte)(flairColor.G * brightness);
			flairColor.B = (byte)(flairColor.B * brightness);
			//texture = GetTexture(Texture+"_Glow");
			//spriteBatch.Draw(texture, pos - Main.screenPosition,
			//	texture.Bounds, flairColor, projectile.rotation,
			//	texture.Bounds.Center.ToVector2(), 1, effects, 0);
			texture = GetTexture(Texture+"_Rider");
			spriteBatch.Draw(texture, pos + new Vector2(0, -8) - Main.screenPosition,
				texture.Bounds, lightColor, 0,
				texture.Bounds.Center.ToVector2(), 1, effects, 0);
			texture = GetTexture(Texture+"_RiderGlow");
			spriteBatch.Draw(texture, pos + new Vector2(0, -8) - Main.screenPosition,
				texture.Bounds, flairColor, 0,
				texture.Bounds.Center.ToVector2(), 1, effects, 0);
		}

		public override void OnSpawn()
		{
			flairColor = player.GetModPlayer<MinionSpawningItemPlayer>().GetNextColor();
		}
		
		protected override void DoGroundedMovement(Vector2 vector)
		{
			if(vector.Y < -projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 7;
			int xMaxSpeed = 9;
			if(vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if(animationFrame - lastHitFrame > 10)
			{
				projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
			else
			{
				projectile.velocity.X = Math.Sign(projectile.velocity.X) * xMaxSpeed * 0.75f;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(gHelper.didJustLand)
			{
				projectile.rotation = 0;
			} else
			{
				projectile.rotation = -projectile.spriteDirection * MathHelper.Pi / 8;
			}
			if(Math.Abs(projectile.velocity.X) < 1)
			{
				return;
			}
			base.Animate(minFrame, maxFrame);
			if(((gHelper.didJustLand && Math.Abs(projectile.velocity.X) > 4) || gHelper.isFlying) && animationFrame % 3 == 0)
			{
				int idx = Dust.NewDust(projectile.Bottom, 8, 8, 16, -projectile.velocity.X / 2, -projectile.velocity.Y / 2);
				Main.dust[idx].alpha = 112;
				Main.dust[idx].scale = .9f;
			}
		}
	}
}
