using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;
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
		public override int BuffId => BuffType<SanguineBatMinionBuff>();

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
			AttackThroughWalls = true;
			maxSpeed = 16;
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
			return Player.Center - offset - Projectile.Center;

		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(0, 4);
			if(VectorToTarget == default && VectorToIdle.LengthSquared() < 16 * 16)
			{
				Projectile.spriteDirection = Player.direction;
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
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Color outlineColor = Color.Red;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			blurDrawer.DrawBlur(texture, outlineColor * 0.25f, bounds, Projectile.rotation);
			OutlineDrawer.DrawOutline(texture, pos, bounds, outlineColor, Projectile.rotation, effects);
			// main entity
			Main.EntitySpriteDraw(texture, pos,
				bounds, Color.White, Projectile.rotation, bounds.GetOrigin(), 1, effects, 0);

			return false;
		}

		public override Vector2? FindTarget()
		{
			if(currentTarget != null)
			{
				TargetNPCIndex = currentTarget.whoAmI;
				return currentTarget.Center - Projectile.Center;
			} 
			else if (AttackState != AttackState.RETURNING && IsMyTurn() && base.FindTarget() is Vector2 target)
			{
				currentTarget = Main.npc[(int)TargetNPCIndex];
				return target;
			} else
			{
				return null;
			}
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			maxSpeed = 16;
			if(AttackState == AttackState.RETURNING)
			{
				IdleMovement(VectorToIdle);
				return;
			}
			if(vectorToTargetPosition.LengthSquared() > maxSpeed * maxSpeed)
			{
				vectorToTargetPosition.SafeNormalize();
				vectorToTargetPosition *= maxSpeed;
			}
			Projectile.velocity = vectorToTargetPosition;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			maxSpeed = 16 + (int)Player.velocity.Length(); // keep pace with the player when coming back
			if(vectorToIdlePosition.LengthSquared() < 24 * 24)
			{
				AttackState = AttackState.IDLE;
			}
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void OnHitTarget(NPC target)
		{
			AttackState = AttackState.RETURNING;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurDrawer.Update(Projectile.Center, VectorToTarget != default || Projectile.velocity.LengthSquared() > 2 * 2);
		}
	}
}
