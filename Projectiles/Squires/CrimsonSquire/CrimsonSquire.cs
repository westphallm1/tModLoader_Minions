using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
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
		protected override string SpecialName => "Flask of Ichor";
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

	public class EvilSquireExplosion : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_0";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 1;
			projectile.height = 1;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 30;
			projectile.tileCollide = false;
			projectile.timeLeft = 30;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			projHitbox.Inflate(96, 96);
			return projHitbox.Intersects(targetHitbox);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			// pass buff type and duration in via ai
			target.AddBuff((int)projectile.ai[0], (int)projectile.ai[1]);
		}
	}

	public abstract class EvilSquireFlask : ModProjectile
	{
		const int TimeToLive = 180;
		const int TimeLeftToStartFalling = TimeToLive - 20;
		protected abstract int DustId { get; }
		protected abstract int BuffId { get; }
		protected abstract int BuffDuration { get; }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = 1;
			projectile.width = 12;
			projectile.height = 12;
			projectile.timeLeft = TimeToLive;
			projectile.friendly = true;
			projectile.tileCollide = true;
			projectile.minion = true;
		}

		public override void AI()
		{
			base.AI();
			if(projectile.timeLeft < TimeLeftToStartFalling && projectile.velocity.Y < 16)
			{
				projectile.velocity.Y += 0.5f;
				projectile.velocity.X *= 0.99f;
			}
			projectile.rotation += MathHelper.Pi / 16 * Math.Sign(projectile.velocity.X);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = 0; // subsequent explosion does the damage
		}

		public override void Kill(int timeLeft)
		{
			Main.PlaySound(new LegacySoundStyle(2, 107), projectile.Center);
			Vector2 position = projectile.Center;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 10; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, DustId, 0f, 0f, 100, default, 2f);
				Main.dust[dustIdx].noGravity = true;
				Main.dust[dustIdx].velocity *= 3f;
				dustIdx = Dust.NewDust(position, width, height, DustId, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 1.5f;
			}
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				int goreIdx = Gore.NewGore(position, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= 0.25f;
				Main.gore[goreIdx].velocity += offset;
			}
			if(projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(
					projectile.Center,
					Vector2.Zero,
					ProjectileType<EvilSquireExplosion>(),
					projectile.damage,
					projectile.knockBack,
					projectile.owner,
					ai0: BuffId,
					ai1: BuffDuration);
			}
		}
	}

	public class IchorFlaskProjectile : EvilSquireFlask
	{
		public override string Texture => "Terraria/Item_" + ItemID.FlaskofIchor;
		protected override int DustId => 87;
		protected override int BuffId => BuffID.Ichor;
		protected override int BuffDuration => 300;
	}

	public class CrimsonSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<CrimsonSquireMinionBuff>();
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => "Terraria/Item_" + ItemID.BloodLustCluster;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override float projectileVelocity => 12;

		protected override LegacySoundStyle SpecialStartSound => new LegacySoundStyle(2, 106);

		protected override int SpecialCooldown => 8 * 60;
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

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.SpecialTargetedMovement(vectorToTargetPosition);
			if(specialFrame == 1 && player.whoAmI == Main.myPlayer)
			{
				Vector2 vector2Mouse = Vector2.DistanceSquared(projectile.Center, Main.MouseWorld) < 48 * 48 ?
					Main.MouseWorld - player.Center : Main.MouseWorld - projectile.Center;
				vector2Mouse.SafeNormalize();
				vector2Mouse *= ModifiedProjectileVelocity();
				Projectile.NewProjectile(projectile.Center,
					vector2Mouse,
					ProjectileType<IchorFlaskProjectile>(),
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer,
					8);
			}
		}


		protected override float WeaponDistanceFromCenter() => 30;

		protected override int WeaponHitboxEnd() => 55;

		public override float ComputeIdleSpeed() => 8.5f;

		public override float ComputeTargetedSpeed() => 8.5f;

		public override float MaxDistanceFromPlayer() => 192;
	}
}
