using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CobaltStar
{
	public class CobaltStarMinionBuff : MinionBuff
	{
		public CobaltStarMinionBuff() : base(ProjectileType<CobaltStarMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Cobalt Star");
			Description.SetDefault("You know you make me feel like, a Cobalt Star...");
		}
	}

	public class CobaltStarMinionItem : MinionItem<CobaltStarMinionBuff, CobaltStarMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Cobalt Star Staff");
			Tooltip.SetDefault("Summons a pair of cobalt throwing stars to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 26;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CobaltBar, 12);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}


	// Uses ai[1] to check when to start colliding with tiles
	public class CobaltFallingStarProjectile : ModProjectile, ISpinningBladeMinion
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/CobaltStar/CobaltStarMinion";

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 60;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 60; // only hit an enemy once
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return true;
		}

		public override void AI()
		{
			if(projectile.position.Y >= projectile.ai[1])
			{
				projectile.tileCollide = true;
			}
			projectile.rotation += 0.1f;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			SpinningBladeDrawer.DrawBlade(this, spriteBatch, lightColor, projectile.rotation);
			return false;
		}
	}

	public class CobaltStarMinion : SpinningBladeMinion<CobaltStarMinionBuff>, ISpinningBladeMinion
	{
		protected override int bladeType => ProjectileType<CobaltFallingStarProjectile>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Cobalt Star");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			attackFrames = 60;
			idleInertia = 8;
			SpinAnimationLength = 30;
			SpinTravelLength = 0;
		}

		protected override void DoDrift(Vector2 driftVelocity)
		{
			DistanceFromGroup(ref driftVelocity);
			projectile.velocity = driftVelocity;
		}

		protected override void DoSpin(Vector2 spinVelocity)
		{
			projectile.velocity = spinVelocity;
		}

		protected override void StopSpin()
		{
			// no op
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			projectile.friendly = false; // only summoned projectiles can deal damage
		}

		protected override void SummonSecondBlade(Vector2 vectorToTargetPosition)
		{
			npcVelocity = Main.npc[(int)targetNPCIndex].velocity;
			Vector2 npcCenter = Main.npc[(int)targetNPCIndex].Center;
			float incoingAngle = MathHelper.PiOver2 + Main.rand.NextFloat(MathHelper.Pi / 4) - MathHelper.PiOver2 / 8;
			Vector2 angleVector = incoingAngle.ToRotationVector2();
			Vector2 launchPosition = npcCenter + -128 * angleVector;
			Vector2 launchVelocity = SpinVelocity * angleVector + new Vector2(npcVelocity.X, 0);
			if (Main.myPlayer == player.whoAmI)
			{
				Projectile.NewProjectile(
					launchPosition,
					launchVelocity,
					ProjectileType<CobaltFallingStarProjectile>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI,
					ai1: projectile.Center.Y + vectorToTargetPosition.Y);
			}
		}
	}
}
