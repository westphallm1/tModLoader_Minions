using AmuletOfManyMinions.Core.Minions.Effects;
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
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Armored Bone Squire");
			Description.SetDefault("An armored bone squire will follow your orders!");
		}
	}

	public class ArmoredBoneSquireMinionItem : SquireMinionItem<ArmoredBoneSquireMinionBuff, ArmoredBoneSquireMinion>
	{
		protected override string SpecialName => "Spirit of the Flail";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Armored Bones");
			Tooltip.SetDefault("Summons a squire\nAn armored bone squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 7f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 90;
			Item.value = Item.sellPrice(0, 8, 0, 0);
			Item.rare = ItemRarityID.Yellow;
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
			ProjectileID.Sets.CountsAsHoming[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.timeLeft = 1800;
			Projectile.penetrate = 1;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			Vector2 move = Vector2.Zero;
			float lockonDistance = 500f;
			enemyNearby = false;
			Projectile.localAI[0] = 0;
			for (int i = 0; i < 200; i++)
			{
				if (Main.npc[i].active && !Main.npc[i].dontTakeDamage && !Main.npc[i].friendly)
				{
					Vector2 newMove = Main.npc[i].Center - Projectile.Center;
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
			Projectile.rotation = MathHelper.PiOver2 + Projectile.velocity.ToRotation();
			AdjustMagnitude(ref move);
			Projectile.velocity = (5 * Projectile.velocity + move); //This controls how fast the projectile turns.
			AdjustMagnitude(ref Projectile.velocity);
			Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3());

			if (enemyNearby)
			{
				int bonedust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 137, 0f, 0f, 0, Scale: 1f);
				Main.dust[bonedust].position.X = Projectile.Center.X - 4f + (float)Main.rand.Next(-2, 3);
				Main.dust[bonedust].position.Y = Projectile.Center.Y - (float)Main.rand.Next(-2, 3);
				Main.dust[bonedust].noGravity = true;
			}
			if (!enemyNearby)
			{
				Projectile.rotation = 0;
				Projectile.timeLeft -= 60;
				Projectile.velocity = Vector2.Zero;
				int idlebonedust = Dust.NewDust(Projectile.position, 4, 4, 137, Projectile.velocity.X, Projectile.velocity.Y, 0, Scale: 1f);
				Main.dust[idlebonedust].position.Y = Projectile.Center.Y + 2f;
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

	public class SpiritFlailWormMinion : SquireMinion
	{
		public SpiritFlailWormMinion() : base(ItemType<ArmoredBoneSquireMinionItem>()) { }

		internal override int BuffId => BuffType<ArmoredBoneSquireMinionBuff>();

		// used to allow the flail itself to move through walls a bit while the "center"
		// remains bounded by tiles
		internal Vector2 flailPosition;
		internal Vector2 flailVelocity;
		internal Vector2 flailTarget;
		internal int flailSpeed = 12;
		internal WormDrawer wormDrawer;
		private NPC target;
		private int firingFrame1 = 0;
		private int firingFrame2 = 15;
		private int attackFrames = 20;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spirit Flail Chain");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 3;
		}
		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			frameSpeed = 10;
			wormDrawer = new SpiritFlailDrawer();
			Projectile.minionSlots = 0;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return targetHitbox.Contains((Projectile.Center + flailPosition).ToPoint());
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			// prefer circling around the target, or between the target and the mouse
			if(GetClosestEnemyToPosition(syncedMouseWorld, 120f, false) is NPC newTarget)
			{
				target = newTarget;
			}
			if(target == default || !target.active || Vector2.DistanceSquared(target.Center, Projectile.Center) > 128 * 128)
			{
				target = default;
				float flailAngle = 2 * MathHelper.Pi * animationFrame / 60f;
				flailTarget = 40 * flailAngle.ToRotationVector2();
			} 
			else 
			{
				float flailAngle = 2 * MathHelper.Pi * animationFrame / attackFrames;
				flailTarget = 40 * flailAngle.ToRotationVector2();
				// circle between the mouse cursor and the target
				Vector2 axis = Projectile.Center - target.Center;
				axis.SafeNormalize();
				axis *= 40;
				flailTarget += target.Center + axis - Projectile.Center;
			}
			SpawnWisps();
			base.StandardTargetedMovement(vectorToTargetPosition);
			UpdateFlailOffset();
		}

		private void SpawnWisps()
		{
			int attackFrame = animationFrame % attackFrames;
			if(attackFrame == 0)
			{
				firingFrame1 = Main.rand.Next(attackFrames);
				firingFrame2 = Main.rand.Next(attackFrames);
			}
			if (attackFrame == firingFrame1 || attackFrame == firingFrame2)
			{
				if (Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						Projectile.GetProjectileSource_FromThis(),
						Projectile.Center + flailPosition,
						Vector2.Zero,
						ProjectileType<ArmoredBoneSquireSpiritProjectile>(),
						(int)(Projectile.damage / 2),
						Projectile.knockBack / 2,
						Main.myPlayer);
					SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
				}
			}
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			SpawnDust(10);
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			SpawnDust(10);
		}

		public void SpawnDust(int count)
		{
			Vector2 flailPos = Projectile.Center + flailPosition;
			for(int i = 0; i < count; i++)
			{
				int bonedust = Dust.NewDust(flailPos, Projectile.width, Projectile.height, 137, 0f, 0f, 0, Scale: 1f);
				Main.dust[bonedust].position.X = flailPos.X - 4f + (float)Main.rand.Next(-2, 3);
				Main.dust[bonedust].position.Y = flailPos.Y - (float)Main.rand.Next(-2, 3);
				Main.dust[bonedust].noGravity = true;
			}

		}

		private void UpdateFlailOffset()
		{
			Vector2 flailOffset = flailTarget - flailPosition;
			int inertia = 5;
			if(flailOffset.LengthSquared() > flailSpeed * flailSpeed)
			{
				flailOffset.Normalize();
				flailOffset *= flailSpeed;
			}
			flailVelocity = (flailVelocity * (inertia - 1) + flailOffset) / inertia;
			flailPosition += flailVelocity;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
			float flailAngle = 2 * MathHelper.Pi * animationFrame / 60f;
			flailTarget = 40 * flailAngle.ToRotationVector2();
			UpdateFlailOffset();
		}
		public override bool PreDraw(ref Color lightColor)
		{
			wormDrawer.Draw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type], lightColor);
			return false;
		}

		public override float MaxDistanceFromPlayer() => 800;

		public override float ComputeTargetedSpeed() => 18;

		public override float ComputeIdleSpeed() => 18;

		public override void AfterMoving()
		{
			base.AfterMoving();
			// todo add a little swirly effect
			Vector2 flailPos = Projectile.Center + flailPosition;
			wormDrawer.Update(Projectile.frame);
			wormDrawer.AddPosition(flailPos);
			Lighting.AddLight(flailPos, Color.Cyan.ToVector3());
		}
	}
	public class SpiritFlailDrawer : WormDrawer
	{

		public SpiritFlailDrawer() : base(128, 48, 200)
		{
			SegmentCount = 3; 
		}
		protected override void DrawHead()
		{
			Rectangle headFrame = new Rectangle(0, 32 * frame, 32, 32);
			AddSprite(2, headFrame);
		}

		protected override void DrawBody()
		{
			Rectangle body = new Rectangle(8, 98, 24, 6);
			for (int i = 0; i < SegmentCount + 1; i++)
			{
				AddSprite(30 + 22* i, body);
			}
		}


		protected override void DrawTail()
		{
			lightColor = Color.White * 0.85f; // bright/transparent
			Rectangle tail = new Rectangle(18, 108, 14, 14);
			int dist = 30 + 22 * (SegmentCount + 1);
			AddSprite(dist, tail);
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

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;
		protected override LegacySoundStyle attackSound => SoundID.Item1;

		private int firingFrame1 = 0;
		private int firingFrame2 = 15;
		public ArmoredBoneSquireMinion() : base(ItemType<ArmoredBoneSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Armored Bone Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}
		public override void LoadAssets()
		{
			base.LoadAssets();
			AddTexture(Texture + "_Glow");
			AddTexture("AmuletOfManyMinions/Projectiles/Squires/ArmoredBoneSquire/ArmoredBoneSquireFlailChain");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 20;
			Projectile.height = 32;
			Projectile.localNPCHitCooldown = AttackFrames / 3;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// glowy face mask.
			lightColor = Color.White;
			return base.PreDraw(ref lightColor);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.StandardTargetedMovement(vectorToTargetPosition);
			// bit of a long formula
			Vector2 angleVector = UnitVectorFromWeaponAngle();
			Vector2 flailPosition = Projectile.Center +
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
						Projectile.GetProjectileSource_FromThis(),
						flailPosition,
						Vector2.Zero,
						ProjectileType<ArmoredBoneSquireSpiritProjectile>(),
						(int)(Projectile.damage / 2),
						Projectile.knockBack / 2,
						Main.myPlayer);
					SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
				}
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			// the flail takes over while using special
			base.IdleMovement(vectorToIdle);
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D glow = ExtraTextures[2].Value;
			float glowR = Projectile.rotation;
			Vector2 glowpos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle glowbounds = glow.Bounds;
			Vector2 gloworigin = glowbounds.Center.ToVector2();
			Main.EntitySpriteDraw(glow, glowpos - Main.screenPosition,
				glowbounds, Color.White, glowR,
				gloworigin, 1, effects, 0);

			if (usingWeapon)
			{
				Texture2D chainTexture = ExtraTextures[3].Value;
				ChainDrawer drawer = new ChainDrawer(chainTexture.Bounds);
				Vector2 center = Projectile.Center + WeaponCenterOfRotation;
				Vector2 chainVector = UnitVectorFromWeaponAngle() * WeaponDistanceFromCenter();
				drawer.DrawChain(chainTexture, center, center + chainVector, Color.White);
			}
			base.PostDraw(lightColor);
		}

		public override void OnStartUsingSpecial()
		{
			if(player.whoAmI == Main.myPlayer)
			{
				Projectile p = Projectile.NewProjectileDirect(
					Projectile.GetProjectileSource_FromThis(),
					Projectile.Center, 
					Projectile.velocity, 
					ProjectileType<SpiritFlailWormMinion>(), 
					Projectile.damage, 
					Projectile.knockBack, 
					player.whoAmI);
				p.originalDamage = Projectile.originalDamage;
			}
		}

		public override void OnStopUsingSpecial()
		{
			if(player.whoAmI == Main.myPlayer)
			{
				for(int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if(p.active && p.owner == player.whoAmI && p.type == ProjectileType<SpiritFlailWormMinion>())
					{
						p.Kill();
						break;
					}
				}
			}
		}

		protected override float WeaponDistanceFromCenter() => 45;

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() - 10;
		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 10;

		public override float ComputeIdleSpeed() => 18;

		public override float ComputeTargetedSpeed() => 18;

		public override float MaxDistanceFromPlayer() => 220;
	}
}
