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
	//public class CobaltStarMinionBuff : MinionBuff
	//{
	//	public CobaltStarMinionBuff() : base(ProjectileType<CobaltStarMinion>()) { }
	//	public override void SetDefaults()
	//	{
	//		base.SetDefaults();
	//		DisplayName.SetDefault("Cobalt Star");
	//		Description.SetDefault("A cobalt star is fighting for you!");
	//	}
	//}

	////public class CobaltStarMinionItem : MinionItem<CobaltStarMinionBuff, CobaltStarMinion>
	////{
	////	public override void SetStaticDefaults()
	////	{
	////		base.SetStaticDefaults();
	////		DisplayName.SetDefault("Cobalt Star Staff");
	////		Tooltip.SetDefault("Summons a pair of cobalt throwing stars to fight for you!");
	////	}

	////	public override void SetDefaults()
	////	{
	////		base.SetDefaults();
	////		item.damage = 22;
	////		item.knockBack = 0.5f;
	////		item.mana = 10;
	////		item.width = 28;
	////		item.height = 28;
	////		item.value = Item.buyPrice(0, 0, 2, 0);
	////		item.rare = ItemRarityID.Orange;
	////	}
	////	public override void AddRecipes()
	////	{
	////		ModRecipe recipe = new ModRecipe(mod);
	////		recipe.AddIngredient(ItemID.CobaltBar, 12);
	////		recipe.AddTile(TileID.Anvils);
	////		recipe.SetResult(this);
	////		recipe.AddRecipe();
	////	}
	////}


	//// Uses ai[1] to check when to start colliding with tiles
	//public class CobaltFallingStarProjectile : ModProjectile, ISpinningBladeMinion
	//{

	//	public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/CobaltStar/CobaltStarMinion";

	//	public override void SetDefaults()
	//	{
	//		base.SetDefaults();
	//		projectile.width = 16;
	//		projectile.height = 16;
	//		projectile.tileCollide = false;
	//		projectile.friendly = true;
	//		projectile.penetrate = -1;
	//		projectile.timeLeft = 60;
	//		projectile.usesLocalNPCImmunity = true;
	//		projectile.localNPCHitCooldown = 60; // only hit an enemy once
	//		ProjectileID.Sets.MinionShot[projectile.type] = true;
	//	}

	//	public override bool OnTileCollide(Vector2 oldVelocity)
	//	{
	//		return false;
	//	}

	//	public override void AI()
	//	{
	//		if(projectile.timeLeft < projectile.ai[1])
	//		{
	//			projectile.velocity = Vector2.Zero;
	//		}
	//		projectile.rotation += 0.1f;
	//	}

	//	public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
	//	{
	//		SpinningBladeDrawer.DrawBlade(this, spriteBatch, lightColor, projectile.rotation);
	//		return false;
	//	}
	//}

	//public class CobaltStarMinion : SpinningBladeMinion<CobaltStarMinionBuff>, ISpinningBladeMinion
	//{
	//	protected override int bladeType => ProjectileType<CobaltFallingStarProjectile>();

	//	public override void SetStaticDefaults()
	//	{
	//		base.SetStaticDefaults();
	//		DisplayName.SetDefault("Cobalt Star");
	//	}

	//	public override void SetDefaults()
	//	{
	//		base.SetDefaults();
	//		projectile.width = 16;
	//		projectile.height = 16;
	//		attackFrames = 60;
	//		idleInertia = 8;
	//		SpinAnimationLength = 40;
	//	}
	//}
}
