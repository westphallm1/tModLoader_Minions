using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.GoldenRogueSquire
{
	public class GoldenRogueSquireMinionBuff : MinionBuff
	{
		public GoldenRogueSquireMinionBuff() : base(ProjectileType<GoldenRogueSquireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Golden Rogue Squire");
			Description.SetDefault("A golden rogue squire will follow your orders!");
		}
	}
	public class GoldenRogueSquireMinionItem : SquireMinionItem<GoldenRogueSquireMinionBuff, GoldenRogueSquireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Golden Rogue Crest");
			Tooltip.SetDefault("Summons a squire\nA golden rogue squire will fight for you!\nClick and hold to guide its attacks");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 4.0f;
			item.width = 24;
			item.height = 38;
            item.damage = 33;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.Orange;
		}
	}

	public class GoldenDagger : ModProjectile
	{

		const int TimeToLive = 60;
		const int TimeLeftToStartFalling = TimeToLive - 15;

		public override string Texture => "Terraria/Projectile_" + ProjectileID.MagicDagger;

		private Vector2 baseVelocity;
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
			baseVelocity = default;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);
			Rectangle bounds = texture.Bounds;
			Vector2 origin = texture.Bounds.Center.ToVector2();
			Color color = Color.White;
			float scale = 1;
			if(projectile.timeLeft < TimeLeftToStartFalling)
			{
				color.A = 64;
				scale = projectile.timeLeft / (float)TimeLeftToStartFalling;
			}
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				bounds, color, projectile.rotation,
				origin, scale, 0, 0);
			return false;
		}
		public override void AI()
		{
            Projectile parent = Main.projectile[(int)projectile.ai[0]];
			if(baseVelocity == default)
			{
				baseVelocity = projectile.velocity;
				projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
			if(parent.active && projectile.timeLeft > TimeLeftToStartFalling)
			{
				projectile.velocity = parent.velocity + baseVelocity;
			} else if (projectile.timeLeft == TimeLeftToStartFalling || projectile.timeLeft == 15)
			{
				//damage fall off
				projectile.damage = 3 * projectile.damage / 4;
			} else
			{
				projectile.velocity.Y = Math.Min(projectile.velocity.Y + 0.5f, 16);
				projectile.rotation += 0.15f;
				projectile.velocity.X *= 0.99f;
			}
		}
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// manually bypass defense
			// this may not be wholly correct
			int defenseBypass = 20;
			int defense = Math.Min(target.defense, defenseBypass);
			damage += defense / 2;
		}
	}

	public class GoldenRogueSquireMinion : WeaponHoldingSquire<GoldenRogueSquireMinionBuff>
	{
        protected override int AttackFrames => 15;

		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/GoldenWings";

		protected override string WeaponTexturePath => null;

		protected override Vector2 WingOffset => new Vector2(-4, 4);

		protected override float knockbackSelf => 5;
		protected override Vector2 WeaponCenterOfRotation => new Vector2(-4, 4);

		protected override WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.VERTICAL;

		protected override WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;

		private int daggerSpeed = 10;
		private float daggerSpread = 2.25f;

		public GoldenRogueSquireMinion() : base(ItemType<GoldenRogueSquireMinionItem>()) { }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Golden Rogue Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 32;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// glowy golden wings
			return base.PreDraw(spriteBatch, Color.White);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
            base.TargetedMovement(vectorToTargetPosition);
			if (attackFrame == 0)
			{
				if (Main.myPlayer == player.whoAmI)
				{
                    Vector2 vector2Mouse = UnitVectorFromWeaponAngle();
					vector2Mouse *= daggerSpeed;
					Vector2 tangent = new Vector2(vector2Mouse.Y, -vector2Mouse.X);
					tangent.Normalize();
					tangent *= daggerSpread;
					Vector2[] velocities =
					{
						vector2Mouse - tangent,
						vector2Mouse,
						vector2Mouse + tangent
					};
					foreach(Vector2 velocity in velocities)
					{

						Projectile.NewProjectile(projectile.Center,
							velocity,
							ProjectileType<GoldenDagger>(),
							projectile.damage,
							projectile.knockBack,
							Main.myPlayer,
							ai0: projectile.whoAmI);
					}
				}
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D glow = GetTexture(Texture + "_Glow");
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Rectangle bounds = glow.Bounds;
			Vector2 origin = bounds.Center.ToVector2();
			spriteBatch.Draw(glow, pos - Main.screenPosition,
				bounds, Color.White, r,
				origin, 1, effects, 0);
			if(attackFrame < 10)
			{
				// only draw arm at start of attack
				base.PostDraw(spriteBatch, lightColor);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return false;
		}

		public override float MaxDistanceFromPlayer() => 232;

		public override float ComputeTargetedSpeed() => 11;

		public override float ComputeIdleSpeed() => 11;

		protected override float WeaponDistanceFromCenter() => 12;
	}
}

