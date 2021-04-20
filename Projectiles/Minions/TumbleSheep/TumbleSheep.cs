using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.TumbleSheep
{

	public class TumbleSheepMinionBuff : MinionBuff
	{
		public TumbleSheepMinionBuff() : base(ProjectileType<TumbleSheepMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("TumbleSheep");
			Description.SetDefault("A slime miner will fight for you!");
		}
	}

	public class TumbleSheepMinionItem : MinionItem<TumbleSheepMinionBuff, TumbleSheepMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("TumbleSheep Staff");
			Tooltip.SetDefault("Summons slime miner to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 10;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 5, 0);
			item.rare = ItemRarityID.White;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Minecart, 1);
			recipe.AddIngredient(ItemID.MiningHelmet, 1);
			recipe.AddRecipeGroup("AmuletOfManyMinions:Silvers", 12);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class TumbleSheepMinion : SimpleGroundBasedMinion
	{
		protected override int BuffId => BuffType<TumbleSheepMinionBuff>();
		int lastFiredFrame = 0;
		int walkCycleFrame = 0;
		// don't get too close
		int preferredDistanceFromTarget = 128;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.TumbleSheep"));
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 32;
			attackFrames = 30;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			searchDistance = 900;
			maxJumpVelocity = 12;
		}

		protected override void IdleFlyingMovement(Vector2 vector)
		{
			if(animationFrame - lastFiredFrame < 10)
			{
				// don't fly while throwing the spear
				gHelper.didJustLand = false;
				gHelper.isFlying = false;
				gHelper.ApplyGravity();
			} else
			{
				base.IdleFlyingMovement(vector);
			}
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 11;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && targetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		// If your name is AG...
		private void DoBounce()
		{
			lastFiredFrame = animationFrame;
			Main.PlaySound(new LegacySoundStyle(2, 17), projectile.position);
			// TODO
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if (Math.Abs(vectorToTargetPosition.X) < 4 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 4 * preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= attackFrames)
			{
				DoBounce();
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.X) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X = 0;
			}
			if(Math.Abs(vectorToTargetPosition.Y) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.Y) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			projectile.friendly = false;
		}

		private float WalkCycleAngle => Math.Sign(projectile.velocity.X) * MathHelper.TwoPi * (walkCycleFrame % 60) / 60;
		private bool IsWalking => Math.Abs(projectile.velocity.X) > 1;

		private void DrawLegs(SpriteBatch spriteBatch, Color lightColor)
		{
			for(int i = 0; i < 2; i++)
			{
				Vector2 legRotationAngle = (WalkCycleAngle + i * MathHelper.Pi).ToRotationVector2();
				legRotationAngle.X *= 4;
				legRotationAngle.Y *= 1.5f;
				Texture2D texture = GetTexture(Texture + "_Legs" + (i+1));
				float r = 0;
				Vector2 pos = projectile.Center + legRotationAngle + new Vector2(0, 12);
				SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
				Rectangle bounds = new Rectangle(0, 0, texture.Width, texture.Height);
				Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
				spriteBatch.Draw(texture, pos - Main.screenPosition, bounds, lightColor, r, origin, 1, effects, 0);
			}
		}

		private void DrawBody(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);
			float r = IsWalking ? -MathHelper.Pi/64 + MathHelper.Pi/32 * (float)Math.Sin(WalkCycleAngle) : 0;
			float bobAngle = IsWalking ? 2 * WalkCycleAngle : MathHelper.TwoPi * animationFrame/ 90 + MathHelper.Pi/2;
			float bobAmplitude = IsWalking ? 2 : 1;
			Vector2 pos = projectile.Center + (-2  + bobAmplitude * (float)Math.Sin(bobAngle)) * Vector2.UnitY;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle bounds = new Rectangle(0, 0, texture.Width, texture.Height);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition, bounds, lightColor, r, origin, 1, effects, 0);
		}

		private void DrawHead(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture + "_Head");
			float r = 0;
			float bobAngle = IsWalking ? 2 * WalkCycleAngle : MathHelper.TwoPi * animationFrame / 90;
			float bobAmplitude = IsWalking ? 2 : 1;
			Vector2 pos = projectile.Center + Vector2.UnitX * projectile.spriteDirection * 10 + bobAmplitude * (float) Math.Sin(bobAngle) * Vector2.UnitY;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle bounds = new Rectangle(0, 0, texture.Width, texture.Height);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition, bounds, lightColor, r, origin, 1, effects, 0);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			DrawLegs(spriteBatch, lightColor);
			DrawBody(spriteBatch, lightColor);
			DrawHead(spriteBatch, lightColor);
			return false;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			projectile.rotation = 0;
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				projectile.spriteDirection = -Math.Sign(target.X);
			} else if (projectile.velocity.X > 1)
			{
				projectile.spriteDirection = 1;
			} else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = -1;
			}
			if(Math.Abs(projectile.velocity.X) > 1)
			{
				walkCycleFrame++;
			} else
			{
				walkCycleFrame = 0;
			}
		}
	}
}
