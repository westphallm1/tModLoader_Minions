namespace AmuletOfManyMinions.Projectiles.Minions.PalladiumSaw
{
	//public class PalladiumSawMinionBuff : MinionBuff
	//{
	//	public PalladiumSawMinionBuff() : base(ProjectileType<PalladiumSawMinion>()) { }
	//	public override void SetDefaults()
	//	{
	//		base.SetDefaults();
	//		DisplayName.SetDefault("Palladium Saw");
	//		Description.SetDefault("Let 'er Rip!");
	//	}
	//}

	////public class PalladiumSawMinionItem : MinionItem<PalladiumSawMinionBuff, PalladiumSawMinion>
	////{
	////	public override void SetStaticDefaults()
	////	{
	////		base.SetStaticDefaults();
	////		DisplayName.SetDefault("Palladium Saw Staff");
	////		Tooltip.SetDefault("Summons a spinning palladium saw to fight for you!");
	////	}

	////	public override void SetDefaults()
	////	{
	////		base.SetDefaults();
	////		item.damage = 23;
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
	////		recipe.AddIngredient(ItemID.PalladiumBar, 12);
	////		recipe.AddTile(TileID.Anvils);
	////		recipe.SetResult(this);
	////		recipe.AddRecipe();
	////	}
	////}


	//// Uses ai[1] to check when to stop moving
	//public class PalladiumSawSplitProjectile : ModProjectile, ISpinningBladeMinion
	//{

	//	public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/PalladiumSaw/PalladiumSawMinion";

	//	public override void SetDefaults()
	//	{
	//		base.SetDefaults();
	//		projectile.width = 16;
	//		projectile.height = 16;
	//		projectile.tileCollide = true;
	//		projectile.friendly = true;
	//		projectile.penetrate = -1;
	//		projectile.timeLeft = 10;
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

	//	public override bool PreDraw(ref Color lightColor)
	//	{
	//		SpinningBladeDrawer.DrawBlade(this, spriteBatch, lightColor, projectile.rotation);
	//		return false;
	//	}
	//}

	//public class PalladiumSawMinion : SpinningBladeMinion<PalladiumSawMinionBuff>, ISpinningBladeMinion
	//{
	//	protected override int bladeType => ProjectileType<PalladiumSawSplitProjectile>();

	//	public override void SetStaticDefaults()
	//	{
	//		base.SetStaticDefaults();
	//		DisplayName.SetDefault("Palladium Saw");
	//	}

	//	public override void SetDefaults()
	//	{
	//		base.SetDefaults();
	//		projectile.width = 16;
	//		projectile.height = 16;
	//		attackFrames = 60;
	//		SpinAnimationLength = 50;
	//		idleInertia = 8;
	//	}
	//}
}
