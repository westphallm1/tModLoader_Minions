using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class UFOMinionBuff : MinionBuff
	{
		public UFOMinionBuff() : base(ProjectileType<UFOMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("UFO");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class UFOMinionItem : MinionItem<UFOMinionBuff, UFOMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.XenoStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("UFO Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.XenoStaff);
			base.SetDefaults();
		}
	}

	public class UfoDamageHitbox : ModProjectile
	{
		public override string Texture => "Terraria/Item_0";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.penetrate = 1;
			projectile.tileCollide = false;
			projectile.friendly = true;
		}
	}


	public class UFOMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<UFOMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.UFOMinion;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying UFO");
			Main.projFrames[projectile.type] = 4;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			drawOffsetX = (projectile.width - 44) / 2;
			attackFrames = 30;
			targetSearchDistance = 900;
			hsHelper.travelSpeed = 14;
			hsHelper.targetInnerRadius = 200;
			hsHelper.targetOuterRadius = 240;
			hsHelper.targetShootProximityRadius = 196;
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
			projectile.rotation = projectile.velocity.X * 0.05f;
		}

		internal override void AfterFiringProjectile()
		{
			base.AfterFiringProjectile();
			if(targetNPCIndex is int idx)
			{
				NPC target = Main.npc[idx];
				if(Main.myPlayer == player.whoAmI)
				{
					Projectile.NewProjectile(
						target.Center,
						Vector2.Zero,
						ProjectileType<UfoDamageHitbox>(),
						projectile.damage,
						projectile.knockBack,
						Main.myPlayer);
				}
				Vector2 targetVector = target.Center - projectile.Center;
				Vector2 stepVector = targetVector;
				stepVector.Normalize();

				for(int i = 12; i < targetVector.Length(); i++)
				{
					Vector2 posVector = projectile.Center + stepVector * i;
					int dustId = Dust.NewDust(posVector, 1, 1, 160);
					if (Main.rand.Next(2) == 0)
					{
						Main.dust[dustId].color = Color.LimeGreen;
					}
					else
					{
						Main.dust[dustId].color = Color.CornflowerBlue;
					}
					Main.dust[dustId].scale = Main.rand.NextFloat(0.9f, 1.3f);
					Main.dust[dustId].velocity *= 0.2f;
				}

				
			}
		}
	}
}
