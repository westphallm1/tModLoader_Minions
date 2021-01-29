using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.AdamantiteSquire
{
	public class AdamantiteSquireMinionBuff : MinionBuff
	{
		public AdamantiteSquireMinionBuff() : base(ProjectileType<AdamantiteSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Adamantite Squire");
			Description.SetDefault("An adamantite squire will follow your orders!");
		}
	}

	public class AdamantiteSquireMinionItem : SquireMinionItem<AdamantiteSquireMinionBuff, AdamantiteSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Crest");
			Tooltip.SetDefault("Summons a squire\nAn adamantite squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 5.5f;
			item.width = 24;
			item.height = 38;
			item.damage = 33;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.AdamantiteBar, 14);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class AdamantiteSquireMinion : WeaponHoldingSquire
	{
		protected override int BuffId => BuffType<AdamantiteSquireMinionBuff>();
		protected override int AttackFrames => 25;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/AdamantiteSquire/AdamantiteSquireSword";

		protected override Vector2 WingOffset => new Vector2(0, 6);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);
		public AdamantiteSquireMinion() : base(ItemType<AdamantiteSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Adamantite Squire");
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
			//All of this is based on the weapon sprite and AttackFrames above.
            int reachFrames = AttackFrames / 2; //A spear should spend half the AttackFrames extending, and half retracting by default.
            int spearLength = GetTexture(WeaponTexturePath).Width; //A decent aproximation of how long the spear is.
            int spearStart = (spearLength / 3); //Two thirds of the spear starts behind by default.
            float spearSpeed = spearLength / reachFrames; //A calculation of how quick the spear should be moving.
            if (attackFrame <= reachFrames)
			{
				return spearSpeed * attackFrame - spearStart;
			}
			else
			{
				return (spearSpeed * reachFrames - spearStart) - spearSpeed * (attackFrame - reachFrames);
			}
		}

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() + 35;

		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 45;

		public override float MaxDistanceFromPlayer() => 300;

		public override float ComputeTargetedSpeed() => 12;

		public override float ComputeIdleSpeed() => 12;
	}
}
