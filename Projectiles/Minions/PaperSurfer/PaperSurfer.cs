using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.PaperSurfer
{
	public class PaperSurferMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<PaperSurferMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paper Surfer");
			Description.SetDefault("A paper surfer will fight for you!");
		}
	}

	public class PaperSurferMinionItem : MinionItem<PaperSurferMinionBuff, PaperSurferMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Paper Surfer Staff");
			Tooltip.SetDefault("Summons a paper surfer to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 12;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 38;
			Item.height = 40;
			Item.value = Item.buyPrice(0, 0, 70, 0);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.ClayBlock, 30).AddIngredient(ItemID.Cloud, 30).AddTile(TileID.Anvils).Register();
		}
	}
	public class PaperSurferMinion : SurferMinion
	{
		public override int BuffId => BuffType<PaperSurferMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			diveBombHeightRequirement = 40;
			diveBombHeightTarget = 120;
			diveBombHorizontalRange = 80;
			diveBombFrameRateLimit = 60;
			diveBombSpeed = 12;
			diveBombInertia = 15;
			approachSpeed = 8;
			approachInertia = 40;
			targetSearchDistance = 800;
			bumbleSpriteDirection = -1;
			Projectile.width = 28;
			Projectile.height = 32;
		}
	}
}
