using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	public class HoneyBeeMinionBuff : MinionBuff
	{
		public HoneyBeeMinionBuff() : base(ProjectileType<HoneyBeeMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.HoneyBeeMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.HoneyBeeMinion"));
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			base.Update(player, ref buffIndex);
			// CombatPetLevelTable.SpawnIfAbsent(player, buffIndex, projectileTypes[0], 14);
		}
	}

	public class HoneyBeeMinionItem : CombatPetMinionItem<HoneyBeeMinionBuff, HoneyBeeMinion>
	{
		internal override int VanillaItemID => ItemID.QueenBeePetItem;

		internal override string VanillaItemName => "QueenBeePetItem";

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 14;
		}
	}

	public class HoneyPotProjectile : BasePumpkinBomb
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			bounces = 3;
			Projectile.penetrate = 2;
		}
		protected override int TimeToLive => 120;

		protected override int FallAfterFrames => 15;

		protected override void OnFloorBounce(int bouncesLeft, Vector2 oldVelocity)
		{
			Projectile.velocity.Y = -3 * bouncesLeft;
			// make sure not to collide right away again
			Projectile.position.Y -= 8;
			Projectile.velocity.X *= 0.67f;
		}

		protected override void OnWallBounce(int bouncesLeft, Vector2 oldVelocity)
		{
			Projectile.velocity.X = -Math.Sign(oldVelocity.X) * 1.5f * bouncesLeft;
		}

		public override void Kill(int timeLeft)
		{
			Gore.NewGore(Projectile.position, Vector2.Zero, Mod.Find<ModGore>("HoneyPotBottomGore").Type);
			Gore.NewGore(Projectile.position, Vector2.Zero, Mod.Find<ModGore>("HoneyPotLidGore").Type);
			for(int i = 0; i < 3; i++)
			{
				int dustIdx = Dust.NewDust(Projectile.position, 32, 32, DustID.Honey2);
			}
		}
	}


	public class HoneyBeeMinion : HoverShooterMinion
	{
		internal override int BuffId => BuffType<HoneyBeeMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.QueenBeePet;
		internal override int? FiredProjectileId => ProjectileType<HoneyPotProjectile>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.HoneyBee"));
			Main.projFrames[Projectile.type] = 8;
			Main.projPet[Projectile.type] = true;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
			Projectile.width = 32;
			Projectile.height = 24;
			targetSearchDistance = 700;
			attackFrames = 80;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 9;
			hsHelper.projectileVelocity = 12;
			hsHelper.targetInnerRadius = 148;
			hsHelper.targetOuterRadius = 196;
			hsHelper.targetShootProximityRadius = 96;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
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
