using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.Slimepire
{
	public class SlimepireMinionBuff : MinionBuff
	{
		public SlimepireMinionBuff() : base(ProjectileType<SlimepireMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Slimepire");
			Description.SetDefault("A vampire slime will fight for you!");
		}
	}

	public class SlimepireMinionItem : MinionItem<SlimepireMinionBuff, SlimepireMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimepire Staff");
			Tooltip.SetDefault("Summons a vampire slime to fight for you!\nIgnores 10 enemy defense");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 20;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 2, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}
	}

	public class SlimepireMinion : SimpleGroundBasedMinion
	{
		protected override int BuffId => BuffType<SlimepireMinionBuff>();
		private float intendedX = 0;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Slimepire");
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 20;
			projectile.height = 20;
			drawOffsetX = (projectile.width - 44) / 2;
			drawOriginOffsetY = (projectile.height - 32) / 2;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
			searchDistance = 825;
		}

		protected override bool DoPreStuckCheckGroundedMovement()
		{
			if (!gHelper.didJustLand)
			{
				projectile.velocity.X = intendedX;
				// only path after landing
				return false;
			}
			return true;
		}

		protected override bool CheckForStuckness()
		{
			return true;
		}
		protected override void DoGroundedMovement(Vector2 vector)
		{
			// always jump "long" if we're far away from the enemy
			if (Math.Abs(vector.X) > startFlyingAtTargetDist && vector.Y < -32)
			{
				vector.Y = -32;
			}
			gHelper.DoJump(vector);
			int maxHorizontalSpeed = vector.Y < -64 ? 4 : 8;
			projectile.velocity.X = Math.Max(1, Math.Min(maxHorizontalSpeed, Math.Abs(vector.X) / 16)) * Math.Sign(vector.X);
			intendedX = projectile.velocity.X;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = gHelper.isFlying ? 0 : 4;
			maxFrame = gHelper.isFlying ? 4 : 6;
			base.Animate(minFrame, maxFrame);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// manually bypass defense
			// this may not be wholly correct
			int defenseBypass = 10;
			int defense = Math.Min(target.defense, defenseBypass);
			damage += defense / 2;
		}
	}
}
