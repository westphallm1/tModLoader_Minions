using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;

namespace AmuletOfManyMinions.Projectiles.Squires.Squeyere
{
	public class SqueyereMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SqueyereMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Squeyere");
			Description.SetDefault("A Squeyere will follow your orders!");
		}
	}

	public class SqueyereMinionItem : SquireMinionItem<SqueyereMinionBuff, SqueyereMinion>
	{
		protected override string SpecialName => "Seeing Double";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Eyes");
			Tooltip.SetDefault("Summons a squire\nA Squeyere will fight for you!\nClick and hold to guide its attacks\n" +
				"'Squ-Eye-Re. Get it?'");
		}
		
		public override void ApplyCrossModChanges()
		{
			CrossMod.WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, CrossMod.SummonersShineDefaultSpecialWhitelistType.RANGEDNOINSTASTRIKE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 6f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 70;
			Item.value = Item.sellPrice(0, 4, 0, 0);
			Item.rare = ItemRarityID.Pink;
		}
	}

	public abstract class SquireLaser : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 60;
			//Projectile.minion = true; //Bandaid fix? //TODO 1.4
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 12;
			Projectile.height = 12;
		}

		public virtual Color lightColor => Color.Green;

		public override void AI()
		{
			//This caused the projectile to render at a wrong rotation for a single frame, leaving it here 'cause i don't know if this was important, i just moved it to the predraw override.
			//projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Lighting.AddLight(Projectile.position, this.lightColor.ToVector3());
			base.AI();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// manually draw at 2x scale with transparency
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 128);
			float r = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.velocity.X < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 1.5f, effects, 0);
			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, 16, 16);
			return true;
		}

	}

	public class SqueyereEyeLaser : SquireLaser
	{
		public override Color lightColor => Color.Red;
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MiniRetinaLaser;
	}

	public class SqueyereLaser : SquireLaser
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GreenLaser;
	}

	// uses ai[0] for relative position
	public class SqueyereEyeMinion : SquireAccessoryMinion
	{
		protected override bool IsEquipped(SquireModPlayer player) => player.HasSquire() && 
			player.GetSquire().type == ProjectileType<SqueyereMinion>();
		private static int AnimationFrames = 80;

		private int attackRate => (int)Math.Max(30, 60f / player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier);

		private int shootOnFrame => Projectile.ai[0] == 0 ? 0 : 10;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 18;
			Projectile.height = 18;
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
				angleVector *= 24f;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					angleVector,
					ProjectileType<SqueyereEyeLaser>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
			}
		}
	}

	public class SqueyereMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<SqueyereMinionBuff>();
		protected override int ItemType => ItemType<SqueyereMinionItem>();
		protected override int AttackFrames => 60;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/DemonWings";
		protected override string WeaponTexturePath => null;

		protected override float IdleDistanceMulitplier => 2.5f;
		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		protected override Vector2 WingOffset => new Vector2(-4, 0);
		protected override bool travelRangeCanBeModified => false;

		//unfortunately just flipping the the direction doesn't look great for this one
		protected override Vector2 WeaponCenterOfRotation => Projectile.spriteDirection == 1 ? new Vector2(4, -6) : new Vector2(8, -6);

		protected override float projectileVelocity => 24f;

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Ancient Cobalt Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 20;
			Projectile.height = 32;
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
					weaponCenter.X *= Projectile.spriteDirection;
					Vector2 tipCenter = Projectile.Center + weaponCenter;
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						tipCenter,
						angleVector,
						ProjectileType<SqueyereLaser>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer);
				}

				SoundEngine.PlaySound(SoundID.Item33 with { Volume = 0.5f }, Projectile.Center); //Why is it so LOUD?!
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
						ProjectileType<SqueyereEyeMinion>(),
						Projectile.damage,
						Projectile.knockBack,
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
