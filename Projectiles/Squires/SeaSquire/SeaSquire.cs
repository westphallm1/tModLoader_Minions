using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.SeaSquire
{
	public class SeaSquireMinionBuff : MinionBuff
	{
		public SeaSquireMinionBuff() : base(ProjectileType<SeaSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Sea Squire");
			Description.SetDefault("An flying fish will follow your fancies!");
		}
	}

	public class SeaSquireMinionItem : SquireMinionItem<SeaSquireMinionBuff, SeaSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Sea");
			Tooltip.SetDefault("Summons a squire\nA flying fish squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 2.5f;
			item.width = 28;
			item.height = 32;
			item.damage = 14;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			//ModRecipe recipe = new ModRecipe(mod);
			//recipe.AddIngredient(ItemID.Trident, 1);
			//recipe.AddIngredient(ItemID.Coral, 3);
			//recipe.AddIngredient(ItemID.Starfish, 3);
			//recipe.AddTile(TileID.Anvils);
			//recipe.SetResult(this);
			//recipe.AddRecipe();
		}
	}

	public class SeaSquireMinion : WeaponHoldingSquire<SeaSquireMinionBuff>
	{
		protected override int AttackFrames => 35;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

		protected override string WeaponTexturePath => "Terraria/Item_" + ItemID.Trident;

		protected override Vector2 WingOffset => new Vector2(-4, 2);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);
		public SeaSquireMinion() : base(ItemType<SeaSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Sea Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 30;
			projectile.height = 32;
		}

		protected override float WeaponDistanceFromCenter()
		{
			float spearSpeed = 3.25f;
			int reachFrames = 20;
			int spearStart = 20;

			if (attackFrame <= reachFrames)
			{
				return spearSpeed * attackFrame - spearStart;
			}
			else
			{
				return (spearSpeed * reachFrames - spearStart) - spearSpeed * (attackFrame - reachFrames);
			}
		}

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() + 5;

		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 25;

		public override float MaxDistanceFromPlayer() => 160;

		public override float ComputeTargetedSpeed() => 8;

		public override float ComputeIdleSpeed() => 8;
	}
}
