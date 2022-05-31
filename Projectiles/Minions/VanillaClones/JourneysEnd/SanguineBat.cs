﻿using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd
{
	public class SanguineBatMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.BatOfLight;
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SanguineBatMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.BatOfLight") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.BatOfLight"));
		}

	}

	public class SanguineBatMinionItem : VanillaCloneMinionItem<SanguineBatMinionBuff, SanguineBatMinion>
	{
		internal override int VanillaItemID => ItemID.SanguineStaff;

		internal override string VanillaItemName => "SanguineStaff";
	}

	public class SanguineBatMinion : HeadCirclingGroupAwareMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BatOfLight;
		private int framesSinceLastHit;
		private int cooldownAfterHitFrames = 12;
		private NPC currentTarget;
		internal override int BuffId => BuffType<SanguineBatMinionBuff>();

		private MotionBlurDrawer blurDrawer;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.BatOfLight") + " (AoMM Version)");
			IdleLocationSets.circlingHead.Add(Type);
			Main.projFrames[Projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 60;
			targetSearchDistance = 600;
			circleHelper.idleBumble = false;
			bumbleSpriteDirection = -1;
			attackThroughWalls = true;
			maxSpeed = 18;
			idleInertia = 6;
			blurDrawer = new MotionBlurDrawer(5);
		}

		public override Vector2 IdleBehavior()
		{

			base.IdleBehavior();
			List<Projectile> minions = GetMinionsOfType(Type);
			if (minions.Count == 0)
			{
				return Vector2.Zero;
			}
			int myIndex = minions.FindIndex(p => p.whoAmI == Projectile.whoAmI);
			float myAngle = MathHelper.Pi * myIndex / Math.Max(1, minions.Count - 1);
			Vector2 offset = myAngle.ToRotationVector2() * 32;
			offset.X *= 0.75f;
			if(currentTarget != default && !currentTarget.active)
			{
				currentTarget = default;
			}
			return player.Center - offset - Projectile.Center;

		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(0, 4);
			if(vectorToTarget == default && vectorToIdle.LengthSquared() < 16 * 16)
			{
				Projectile.spriteDirection = player.direction;
			} else
			{
				Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height / Main.projFrames[Type];
			Rectangle bounds = new(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new(bounds.Width / 2, bounds.Height / 2);
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Color outlineColor = Color.Red;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			// motion blur
			float blurScale = 1f;
			for (int k = 0; k < blurDrawer.BlurLength; k++)
			{
				if(!blurDrawer.GetBlurPosAndColor(k, outlineColor, out Vector2 blurPos, out Color blurColor)) { break; }
				Main.EntitySpriteDraw(texture, blurPos - Main.screenPosition, bounds, blurColor * 0.25f, 
					Projectile.rotation, origin, blurScale, 0, 0);
				blurScale *= 0.85f;
			}

			// glowy outline
			for(int i = -1; i <= 1; i+= 1)
			{
				for(int j = -1; j <= 1; j+= 1)
				{
					Vector2 offset = 2 * new Vector2(i, j).RotatedBy(Projectile.rotation);
					Main.EntitySpriteDraw(texture, pos + offset,
						bounds, outlineColor * 0.5f, Projectile.rotation, origin, 1, effects, 0);
				}
			}
			// main entity
			Main.EntitySpriteDraw(texture, pos,
				bounds, Color.White, Projectile.rotation, origin, 1, effects, 0);

			return false;
		}

		public override Vector2? FindTarget()
		{
			if(currentTarget != null)
			{
				targetNPCIndex = currentTarget.whoAmI;
				return currentTarget.Center - Projectile.Center;
			} 
			else if (attackState != AttackState.RETURNING && IsMyTurn() && base.FindTarget() is Vector2 target)
			{
				currentTarget = Main.npc[(int)targetNPCIndex];
				return target;
			} else
			{
				return null;
			}
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int targetSpeed = maxSpeed - 3;
			if(attackState == AttackState.RETURNING)
			{
				IdleMovement(vectorToIdle);
				return;
			}
			if(vectorToTargetPosition.LengthSquared() > targetSpeed * targetSpeed)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= targetSpeed;
			}
			Projectile.velocity = vectorToTargetPosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(vectorToIdlePosition.LengthSquared() < 24 * 24)
			{
				attackState = AttackState.IDLE;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void OnHitTarget(NPC target)
		{
			attackState = AttackState.RETURNING;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurDrawer.Update(Projectile.Center, vectorToTarget != default || Projectile.velocity.LengthSquared() > 2 * 2);
		}
	}
}