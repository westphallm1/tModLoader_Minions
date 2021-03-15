using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class PirateMinionBuff : MinionBuff
	{
		public PirateMinionBuff() : base(ProjectileType<PirateMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
		}

	}

	public class PirateMinionItem : MinionItem<PirateMinionBuff, PirateMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.PirateStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ItemName.SlimeStaff"));
			Tooltip.SetDefault("Summons a vampire slime to fight for you!\nIgnores 10 enemy defense");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.PirateStaff);
			base.SetDefaults();
		}
	}
	public class PirateMinion : SimpleGroundBasedMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.OneEyedPirate;
		protected override int BuffId => BuffType<PirateMinionBuff>();
		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (10, 14),
			[GroundAnimationState.JUMPING] = (14, 14),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (0, 4),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("PricklyPear");
			Main.projFrames[projectile.type] = 15;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 26;
			drawOffsetX = -2;
			drawOriginOffsetY = -14;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 7;
			int xMaxSpeed = 10;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if (animationFrame - lastHitFrame > 10)
			{
				projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
			else
			{
				projectile.velocity.X = Math.Sign(projectile.velocity.X) * xMaxSpeed * 0.75f;
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(!gHelper.isFlying && vectorToTarget is Vector2 target && target.Length() < 48)
			{
				if(gHelper.didJustLand)
				{
					base.Animate(4, 7);
				} else
				{
					base.Animate(7, 10);
				}
			} else
			{
				GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			}
		}
	}
}
