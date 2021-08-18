using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.Slimecart
{
	public class SlimecartMinionBuff : MinionBuff
	{
		public SlimecartMinionBuff() : base(ProjectileType<SlimecartMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Slimecart");
			Description.SetDefault("A slime miner will fight for you!");
		}
	}

	public class SlimecartMinionItem : MinionItem<SlimecartMinionBuff, SlimecartMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimecart Staff");
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

	public class SlimecartMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<SlimecartMinionBuff>();

		internal override WaypointMovementStyle waypointMovementStyle => WaypointMovementStyle.TARGET;
		private int slimeIndex;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimecart");
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 28;
			projectile.localNPCHitCooldown = 20;
			drawOffsetX = -2;
			drawOriginOffsetY = 2;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			float brightness = (lightColor.R + lightColor.G + lightColor.B) / (3f * 255f);
			if (gHelper.isFlying)
			{
				texture = GetTexture(Texture + "_Umbrella");
				spriteBatch.Draw(texture, pos + new Vector2(0, -36) - Main.screenPosition,
					texture.Bounds, lightColor, 0,
					texture.Bounds.Center.ToVector2(), 1, effects, 0);
			}
			texture = GetTexture(Texture + "_Slime");
			int frameHeight = texture.Height / 7;
			Rectangle bounds = new Rectangle(0, slimeIndex * frameHeight, texture.Width, frameHeight);
			spriteBatch.Draw(texture, pos + new Vector2(0, -14) - Main.screenPosition,
				bounds, lightColor, 0,
				new Vector2(bounds.Width/2, bounds.Height/2), 1, effects, 0);
			if(!PartyHatSystem.IsParty)
			{
				texture = GetTexture(Texture + "_Hat");
				spriteBatch.Draw(texture, pos + new Vector2(0, -23) - Main.screenPosition,
					texture.Bounds, lightColor, 0,
					texture.Bounds.Center.ToVector2(), 1, effects, 0);
			}
			return true;
		}

		public override void OnSpawn()
		{
			slimeIndex = player.GetModPlayer<MinionSpawningItemPlayer>().GetNextColorIndex() % 7;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			if (vector.Y < -projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 8;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if (animationFrame - lastHitFrame > 15)
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
			if (gHelper.didJustLand)
			{
				projectile.rotation = 0;
			}
			else
			{
				projectile.rotation = -projectile.spriteDirection * MathHelper.Pi / 8;
			}
			if (Math.Abs(projectile.velocity.X) < 1)
			{
				return;
			}
			base.Animate(minFrame, maxFrame);
			if (gHelper.didJustLand && Math.Abs(projectile.velocity.X) > 4 && animationFrame % 5 == 0)
			{
				Vector2 pos = projectile.Bottom;
				pos.Y -= 4;
				int idx = Dust.NewDust(pos, 8, 8, 16, -projectile.velocity.X / 2, 0, newColor: Color.Coral);
				Main.dust[idx].scale = .8f;
				Main.dust[idx].alpha = 112;
			}
		}
	}
}
