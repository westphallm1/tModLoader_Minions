using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.ArmoredBoneSquire
{
	public class ArmoredBoneSquireMinionBuff : MinionBuff
	{
		public ArmoredBoneSquireMinionBuff() : base(ProjectileType<ArmoredBoneSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Armored Bone Squire");
			Description.SetDefault("An armored bone squire will follow your orders!");
		}
	}

	public class ArmoredBoneSquireMinionItem : SquireMinionItem<ArmoredBoneSquireMinionBuff, ArmoredBoneSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Armored Bones");
			Tooltip.SetDefault("Summons a squire\nAn armored bone squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 7f;
			item.width = 24;
			item.height = 38;
			item.damage = 90;
			item.value = Item.sellPrice(0, 8, 0, 0);
			item.rare = ItemRarityID.Yellow;
		}
	}

	public class ArmoredBoneSquireSpiritProjectile : ModProjectile
	{

		const int MAX_VELOCITY = 20;
		public const int STARTING_VELOCITY = 12;
		const int INERTIA = 8;
		const float ACCELERATION = (MAX_VELOCITY - STARTING_VELOCITY) / 30f;
		private float currentSpeed = STARTING_VELOCITY;

		private Vector2 targetPosition = Vector2.Zero;
		private Vector2 targetDirection = Vector2.Zero;
		public override void SetDefaults()
		{
			projectile.timeLeft = 120;
			projectile.penetrate = 1;
			projectile.width = 10;
			projectile.height = 14;
			projectile.tileCollide = false;
			projectile.friendly = true;
		}

		public bool SetDirection
		{
			get => projectile.localAI[0] != 0f;
			set => projectile.localAI[0] = value ? 1f : 0f;
		}

		public override void AI()
		{
			Player player = Main.player[Main.myPlayer];
			if (targetDirection == Vector2.Zero && !SetDirection)
			{
				SetDirection = true;

				Main.PlaySound(SoundID.Item1, projectile.Center);

				//Take the initially given velocity and use it as the direction
				targetDirection = projectile.velocity;
				targetDirection.SafeNormalize();
			}
			if (targetPosition == Vector2.Zero)
			{
				targetPosition = player.Center + targetDirection * Vector2.Distance(player.Center, projectile.Center);
			}
			Vector2 vector2Target = targetPosition - projectile.Center;
			if (vector2Target.Length() < 4 * MAX_VELOCITY)
			{
				targetPosition += targetDirection * MAX_VELOCITY;
				vector2Target = targetPosition - projectile.Center;
			}
			if (projectile.timeLeft < 110)
			{
				projectile.tileCollide = true;
			}
			vector2Target.SafeNormalize();
			vector2Target *= currentSpeed;
			projectile.velocity = (projectile.velocity * (INERTIA - 1) + vector2Target) / INERTIA;
			projectile.rotation = MathHelper.PiOver2 + projectile.velocity.ToRotation();
			if (currentSpeed < MAX_VELOCITY)
			{
				currentSpeed += ACCELERATION;
			}
			Lighting.AddLight(projectile.Center, Color.LightCyan.ToVector3());
		}
	}


	public class ArmoredBoneSquireMinion : WeaponHoldingSquire<ArmoredBoneSquireMinionBuff>
	{
		protected override int AttackFrames => 27;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/BoneWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/ArmoredBoneSquire/ArmoredBoneSquireFlailBall";

		// swing weapon in a full circle
		protected override float SwingAngle1 => SwingAngle0 - 2 * (float)Math.PI;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override float knockbackSelf => 5f;

		private int firingFrame1 = 0;
		private int firingFrame2 = 15;
		public ArmoredBoneSquireMinion() : base(ItemType<ArmoredBoneSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Armored Bone Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 20;
			projectile.height = 32;
			projectile.localNPCHitCooldown = AttackFrames / 2;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(target, damage, knockback, crit);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			// bit of a long formula
			Vector2 angleVector = UnitVectorFromWeaponAngle();
			Vector2 flailPosition = projectile.Center +
				WeaponCenterOfRotation + angleVector * WeaponDistanceFromCenter();
			if (attackFrame == 0)
			{
				firingFrame1 = Main.rand.Next(AttackFrames);
				firingFrame2 = Main.rand.Next(AttackFrames);
			}
			if (attackFrame == firingFrame1 || attackFrame == firingFrame2)
			{
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						flailPosition,
						angleVector * ArmoredBoneSquireSpiritProjectile.STARTING_VELOCITY,
						ProjectileType<ArmoredBoneSquireSpiritProjectile>(),
						(int)(projectile.damage * 1.25f),
						projectile.knockBack,
						Main.myPlayer);
				}
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (usingWeapon)
			{
				Texture2D chainTexture = GetTexture("AmuletOfManyMinions/Projectiles/Squires/ArmoredBoneSquire/ArmoredBoneSquireFlailChain");
				Vector2 chainVector = UnitVectorFromWeaponAngle();
				float r = (float)Math.PI / 2 + chainVector.ToRotation();
				Vector2 center = projectile.Center + WeaponCenterOfRotation;
				Rectangle bounds = chainTexture.Bounds;
				Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
				int i;
				for (i = bounds.Height / 2; i < WeaponDistanceFromCenter(); i += bounds.Height)
				{
					Vector2 pos = center + chainVector * i;
					spriteBatch.Draw(chainTexture, pos - Main.screenPosition,
						bounds, lightColor, r,
						origin, 1, SpriteEffects.None, 0);
				}

			}
			base.PostDraw(spriteBatch, lightColor);
		}

		protected override float WeaponDistanceFromCenter() => 45;

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() - 10;
		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 10;

		public override float ComputeIdleSpeed() => 18;

		public override float ComputeTargetedSpeed() => 18;

		public override float MaxDistanceFromPlayer() => 220;
	}
}
