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
		internal override int[] ProjectileTypes => new int[] { ProjectileType<CrimsonSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
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
			CrossMod.SummonersShineMinionPowerCollection minionCollection = new CrossMod.SummonersShineMinionPowerCollection();
			minionCollection.AddMinionPower(5);
			CrossMod.BakeSummonersShineMinionPower_NoHooks(Item.type, minionCollection);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 5f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 28;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.CrimtaneBar, 12).AddIngredient(ItemID.TissueSample, 6).AddTile(TileID.Anvils).Register();
		}
	}

	public class EvilSquireExplosion : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_0";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 30;
		}

		public override bool PreDraw(ref Color lightColor)
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
			target.AddBuff((int)Projectile.ai[0], (int)Projectile.ai[1]);
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
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
			CrossMod.SetSummonersShineProjMaxEnergy(Projectile.type, 0);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.timeLeft = TimeToLive;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			//Projectile.minion = true; //TODO 1.4
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void AI()
		{
			base.AI();
			if(Projectile.timeLeft < TimeLeftToStartFalling && Projectile.velocity.Y < 16)
			{
				Projectile.velocity.Y += 0.5f;
				Projectile.velocity.X *= 0.99f;
			}
			Projectile.rotation += MathHelper.Pi / 16 * Math.Sign(Projectile.velocity.X);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = 0; // subsequent explosion does the damage
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(new LegacySoundStyle(2, 107), Projectile.Center);
			Vector2 position = Projectile.Center;
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
			var source = Projectile.GetSource_Death();
			foreach (Vector2 offset in new Vector2[] { Vector2.One, -Vector2.One, new Vector2(1, -1), new Vector2(-1, 1) })
			{
				int goreIdx = Gore.NewGore(source, position, default, Main.rand.Next(61, 64));
				Main.gore[goreIdx].velocity *= 0.25f;
				Main.gore[goreIdx].velocity += offset;
			}
			if(Projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					ProjectileType<EvilSquireExplosion>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner,
					ai0: BuffId,
					ai1: BuffDuration);
			}
		}
	}

	public class IchorFlaskProjectile : EvilSquireFlask
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.FlaskofIchor;
		protected override int DustId => 87;
		protected override int BuffId => BuffID.Ichor;
		protected override int BuffDuration => Projectile.originalDamage;
	}

	public class CrimsonSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<CrimsonSquireMinionBuff>();
		protected override int ItemType => ItemType<CrimsonSquireMinionItem>();
		protected override int AttackFrames => 30;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => "Terraria/Images/Item_" + ItemID.BloodLustCluster;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 0);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override float projectileVelocity => 12;

		protected override LegacySoundStyle SpecialStartSound => new LegacySoundStyle(2, 106);

		protected override int SpecialCooldown => 8 * 60;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crimson Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
			CrossMod.SetSummonersShineProjMaxEnergy(Projectile.type, 0);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 32;
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.SpecialTargetedMovement(vectorToTargetPosition);
			if(specialFrame == 1 && player.whoAmI == Main.myPlayer)
			{
				Vector2 vector2Mouse = Vector2.DistanceSquared(Projectile.Center, Main.MouseWorld) < 48 * 48 ?
					Main.MouseWorld - player.Center : Main.MouseWorld - Projectile.Center;
				vector2Mouse.SafeNormalize();
				vector2Mouse *= ModifiedProjectileVelocity();
				Projectile proj = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(), 
					Projectile.Center,
					vector2Mouse,
					ProjectileType<IchorFlaskProjectile>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer,
					8);
				proj.originalDamage = (int)(60 * CrossMod.ApplyCrossModScaling(5, Projectile, 0));
			}
		}


		protected override float WeaponDistanceFromCenter() => 30;

		protected override int WeaponHitboxEnd() => 55;

		public override float ComputeIdleSpeed() => 8.5f;

		public override float ComputeTargetedSpeed() => 8.5f;

		public override float MaxDistanceFromPlayer() => 192;
	}
}
