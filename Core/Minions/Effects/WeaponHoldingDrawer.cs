using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Effects
{
	class WeaponHoldingDrawer
	{
		Vector2 lastAttackVector;
		internal Vector2 WeaponOffset;
		internal float WeaponHoldDistance;

		internal int AttackDuration = 30;
		internal int ForwardDir = 1;
		internal int frame;
		internal int lastAttackFrame;
		internal float yOffsetScale = 1f;
		internal Projectile Projectile;
		internal WeaponSpriteOrientation spriteOrientation = WeaponSpriteOrientation.VERTICAL;

		internal int attackFrame => frame - lastAttackFrame;
		internal int attackDir => Math.Sign(lastAttackVector.X);
		
		public void Update(Projectile projectile, int animationFrame)
		{
			frame = animationFrame;
			Projectile = projectile;
		}

		public void StartAttack(Vector2 target)
		{
			if(attackFrame > AttackDuration)
			{
				lastAttackFrame = frame;
				lastAttackVector = target;
			}
		}

		public void Draw(Texture2D texture, Color lightColor)
		{
			if (lastAttackVector != default && attackFrame <= AttackDuration)
			{
				DrawWeapon(texture, lightColor);
			}
		}

		// lifted from WeaponHoldingSquire
		private float GetWeaponAngle(Vector2 attackVector)
		{
			float weaponAngle;
			if (attackDir == 1)
			{
				weaponAngle = attackVector.ToRotation();
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
				weaponAngle = angle;
			}
			if(spriteOrientation == WeaponSpriteOrientation.DIAGONAL)
			{
				weaponAngle += attackDir * MathHelper.PiOver4;
			}
			return weaponAngle;
		}

		//protected virtual float SpriteRotationFromWeaponAngle(float weaponAngle)
		//{
		//	float rotationBase = spriteOrientation == WeaponSpriteOrientation.DIAGONAL ? (float)Math.PI / 4 : 0;
		//	return (ForwardDir * Projectile.spriteDirection) * (rotationBase - weaponAngle);
		//}

		private void DrawWeapon(Texture2D texture, Color lightColor)
		{
			Vector2 holdOffset = lastAttackVector;
			holdOffset.SafeNormalize();
			holdOffset *= WeaponHoldDistance;
			holdOffset.Y *= yOffsetScale;
			Rectangle bounds = new Rectangle(0, 0, texture.Width, texture.Height);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2); // origin should hopefully be more or less center of squire
			float r = GetWeaponAngle(holdOffset);
			Vector2 pos = Projectile.Center + WeaponOffset + holdOffset;
			SpriteEffects effects = attackDir == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
		}

	}
}
