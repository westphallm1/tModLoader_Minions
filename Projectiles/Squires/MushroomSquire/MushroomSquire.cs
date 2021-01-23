using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.MushroomSquire
{
	public class MushroomSquireMinionBuff : MinionBuff
	{
		public MushroomSquireMinionBuff() : base(ProjectileType<MushroomSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Mushroom Squire");
			Description.SetDefault("A mushroom squire will follow your orders!");
		}
	}

	public class MushroomSquireMinionItem : SquireMinionItem<MushroomSquireMinionBuff, MushroomSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Forest");
			Tooltip.SetDefault("Summons a squire\nA mushroom squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 2f;
			item.width = 24;
			item.height = 38;
			item.damage = 9;
			item.value = Item.sellPrice(0, 0, 0, 75);
			item.rare = ItemRarityID.White;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Wood, 18);
			recipe.AddIngredient(ItemID.Mushroom, 8);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}


	public class MushroomSquireMinion : WeaponHoldingSquire
	{
		protected override int BuffId => BuffType<MushroomSquireMinionBuff>();
		protected override int AttackFrames => 20;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/LeafWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/MushroomSquire/MushroomSquireSword";

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);
		public MushroomSquireMinion() : base(ItemType<MushroomSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mushroom Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 30;
		}

		public override float MaxDistanceFromPlayer() => 120;
		protected override float WeaponDistanceFromCenter() => 30;

	}
}
