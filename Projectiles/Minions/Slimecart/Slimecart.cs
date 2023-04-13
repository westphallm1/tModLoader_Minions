using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static AmuletOfManyMinions.CrossModClient.SummonersShine.CrossModSetup;

namespace AmuletOfManyMinions.Projectiles.Minions.Slimecart
{
	public class SlimecartMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SlimecartMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Slimecart");
			// Description.SetDefault("A slime miner will fight for you!");
		}
	}

	public class SlimecartMinionItem : MinionItem<SlimecartMinionBuff, SlimecartMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Slimecart Staff");
			// Tooltip.SetDefault("Summons slime miner to fight for you!");
		}
		public override void ApplyCrossModChanges()
		{
			WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, SummonersShineDefaultSpecialWhitelistType.MELEE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 10;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.value = Item.buyPrice(0, 0, 5, 0);
			Item.rare = ItemRarityID.White;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Minecart, 1).AddIngredient(ItemID.MiningHelmet, 1).AddRecipeGroup("AmuletOfManyMinions:Silvers", 12).AddTile(TileID.Anvils).Register();
		}
	}

	public class SlimecartMinion : SimpleGroundBasedMinion
	{
		public override int BuffId => BuffType<SlimecartMinionBuff>();

		public override WaypointMovementStyle WaypointMovementStyle => WaypointMovementStyle.TARGET;
		private int slimeIndex;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Slimecart");
			Main.projFrames[Projectile.type] = 4;
		}
		public override void LoadAssets()
		{
			AddTexture(Texture + "_Umbrella");
			AddTexture(Texture + "_Slime");
			AddTexture(Texture + "_Hat");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 28;
			Projectile.localNPCHitCooldown = 20;
			DrawOffsetX = -2;
			DrawOriginOffsetY = 2;
			attackFrames = 60;
			NoLOSPursuitTime = 300;
			StartFlyingHeight = 96;
			StartFlyingDist = 64;
			DefaultJumpVelocity = 4;
			MaxJumpVelocity = 12;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			float brightness = (lightColor.R + lightColor.G + lightColor.B) / (3f * 255f);
			if (GHelper.isFlying)
			{
				texture = ExtraTextures[0].Value;
				Main.EntitySpriteDraw(texture, pos + new Vector2(0, -36) - Main.screenPosition,
					texture.Bounds, lightColor, 0,
					texture.Bounds.Center.ToVector2(), 1, effects, 0);
			}
			texture = ExtraTextures[1].Value;
			int frameHeight = texture.Height / 7;
			Rectangle bounds = new Rectangle(0, slimeIndex * frameHeight, texture.Width, frameHeight);
			Main.EntitySpriteDraw(texture, pos + new Vector2(0, -14) - Main.screenPosition,
				bounds, lightColor, 0,
				new Vector2(bounds.Width/2, bounds.Height/2), 1, effects, 0);
			texture = ExtraTextures[2].Value;
			Main.EntitySpriteDraw(texture, pos + new Vector2(0, -23) - Main.screenPosition,
				texture.Bounds, lightColor, 0,
				texture.Bounds.Center.ToVector2(), 1, effects, 0);
			return true;
		}

		public override void OnSpawn()
		{
			slimeIndex = Player.GetModPlayer<MinionSpawningItemPlayer>().GetNextColorIndex() % 7;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			if (vector.Y < -Projectile.height && Math.Abs(vector.X) < StartFlyingHeight)
			{
				GHelper.DoJump(vector);
			}
			float xInertia = GHelper.stuckInfo.overLedge && !GHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 8;
			if (VectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = Player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if (AnimationFrame - LastHitFrame > 15)
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
			else
			{
				Projectile.velocity.X = Math.Sign(Projectile.velocity.X) * xMaxSpeed * 0.75f;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (GHelper.didJustLand)
			{
				Projectile.rotation = 0;
			}
			else
			{
				Projectile.rotation = -Projectile.spriteDirection * MathHelper.Pi / 8;
			}
			if (Math.Abs(Projectile.velocity.X) < 1)
			{
				return;
			}
			base.Animate(minFrame, maxFrame);
			if (GHelper.didJustLand && Math.Abs(Projectile.velocity.X) > 4 && AnimationFrame % 5 == 0)
			{
				Vector2 pos = Projectile.Bottom;
				pos.Y -= 4;
				int idx = Dust.NewDust(pos, 8, 8, 16, -Projectile.velocity.X / 2, 0, newColor: Color.Coral);
				Main.dust[idx].scale = .8f;
				Main.dust[idx].alpha = 112;
			}
		}
	}
}
