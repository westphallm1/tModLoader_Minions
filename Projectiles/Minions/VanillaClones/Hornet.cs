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
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.HornetMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.HornetMinion"));
		}
	}

	public class HornetMinionItem : VanillaCloneMinionItem<HornetMinionBuff, HornetMinion>
	{
		internal override int VanillaItemID => ItemID.HornetStaff;

		internal override string VanillaItemName => "HornetStaff";

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.UseSound = new LegacySoundStyle(2, 76);
		}
	}

	public abstract class StingerProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.HornetStinger);
			base.SetDefaults();
		}

		public override void PostAI()
		{
			SpawnDust();
		}

		public virtual void SpawnDust()
		{
			if (Main.rand.Next(2) == 0)
			{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 18, 0f, 0f, 0, default, 0.9f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.5f;
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Poisoned, 300);
		}

	}

	public class HornetStinger : StingerProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HornetStinger;
	}

	public class HornetMinion : HoverShooterMinion
	{
		internal override int BuffId => BuffType<HornetMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Hornet;
		internal override int? FiredProjectileId => ProjectileType<HornetStinger>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Hornet"));
			Main.projFrames[Projectile.type] = 3;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			DrawOffsetX = (Projectile.width - 44) / 2;
			targetSearchDistance = 700;
			attackFrames = 50;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 9;
			hsHelper.projectileVelocity = 12;
			hsHelper.targetInnerRadius = 128;
			hsHelper.targetOuterRadius = 176;
			hsHelper.targetShootProximityRadius = 96;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type])
				{
					Projectile.frame = 0;
				}
			}
			if(vectorToTarget is Vector2 target)
			{
				Projectile.spriteDirection = -Math.Sign(target.X);
			}
			else if(Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = -1;
			}
			else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = 1;
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}
}
