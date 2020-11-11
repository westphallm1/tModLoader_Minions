using AmuletOfManyMinions.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;


namespace AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses
{

	public enum WeaponSpriteOrientation
	{
		DIAGONAL,
		VERTICAL
	}

	public enum WeaponAimMode
	{
		FIXED,
		TOWARDS_MOUSE
	}

	public abstract class WeaponHoldingSquire<T> : SquireMinion<T> where T : ModBuff
	{
		protected bool usingWeapon = false;
		protected abstract int AttackFrames { get; }
		protected virtual int SpaceBetweenFrames => 42;
		protected virtual int BodyFrames => 1;
		protected virtual float SwingAngle0 => 5 * (float)Math.PI / 8;
		protected virtual float SwingAngle1 => -(float)Math.PI / 4;
		protected virtual string WingTexturePath => null;
		protected abstract string WeaponTexturePath { get; }
		protected virtual Vector2 WingOffset => Vector2.Zero;
		protected virtual WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;
		protected virtual WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.DIAGONAL;
		protected virtual Vector2 WeaponCenterOfRotation => Vector2.Zero;

		protected virtual LegacySoundStyle attackSound => SoundID.Item1;

		protected virtual float knockbackSelf => 10f;
		protected int wingFrame = 0;
		protected int attackFrame = 0;
		protected float weaponAngle = 0;
		public WeaponHoldingSquire(int itemID) : base(itemID) { }

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.usesLocalNPCImmunity = true;
			// fixed angle weapons are really good at hitting enemies over and over again
			// so give them a longer cooldown
			if (aimMode == WeaponAimMode.FIXED)
			{
				projectile.localNPCHitCooldown = AttackFrames - 2;
			}
			else
			{
				projectile.localNPCHitCooldown = AttackFrames / 2;
			}
			useBeacon = false;
		}

		protected virtual bool IsAttacking()
		{
			return usingWeapon || attackFrame > 0;
		}

		protected virtual int WeaponHitboxEnd()
		{
			return 40;
		}

		protected virtual int WeaponHitboxStart()
		{
			return 16;
		}

		public override Vector2 IdleBehavior()
		{
			if (GetSpriteDirection() is int direction)
			{
				projectile.spriteDirection = direction;
			}
			if (IsAttacking())
			{
				attackFrame = (attackFrame + 1) % AttackFrames;
			}
			return base.IdleBehavior();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// use a computed weapon hitbox instead of the projectile's natural hitbox
			if (!IsAttacking())
			{
				return false;
			}
			Vector2 unitAngle = UnitVectorFromWeaponAngle();
			for (int i = WeaponHitboxStart(); i < WeaponHitboxEnd(); i += 8)
			{
				Vector2 tipCenter = projectile.Center + WeaponCenterOfRotation + i * unitAngle;
				Rectangle tipHitbox = new Rectangle((int)tipCenter.X - 8, (int)tipCenter.Y - 8, 16, 16);
				if (tipHitbox.Intersects(targetHitbox))
				{
					return Collision.CanHitLine(
						tipHitbox.Center.ToVector2(), 1, 1,
						projectile.Center, 1, 1);
				}
			}
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = projectile.spriteDirection; // always knock projectile away from player
		}

		protected virtual float GetFixedWeaponAngle()
		{
			float angleStep = (SwingAngle1 - SwingAngle0) / AttackFrames;
			return SwingAngle0 + angleStep * attackFrame;
		}

		protected float GetMouseWeaponAngle()
		{
			Vector2 attackVector;
			// when the squire is close enough to the mouse, attack along the 
			// mouse-player line
			Vector2? _mouseWorld = player.GetModPlayer<MousePlayer>().GetMousePosition();
			if (_mouseWorld is Vector2 mouseWorld)
			{
				if (Vector2.Distance(mouseWorld, projectile.Center) < 48)
				{
					attackVector = mouseWorld - player.Center;
				}
				else
				{
					//otherwise, attack along the mouse-squire line
					attackVector = mouseWorld - WeaponCenter();
				}
				if (projectile.spriteDirection == 1)
				{
					return -attackVector.ToRotation();
				}
				else
				{
					// this code is rather unfortunate, but need to normalize
					// everything to [-Math.PI/2, Math.PI/2] for arm drawing to work
					float angle = (float)-Math.PI + attackVector.ToRotation();
					if (angle < -Math.PI / 2)
					{
						angle += 2 * (float)Math.PI;
					}
					return angle;
				}
			}
			return 0f;
		}

		protected virtual float GetWeaponAngle()
		{
			if (!IsAttacking())
			{
				return 0;
			}
			if (aimMode == WeaponAimMode.FIXED)
			{
				return GetFixedWeaponAngle();
			}
			else
			{
				return GetMouseWeaponAngle();
			}

		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			if (WingTexturePath != null)
			{
				Texture2D wingTexture = ModContent.GetTexture(WingTexturePath);
				Vector2 wingOffset = WingOffset;
				wingOffset.X *= projectile.spriteDirection;
				Vector2 pos = projectile.Center + wingOffset;
				Rectangle bounds = new Rectangle(0, wingTexture.Height / 4 * (wingFrame % 4), wingTexture.Width, wingTexture.Height / 4);
				Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
				SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
				float r = projectile.rotation;
				spriteBatch.Draw(wingTexture, pos - Main.screenPosition,
					bounds, lightColor, r,
					origin, 1, effects, 0);
			}
			return true;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = Main.projectileTexture[projectile.type];
			Vector2 origin = new Vector2(projectile.width / 2f, projectile.height / 2f);
			Vector2 pos = projectile.Center;
			float r = projectile.rotation;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			int armFrame;
			if (!IsAttacking())
			{
				armFrame = BodyFrames;
			}
			else if (weaponAngle > (float)Math.PI / 8)
			{
				armFrame = BodyFrames + 1;
			}
			else if (weaponAngle > -Math.PI / 8)
			{
				armFrame = BodyFrames + 2;
			}
			else
			{
				armFrame = BodyFrames + 3;
			}
			if (IsAttacking())
			{
				DrawWeapon(spriteBatch, lightColor);
			}
			Rectangle bounds = new Rectangle(0, armFrame * SpaceBetweenFrames, projectile.width, projectile.height);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
		}

		protected virtual Vector2 WeaponCenter()
		{
			return projectile.Center;
		}

		protected Vector2 UnitVectorFromWeaponAngle()
		{
			if (projectile.spriteDirection == 1)
			{
				return new Vector2((float)Math.Cos(-weaponAngle), (float)Math.Sin(-weaponAngle));
			}
			else
			{
				var reflectedAngle = Math.PI - weaponAngle;
				return new Vector2((float)Math.Cos(-reflectedAngle), (float)Math.Sin(-reflectedAngle));
			}
		}

		protected virtual float SpriteRotationFromWeaponAngle()
		{
			float rotationBase = spriteOrientation == WeaponSpriteOrientation.DIAGONAL ? (float)Math.PI / 4 : 0;
			if (projectile.spriteDirection == 1)
			{
				return rotationBase - weaponAngle;
			}
			else
			{
				return -(rotationBase - weaponAngle);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			usingWeapon = true;
			weaponAngle = GetWeaponAngle();
			if(attackFrame == 0 && attackSound != null)
			{
				Main.PlaySound(attackSound, projectile.Center);
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			usingWeapon = false;
			weaponAngle = GetWeaponAngle();
			base.IdleMovement(vectorToIdlePosition);
		}

		protected virtual void DrawWeapon(SpriteBatch spriteBatch, Color lightColor)
		{
			if (WeaponTexturePath == null)
			{
				return;
			}
			Texture2D texture = ModContent.GetTexture(WeaponTexturePath);
			Rectangle bounds = new Rectangle(0, 0, texture.Width, texture.Height);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2); // origin should hopefully be more or less center of squire
			Vector2 center = UnitVectorFromWeaponAngle() * WeaponDistanceFromCenter();
			float r = SpriteRotationFromWeaponAngle();
			Vector2 weaponOffset = WeaponCenterOfRotation;
			weaponOffset.X *= projectile.spriteDirection;
			Vector2 pos = projectile.Center + WeaponCenterOfRotation + center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
		}

		protected abstract float WeaponDistanceFromCenter();

		protected virtual int? GetSpriteDirection()
		{
			if (vectorToTarget is Vector2 target)
			{
				MousePlayer mPlayer = player.GetModPlayer<MousePlayer>();
				Vector2? _mouseWorld = mPlayer.GetMousePosition();
				if (_mouseWorld is Vector2 mouseWorld)
				{
					return Math.Sign((mouseWorld - player.Center).X);
				}
				else return null;
			}
			else if (attackFrame > 0)
			{
				// don't change directions while weapon is still out
			}
			else if (projectile.velocity.X < -1)
			{
				return -1;
			}
			else if (projectile.velocity.X > 1)
			{
				return 1;
			}
			return null;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			projectile.rotation = projectile.velocity.X * 0.05f;
			projectile.frameCounter++;
			if (projectile.frameCounter == frameSpeed)
			{
				projectile.frameCounter = 0;
				wingFrame += 1;
			}
		}

	}
}
