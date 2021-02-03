using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
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
			Description.SetDefault("A flying fish will follow your fancies!");
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
			item.knockBack = 7f;
			item.width = 28;
			item.height = 32;
			item.damage = 10;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Coral, 3);
			recipe.AddIngredient(ItemID.Starfish, 3);
			recipe.AddIngredient(ItemID.Seashell, 3);
			recipe.AddIngredient(ItemID.SharkFin, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class SeaSquireBubble : ModProjectile
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SeaSquire/SeaSquireBubble";
		public override void SetDefaults()
		{
			base.SetDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
			projectile.CloneDefaults(ProjectileID.Bubble);
			projectile.alpha = 240;
			projectile.timeLeft = 180;
			projectile.penetrate = 1;
			projectile.ignoreWater = true;
			projectile.friendly = true;
			projectile.width = 12;
			projectile.height = 12;
			projectile.magic = false; //Bandaid fix
			projectile.minion = true;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Wet, 300);
		}

		public override void Kill(int timeLeft)
		{
			Main.PlaySound(new LegacySoundStyle(2, 54), projectile.position);
			for (int i = 0; i < 8; i++)
			{
				int dustCreated = Dust.NewDust(projectile.Center, 1, 1, 137, projectile.velocity.X, projectile.velocity.Y, 0, Scale: 1.4f);
				Main.dust[dustCreated].noGravity = true;
			}
		}
	}

	public class SeaSquireMinion : WeaponHoldingSquire
	{
		protected override int BuffId => BuffType<SeaSquireMinionBuff>();
		protected override int AttackFrames => 35;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/AngelWings";

		protected override string WeaponTexturePath => "Terraria/Item_" + ItemID.Trident;

		protected override Vector2 WingOffset => new Vector2(-4, 2);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 6);
		protected override float projectileVelocity => 8;
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

		public override void OnSpawn()
		{
			base.OnSpawn();
			// the spear does half damage, this is re-multiplied by 2 to get the bubble damage
			// maybe a little bit iffy
			projectile.damage = projectile.damage / 2;
		}

		protected override float WeaponDistanceFromCenter()
		{
			//All of this is based on the weapon sprite and AttackFrames above.
			int reachFrames = AttackFrames / 2; //A spear should spend half the AttackFrames extending, and half retracting by default.
			int spearLength = GetTexture(WeaponTexturePath).Width; //A decent aproximation of how long the spear is.
			int spearStart = (spearLength / 3 - 10); //Two thirds of the spear starts behind by default. Subtract to start it further out since this one is puny.
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

		public override Vector2 IdleBehavior()
		{
			return base.IdleBehavior();
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float bubbleVelOffset = Main.rand.NextFloat() * 2;
			base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				Main.PlaySound(new LegacySoundStyle(2, 85), projectile.position);
				if (Main.myPlayer == player.whoAmI)
				{
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector *= ModifiedProjectileVelocity() + bubbleVelOffset;
					Projectile.NewProjectile(
						projectile.Center,
						angleVector,
						ProjectileType<SeaSquireBubble>(),
						projectile.damage * 2,
						projectile.knockBack / 4,
						Main.myPlayer);
				}
			}
		}

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() + 15;

		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 25;

		public override float MaxDistanceFromPlayer() => 160;

		public override float ComputeTargetedSpeed() => 8;

		public override float ComputeIdleSpeed() => 8;
	}
}
