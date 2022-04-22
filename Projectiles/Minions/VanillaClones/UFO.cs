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
	public class UFOMinionBuff : MinionBuff
	{
		public UFOMinionBuff() : base(ProjectileType<UFOMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.UFOMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.UFOMinion"));
		}
	}

	public class UFOMinionItem : VanillaCloneMinionItem<UFOMinionBuff, UFOMinion>
	{
		internal override int VanillaItemID => ItemID.XenoStaff;

		internal override string VanillaItemName => "XenoStaff";
	}

	public class UfoDamageHitbox : ModProjectile
	{
		public override string Texture => "Terraria/Images/Item_0";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}
	}


	public class UFOMinion : HoverShooterMinion
	{
		internal override int BuffId => BuffType<UFOMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.UFOMinion;

		internal int baseSpeed = 14;
		internal int baseInertia = 10;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.UFOMinion") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 4;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			DrawOffsetX = (Projectile.width - 44) / 2;
			attackFrames = 22;
			targetSearchDistance = 900;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 14;
			hsHelper.targetInnerRadius = 200;
			hsHelper.targetOuterRadius = 240;
			hsHelper.targetShootProximityRadius = 196;
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
			float boundedX = Math.Sign(Projectile.velocity.X) * Math.Min(16, Math.Abs(Projectile.velocity.X));
			Projectile.rotation = boundedX * 0.05f;
		}

		private void updateDust(int dustId)
		{
			if (Main.rand.NextBool(2))
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

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// "teleport" functionality (but not really since it's just moving fast)
			float teleportLength = 1.5f * hsHelper.targetOuterRadius;
			if(vectorToTargetPosition.LengthSquared() > teleportLength * teleportLength)
			{
				int speedMult = 4;
				hsHelper.travelSpeed = speedMult * baseSpeed;
				hsHelper.inertia = 1;
				Vector2 stepVector = vectorToTargetPosition;
				stepVector.SafeNormalize();
				for(int i = 0; i < hsHelper.travelSpeed; i += baseSpeed / 2)
				{
					Vector2 posVector = Projectile.Center + stepVector * i;
					int dustId = Dust.NewDust(posVector, 24, 24, 160);
					updateDust(dustId);
				}
			} else
			{
				hsHelper.travelSpeed = baseSpeed;
				if(Projectile.velocity.LengthSquared() < baseSpeed)
				{
					hsHelper.inertia = baseInertia;
				}
			}
			base.TargetedMovement(vectorToTargetPosition);
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
					Projectile.GetSource_FromThis(),
						target.Center,
						Vector2.Zero,
						ProjectileType<UfoDamageHitbox>(),
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer);
				}
				Vector2 targetVector = target.Center - Projectile.Center;
				Vector2 stepVector = targetVector;
				stepVector.Normalize();

				for(int i = 12; i < targetVector.Length(); i++)
				{
					Vector2 posVector = Projectile.Center + stepVector * i;
					int dustId = Dust.NewDust(posVector, 1, 1, 160);
					updateDust(dustId);
				}

				
			}
		}
	}
}
