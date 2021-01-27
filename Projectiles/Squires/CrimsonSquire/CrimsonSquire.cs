using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.CrimsonSquire
{
	public class CrimsonSquireMinionBuff : MinionBuff
	{
		public CrimsonSquireMinionBuff() : base(ProjectileType<CrimsonSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Crimson Squire");
			Description.SetDefault("A crimson squire will follow your orders!");
		}
	}

	public class CrimsonSquireMinionItem : SquireMinionItem<CrimsonSquireMinionBuff, CrimsonSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Crimson");
			Tooltip.SetDefault("Summons a squire\nA crimson squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 5f;
			item.width = 24;
			item.height = 38;
			item.damage = 28;
			item.value = Item.sellPrice(0, 0, 20, 0);
			item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CrimtaneBar, 12);
			recipe.AddIngredient(ItemID.TissueSample, 6);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}


	public class CrimsonSquireMinion : WeaponHoldingSquire<CrimsonSquireMinionBuff>
	{
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => "Terraria/Item_" + ItemID.BloodLustCluster;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);
		public CrimsonSquireMinion() : base(ItemType<CrimsonSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 22;
			projectile.height = 32;
		}


		protected override float WeaponDistanceFromCenter() => 30;

		protected override int WeaponHitboxEnd() => 55;

		public override float ComputeIdleSpeed() => 8.5f;

		public override float ComputeTargetedSpeed() => 8.5f;

		public override float MaxDistanceFromPlayer() => 192;
	}
}
