using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.Squeyere
{
	public class SqueyereMinionBuff : MinionBuff
	{
		public SqueyereMinionBuff() : base(ProjectileType<SqueyereMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Squeyere");
			Description.SetDefault("A Squeyere will follow your orders!");
		}
	}

	public class SqueyereMinionItem : SquireMinionItem<SqueyereMinionBuff, SqueyereMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Eyes");
			Tooltip.SetDefault("Summons a squire\nA Squeyere will fight for you!\nClick and hold to guide its attacks\n" +
				"'Squ-Eye-Re. Get it?'");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 6f;
			item.width = 24;
			item.height = 38;
			item.damage = 70;
			item.value = Item.sellPrice(0, 4, 0, 0);
			item.rare = ItemRarityID.Pink;
		}
	}

	public abstract class SquireLaser : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.friendly = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 60;
			projectile.minion = true; //Bandaid fix?
			projectile.width = 12;
			projectile.height = 12;
		}

		public virtual Color lightColor => Color.Green;

		public override void AI()
		{
			//This caused the projectile to render at a wrong rotation for a single frame, leaving it here 'cause i don't know if this was important, i just moved it to the predraw override.
			//projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Lighting.AddLight(projectile.position, this.lightColor.ToVector3());
			base.AI();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// manually draw at 2x scale with transparency
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1.5f, effects, 0);
			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Main.PlaySound(SoundID.Item10, (int)projectile.position.X, (int)projectile.position.Y);
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, 16, 16);
			return true;
		}

	}

	public class SqueyereEyeLaser : SquireLaser
	{
		public override Color lightColor => Color.Red;
		public override string Texture => "Terraria/Projectile_" + ProjectileID.MiniRetinaLaser;
	}

	public class SqueyereLaser : SquireLaser
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.GreenLaser;
	}

	// uses ai[0] for relative position
	public class SqueyereEyeMinion : SquireAccessoryMinion
	{
		protected override bool IsEquipped(SquireModPlayer player) => player.HasSquire() && 
			player.GetSquire().type == ProjectileType<SqueyereMinion>();
		private static int AnimationFrames = 80;

		private int attackRate => (int)Math.Max(30, 60f / player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier);

		private int shootOnFrame => projectile.ai[0] == 0 ? 0 : 10;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 18;
			projectile.height = 18;
			frameSpeed = 10;
		}

		public override Vector2 IdleBehavior()
		{
			int angleFrame = animationFrame % AnimationFrames;
			float baseAngle = projectile.ai[0] == 0 ? 0 : MathHelper.Pi;
			float angle = baseAngle + (MathHelper.TwoPi * angleFrame) / AnimationFrames;
			float radius = 24;
			Vector2 angleVector = radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			SquireModPlayer modPlayer = player.GetModPlayer<SquireModPlayer>();
			if(modPlayer.HasSquire())
			{
				projectile.spriteDirection = modPlayer.GetSquire().spriteDirection;
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
				Dust.NewDust(projectile.position, 20, 20, DustID.Blood);
			}
		}

		public override void Kill(int timeLeft)
		{
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(projectile.position, 20, 20, DustID.Blood);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.IdleMovement(vectorToIdle);
			if (animationFrame % attackRate == shootOnFrame && Main.myPlayer == player.whoAmI)
			{
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
				Vector2 angleVector = horizonVector - projectile.Center;
				angleVector.SafeNormalize();
				angleVector *= 24f;
				Projectile.NewProjectile(
					projectile.Center,
					angleVector,
					ProjectileType<SqueyereEyeLaser>(),
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer);
			}
		}
	}

	public class SqueyereMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<SqueyereMinionBuff>();
		protected override int AttackFrames => 60;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => null;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override Vector2 WingOffset => new Vector2(-4, 0);
		protected override bool travelRangeCanBeModified => false;

		//unfortunately just flipping the the direction doesn't look great for this one
		protected override Vector2 WeaponCenterOfRotation => projectile.spriteDirection == 1 ? new Vector2(4, -6) : new Vector2(8, -6);

		protected override float projectileVelocity => 24f;

		protected override int SpecialDuration => 8 * 60;
		protected override int SpecialCooldown => 12 * 60;

		public SqueyereMinion() : base(ItemType<SqueyereMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 20;
			projectile.height = 32;
		}


		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0 || attackFrame == 10 || attackFrame == 20)
			{
				if (Main.myPlayer == player.whoAmI)
				{
					Vector2 angleVector = UnitVectorFromWeaponAngle();
					angleVector *= ModifiedProjectileVelocity();
					Vector2 weaponCenter = WeaponCenterOfRotation;
					weaponCenter.X *= projectile.spriteDirection;
					Vector2 tipCenter = projectile.Center + weaponCenter;
					Projectile.NewProjectile(
						tipCenter,
						angleVector,
						ProjectileType<SqueyereLaser>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}

				Main.PlaySound(SoundID.Item33.WithVolume(.5f), projectile.Center); //Why is it so LOUD?!
			}
		}

		public override void OnStartUsingSpecial()
		{
			if(player.whoAmI == Main.myPlayer)
			{
				for(int i = 0; i < 2; i++)
				{
					Projectile.NewProjectile(
						projectile.Center,
						Vector2.Zero,
						ProjectileType<SqueyereEyeMinion>(),
						projectile.damage,
						projectile.knockBack,
						player.whoAmI,
						ai0: i);
				}
			}
		}

		public override void OnStopUsingSpecial()
		{
			int projType = ProjectileType<SqueyereEyeMinion>();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.owner == player.whoAmI && p.type == projType)
				{
					p.Kill();
				}
			}
		}


		protected override float WeaponDistanceFromCenter() => 6;

		protected override int WeaponHitboxEnd() => 6;

		public override float ComputeIdleSpeed() => 14;

		public override float ComputeTargetedSpeed() => 14;

		public override float MaxDistanceFromPlayer() => 60;
	}
}
