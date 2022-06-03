using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;

namespace AmuletOfManyMinions.Projectiles.Squires.DemonSquire
{
	public class DemonSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<DemonSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Demon Squire");
			Description.SetDefault("A demonic squire will follow your orders!");
		}
	}

	public class DemonSquireMinionItem : SquireMinionItem<DemonSquireMinionBuff, DemonSquireMinion>
	{
		protected override string SpecialName => "Imp Assistants";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of the Underworld");
			Tooltip.SetDefault("Summons a squire\nA demon squire will fight for you!\nClick and hold to guide its attacks");
		}
		
		public override void ApplyCrossModChanges()
		{
			CrossMod.SummonersShineMinionPowerCollection minionCollection = new CrossMod.SummonersShineMinionPowerCollection();
			minionCollection.AddMinionPower(1.5f);
			CrossMod.BakeSummonersShineMinionPower_NoHooks(Item.type, minionCollection);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 7.0f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 32;
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.Orange;
		}
	}

	public class DemonSquireImpFireball : BaseImpFireball
	{
	}

	// uses ai[0] for relative position
	public class DemonSquireImpMinion : SquireAccessoryMinion
	{
		protected override bool IsEquipped(SquireModPlayer player) => player.HasSquire() && 
			player.GetSquire().type == ProjectileType<DemonSquireMinion>();
		private static int AnimationFrames = 80;

		private int attackRate => (int)Math.Max(30, 60f / player.GetModPlayer<SquireModPlayer>().SquireAttackSpeedMultiplier);

		private int shootOnFrame => Projectile.ai[0] == 0 ? 0 : 10;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 42;
			Projectile.height = 34;
			frameSpeed = 10;
		}

		public override Vector2 IdleBehavior()
		{
			int angleFrame = animationFrame % AnimationFrames;
			float baseAngle = Projectile.ai[0] == 0 ? 0 : MathHelper.Pi;
			float angle = baseAngle + (MathHelper.TwoPi * angleFrame) / AnimationFrames;
			float radius = 24;
			Vector2 angleVector = radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			SquireModPlayer modPlayer = player.GetModPlayer<SquireModPlayer>();
			if(modPlayer.HasSquire())
			{
				Projectile.spriteDirection = modPlayer.GetSquire().spriteDirection;
			}
			// offset downward vertically a bit
			// the scale messes with the positioning in some way
			return base.IdleBehavior() + angleVector;
		}
		public override Vector2? FindTarget()
		{
			if (animationFrame % attackRate == shootOnFrame && SquireAttacking())
			{
				return Vector2.One; // a bit hacky, doesn't actually attack along this vector
			}
			return null;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 20, 20, DustID.Blood);
			}
		}

		public override void Kill(int timeLeft)
		{
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 20, 20, DustID.Blood);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.IdleMovement(vectorToIdle);
			if (animationFrame % attackRate == shootOnFrame && Main.myPlayer == player.whoAmI)
			{
				SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
				Projectile squire = squirePlayer.GetSquire();
				// attack "towards the horizon" along the squire-mouse line
				Vector2 horizonVector;
				if (Vector2.DistanceSquared(squire.Center, Main.MouseWorld) < 48 * 48)
				{
					Vector2 horizonAngle = Main.MouseWorld - player.Center;
					horizonAngle.SafeNormalize();
					horizonVector = player.Center + 2000f * horizonAngle;
				} else
				{
					Vector2 horizonAngle = Main.MouseWorld - squire.Center;
					horizonAngle.SafeNormalize();
					horizonVector = squire.Center + 2000f * horizonAngle;
				}
				Vector2 angleVector = horizonVector - Projectile.Center;
				angleVector.SafeNormalize();
				angleVector *= CrossMod.ApplyCrossModScaling(24f, Projectile, 0);
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					angleVector,
					ProjectileType<DemonSquireImpFireball>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
			}
		}
	}


	public class DemonSquireUnholyTrident: BaseMinionUnholyTrident
	{
		private Vector2 baseVelocity;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = 30;
		}
		public override void AI()
		{
			base.AI();
			Projectile parent = Main.projectile[(int)Projectile.ai[0]];
			if (baseVelocity == default)
			{
				baseVelocity = Projectile.velocity;
			}
			if (parent.active)
			{
				Projectile.velocity = parent.velocity + baseVelocity;
			}
			Projectile.rotation = baseVelocity.ToRotation() + MathHelper.Pi/4;
		}
	}


	public class DemonSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<DemonSquireMinionBuff>();
		protected override int ItemType => ItemType<DemonSquireMinionItem>();
		protected override int AttackFrames => 25;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => null;

		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override float knockbackSelf => 5f;
		protected override Vector2 WingOffset => new Vector2(-4, 2);

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;

		protected override float projectileVelocity => 12;

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 30;
			DrawOriginOffsetY = -8;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Demon Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0 && Main.myPlayer == player.whoAmI)
			{
				Vector2 vector2Mouse = UnitVectorFromWeaponAngle();
				vector2Mouse *= ModifiedProjectileVelocity();
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(), 
					Projectile.Center,
					vector2Mouse,
					ProjectileType<DemonSquireUnholyTrident>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer,
					ai0: Projectile.whoAmI);
			}
		}

		public override void OnStartUsingSpecial()
		{
			if(player.whoAmI == Main.myPlayer)
			{
				for(int i = 0; i < 2; i++)
				{
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						Vector2.Zero,
						ProjectileType<DemonSquireImpMinion>(),
						Projectile.damage,
						Projectile.knockBack,
						player.whoAmI,
						ai0: i);
				}
			}
		}

		public override void OnStopUsingSpecial()
		{
			int projType = ProjectileType<DemonSquireImpMinion>();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.owner == player.whoAmI && p.type == projType)
				{
					p.Kill();
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		protected override float WeaponDistanceFromCenter() => 12;

		public override float ComputeIdleSpeed() => 11;

		public override float ComputeTargetedSpeed() => 11;

		public override float MaxDistanceFromPlayer() => 232;
	}
}
