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

		public static int TimeLeft = 1800; //Don't worry, this becomes like half a second if there's no target.
		public static bool enemyNearby = false;

		const int MAX_VELOCITY = 14;
		public const int STARTING_VELOCITY = 0;
		const float ACCELERATION = 0.2f;
		float currentSpeed = STARTING_VELOCITY;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.timeLeft = 1800;
			projectile.penetrate = 1;
			projectile.width = 8;
			projectile.height = 8;
			projectile.tileCollide = false;
			projectile.friendly = true;
		}

		public override void AI()
		{
			Vector2 move = Vector2.Zero;
			float lockonDistance = 500f;
			enemyNearby = false;
			projectile.localAI[0] = 0;
			for (int i = 0; i < 200; i++)
			{
				if (Main.npc[i].active && !Main.npc[i].dontTakeDamage && !Main.npc[i].friendly)
				{
					Vector2 newMove = Main.npc[i].Center - projectile.Center;
					float distanceTo = (float)Math.Sqrt(newMove.X * newMove.X + newMove.Y * newMove.Y);
					if (distanceTo < lockonDistance)
					{
						move = newMove;
						lockonDistance = distanceTo;
						enemyNearby = true;
					}
				}
			}

			if (currentSpeed < MAX_VELOCITY)
			{
				currentSpeed += ACCELERATION;
			}
			projectile.rotation = MathHelper.PiOver2 + projectile.velocity.ToRotation();
			AdjustMagnitude(ref move);
			projectile.velocity = (5 * projectile.velocity + move); //This controls how fast the projectile turns.
			AdjustMagnitude(ref projectile.velocity);
			Lighting.AddLight(projectile.Center, Color.Cyan.ToVector3());

			if (enemyNearby)
			{
				int bonedust = Dust.NewDust(projectile.position, projectile.width, projectile.height, 137, 0f, 0f, 0, Scale: 1f);
				Main.dust[bonedust].position.X = projectile.Center.X - 4f + (float)Main.rand.Next(-2, 3);
				Main.dust[bonedust].position.Y = projectile.Center.Y - (float)Main.rand.Next(-2, 3);
				Main.dust[bonedust].noGravity = true;
			}
			if (!enemyNearby)
			{
				projectile.rotation = 0;
				projectile.timeLeft -= 60;
				projectile.velocity = Vector2.Zero;
				int idlebonedust = Dust.NewDust(projectile.position, 4, 4, 137, projectile.velocity.X, projectile.velocity.Y, 0, Scale: 1f);
				Main.dust[idlebonedust].position.Y = projectile.Center.Y + 2f;
				Main.dust[idlebonedust].noGravity = true;
			}
		}

		private void AdjustMagnitude(ref Vector2 vector)
		{
			float magnitude = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
			if (magnitude > currentSpeed)
			{
				vector *= currentSpeed / magnitude;
			}
		}
	}


	public class ArmoredBoneSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<ArmoredBoneSquireMinionBuff>();
		protected override int AttackFrames => 27;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/BoneWings";
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/ArmoredBoneSquire/ArmoredBoneSquireFlailBall";

		// swing weapon in a full circle
		protected override float SwingAngle1 => SwingAngle0 - 2 * (float)Math.PI;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override float knockbackSelf => 5f;

		protected override LegacySoundStyle attackSound => SoundID.Item1;

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
			projectile.localNPCHitCooldown = AttackFrames / 3;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// glowy face mask.
			return base.PreDraw(spriteBatch, Color.White);
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
						Vector2.Zero,
						ProjectileType<ArmoredBoneSquireSpiritProjectile>(),
						(int)(projectile.damage / 2),
						projectile.knockBack / 2,
						Main.myPlayer);
					Main.PlaySound(SoundID.Item20, projectile.position);
				}
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D glow = GetTexture(Texture + "_Glow");
			float glowR = projectile.rotation;
			Vector2 glowpos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle glowbounds = glow.Bounds;
			Vector2 gloworigin = glowbounds.Center.ToVector2();
			spriteBatch.Draw(glow, glowpos - Main.screenPosition,
				glowbounds, Color.White, glowR,
				gloworigin, 1, effects, 0);

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
