using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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

	public abstract class WeaponHoldingSquire : SquireMinion
	{
		protected bool usingWeapon = false;
		protected abstract int AttackFrames { get; }

		protected virtual int SpaceBetweenFrames => 42;
		protected virtual int BodyFrames => 1;
		protected virtual float SwingAngle0 => 5 * (float)Math.PI / 8;
		protected virtual float SwingAngle1 => -(float)Math.PI / 4;
		protected virtual string WingTexturePath => null;
		protected abstract string WeaponTexturePath { get; } //TODO 1.4 cache this //Actually not possible since it is supposed to be dynamic :pensive:
		protected virtual Vector2 WingOffset => Vector2.Zero;
		protected virtual WeaponAimMode aimMode => WeaponAimMode.TOWARDS_MOUSE;
		protected virtual WeaponSpriteOrientation spriteOrientation => WeaponSpriteOrientation.DIAGONAL;
		protected virtual Vector2 WeaponCenterOfRotation => Vector2.Zero;

		protected virtual SoundStyle? attackSound => SoundID.Item1;

		// used for motion blur effects
		protected Vector2 lastWeaponPos;
		protected virtual float knockbackSelf => 10f;
		protected int wingFrame = 0;
		protected int attackFrame = 0;
		protected float weaponAngle = 0;

		protected virtual Asset<Texture2D> WeaponTexture => ExtraTextures[1];

		public override void LoadAssets()
		{
			// these throw off the extra texture indices of subclasses, be careful
			AddTexture(WingTexturePath);
			AddTexture(WeaponTexturePath);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			// fixed angle weapons are really good at hitting enemies over and over again
			// so give them a longer cooldown
			if (aimMode == WeaponAimMode.FIXED)
			{
				Projectile.localNPCHitCooldown = AttackFrames - 2;
			}
			else
			{
				Projectile.localNPCHitCooldown = AttackFrames / 2;
			}
			UseBeacon = false;
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
				Projectile.spriteDirection = direction;
			}
			if (IsAttacking())
			{
				attackFrame = (attackFrame + 1) % ModifiedAttackFrames;
			}
			return base.IdleBehavior();
		}

		protected int ModifiedAttackFrames => attackSpeedCanBeModified ?
			(int)(AttackFrames * Player.GetModPlayer<SquireModPlayer>().FullSquireAttackSpeedModifier) :
			AttackFrames;

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
				Vector2 tipCenter = Projectile.Center + WeaponCenterOfRotation + i * unitAngle;
				Rectangle tipHitbox = new Rectangle((int)tipCenter.X - 8, (int)tipCenter.Y - 8, 16, 16);
				if (tipHitbox.Intersects(targetHitbox))
				{
					return Collision.CanHitLine(
						tipHitbox.Center.ToVector2(), 1, 1,
						Projectile.Center, 1, 1);
				}
			}
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = Projectile.spriteDirection; // always knock projectile away from player
		}

		protected virtual float GetFixedWeaponAngle()
		{
			float angleStep = (SwingAngle1 - SwingAngle0) / ModifiedAttackFrames;
			return SwingAngle0 + angleStep * attackFrame;
		}

		protected float GetMouseWeaponAngle()
		{
			Vector2 attackVector;
			// when the squire is close enough to the mouse, attack along the 
			// mouse-player line
			Vector2? _mouseWorld = Player.GetModPlayer<MousePlayer>().GetMousePosition();
			if (_mouseWorld is Vector2 mouseWorld)
			{
				if (Vector2.Distance(mouseWorld, Projectile.Center) < 48)
				{
					attackVector = mouseWorld - Player.Center;
				}
				else
				{
					//otherwise, attack along the mouse-squire line
					attackVector = mouseWorld - WeaponCenter();
				}
				if (Projectile.spriteDirection == 1)
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

		public override bool PreDraw(ref Color lightColor)
		{
			if (WingTexturePath != null)
			{
				Texture2D wingTexture = ExtraTextures[0].Value;
				Vector2 wingOffset = WingOffset;
				wingOffset.X *= Projectile.spriteDirection;
				Vector2 pos = Projectile.Center + wingOffset;
				Rectangle bounds = new Rectangle(0, wingTexture.Height / 4 * (wingFrame % 4), wingTexture.Width, wingTexture.Height / 4);
				SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
				float r = Projectile.rotation;
				Main.EntitySpriteDraw(wingTexture, pos - Main.screenPosition,
					bounds, lightColor, r, bounds.GetOrigin(), 1, effects, 0);
			}
			return true;
		}

		public override void PostDraw(Color lightColor)
		{
			PartyHatSystem.DrawManualHat(Projectile, lightColor);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Vector2 pos = Projectile.Center;
			float r = Projectile.rotation;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
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
				DrawWeapon(lightColor);
			}
			Rectangle bounds = new Rectangle(0, armFrame * SpaceBetweenFrames, Projectile.width, Projectile.height);
			Vector2 offset = new(DrawOriginOffsetX, DrawOriginOffsetY);
			Main.EntitySpriteDraw(texture, pos + offset - Main.screenPosition,
				bounds, lightColor, r, bounds.GetOrigin(), 1, effects, 0);
		}

		protected virtual Vector2 WeaponCenter()
		{
			return Projectile.Center;
		}

		protected Vector2 UnitVectorFromWeaponAngle()
		{
			if (Projectile.spriteDirection == 1)
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
			return Projectile.spriteDirection * (rotationBase - weaponAngle);
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			usingWeapon = true;
			weaponAngle = GetWeaponAngle();
			if (attackFrame == 0 && attackSound.HasValue && !usingSpecial)
			{
				SoundEngine.PlaySound(attackSound.Value, Projectile.Center);
			}
			base.StandardTargetedMovement(vectorToTargetPosition);
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			usingWeapon = false;
			weaponAngle = GetWeaponAngle();
			base.IdleMovement(vectorToIdlePosition);
		}

		protected virtual Rectangle GetWeaponTextureBounds(Texture2D texture)
		{
			return new Rectangle(0, 0, texture.Width, texture.Height);
		}

		protected virtual void DrawWeapon(Color lightColor)
		{
			if (WeaponTexturePath == null)
			{
				return;
			}
			Texture2D texture = WeaponTexture.Value;
			Rectangle bounds = GetWeaponTextureBounds(texture);
			float r = SpriteRotationFromWeaponAngle();
			lastWeaponPos = GetWeaponSpriteLocation();
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(texture, lastWeaponPos - Main.screenPosition,
				bounds, lightColor, r, bounds.GetOrigin(), 1, effects, 0);
		}

		protected virtual Vector2 GetWeaponSpriteLocation()
		{
			Vector2 center = UnitVectorFromWeaponAngle() * WeaponDistanceFromCenter();
			Vector2 weaponOffset = WeaponCenterOfRotation;
			weaponOffset.X *= Projectile.spriteDirection;
			return Projectile.Center + WeaponCenterOfRotation + center;
		}

		protected abstract float WeaponDistanceFromCenter();

		protected virtual int? GetSpriteDirection()
		{
			if (VectorToTarget is Vector2 target)
			{
				MousePlayer mPlayer = Player.GetModPlayer<MousePlayer>();
				Vector2? _mouseWorld = mPlayer.GetMousePosition();
				if (_mouseWorld is Vector2 mouseWorld)
				{
					return Math.Sign((mouseWorld - Player.Center).X);
				}
				else return null;
			}
			else if (attackFrame > 0)
			{
				// don't change directions while weapon is still out
			}
			else if (Projectile.velocity.X < -1)
			{
				return -1;
			}
			else if (Projectile.velocity.X > 1)
			{
				return 1;
			}
			return null;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			Projectile.rotation = Projectile.velocity.X * 0.05f;
			Projectile.frameCounter++;
			if (Projectile.frameCounter == FrameSpeed)
			{
				Projectile.frameCounter = 0;
				wingFrame += 1;
			}
		}

	}
}
