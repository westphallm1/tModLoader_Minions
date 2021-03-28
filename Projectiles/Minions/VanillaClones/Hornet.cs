using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class HornetMinionBuff : MinionBuff
	{
		public HornetMinionBuff() : base(ProjectileType<HornetMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.HornetMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.HornetMinion"));
		}
	}

	public class HornetMinionItem : VanillaCloneMinionItem<HornetMinionBuff, HornetMinion>
	{
		internal override int VanillaItemID => ItemID.HornetStaff;

		internal override string VanillaItemName => "HornetStaff";
	}

	public class HornetStinger : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.HornetStinger;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.HornetStinger);
			base.SetDefaults();
		}

		public override void PostAI()
		{
			if (Main.rand.Next(2) == 0)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 18, 0f, 0f, 0, default, 0.9f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.5f;
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Poisoned, 300);
		}
	}

	public class HornetMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<HornetMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.Hornet;
		internal override int? FiredProjectileId => ProjectileType<HornetStinger>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Hornet"));
			Main.projFrames[projectile.type] = 3;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void OnSpawn()
		{
			// vanilla version is a bit weak, so buff it
			projectile.damage = (int)(projectile.damage * 1.25f);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			drawOffsetX = (projectile.width - 44) / 2;
			targetSearchDistance = 600;
			attackFrames = 60;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 9;
			hsHelper.projectileVelocity = 12;
			hsHelper.targetInnerRadius = 128;
			hsHelper.targetOuterRadius = 176;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type])
				{
					projectile.frame = 0;
				}
			}
			if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			}
			else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
			projectile.rotation = projectile.velocity.X * 0.05f;
		}
	}
}
