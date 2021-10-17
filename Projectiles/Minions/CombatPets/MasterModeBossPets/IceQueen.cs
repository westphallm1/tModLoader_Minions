using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
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

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class IceQueenMinionBuff : CombatPetVanillaCloneBuff
	{
		public IceQueenMinionBuff() : base(ProjectileType<IceQueenMinion>()) { }

		public override int VanillaBuffId => BuffID.IceQueenPet;

		public override string VanillaBuffName => "IceQueenPet";
	}

	public class IceQueenMinionItem : CombatPetMinionItem<IceQueenMinionBuff, IceQueenMinion>
	{
		internal override int VanillaItemID => ItemID.IceQueenPetItem;

		internal override string VanillaItemName => "IceQueenPetItem";
	}

	/// <summary>
	/// Uses AI[0] to determine the height to start colliding with tiles at
	/// </summary>
	public class IceQueenIcicle : ModProjectile
	{

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Blizzard;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.timeLeft = 60;
		}

		public override void AI()
		{
			base.AI();
			Projectile.tileCollide = Projectile.position.Y > Projectile.ai[0];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if(Main.rand.NextBool())
			{
				int idx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFairy);
				Main.dust[idx].noGravity = true;
				Main.dust[idx].velocity *= 0.75f;
			}
		}


	}


	public class IceQueenMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<IceQueenMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.IceQueenPet;
		internal override int? FiredProjectileId => ProjectileType<IceQueenIcicle>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;

		internal override int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(10, 20 - 2 * info.Level);
		internal override int GetProjectileVelocity(CombatPetLevelInfo info) => 10;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.IceQueen"));
			Main.projFrames[Projectile.type] = 16;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			frameSpeed = 5;
			hsHelper.CustomFireProjectile = LaunchIcicle;
		}

		private void LaunchIcicle(Vector2 lineOfFire, int projId, float ai0)
		{
			if(player.whoAmI == Main.myPlayer && targetNPCIndex is int idx)
			{
				int spawnCount = Main.rand.Next(2) + 1;
				for(int i = 0; i < spawnCount; i++)
				{
					Vector2 spawnAngle = Vector2.UnitY.RotatedBy(
						Main.rand.NextFloat(MathHelper.Pi / 4) - MathHelper.PiOver2 / 8);
					Vector2 spawnPos = Main.npc[idx].Top;
					float spawnY = spawnPos.Y;
					Projectile.NewProjectile(
						Projectile.GetProjectileSource_FromThis(),
						spawnPos - 128 * spawnAngle,
						VaryLaunchVelocity(hsHelper.projectileVelocity * spawnAngle),
						projId,
						Projectile.damage,
						Projectile.knockBack,
						Main.myPlayer,
						ai0: spawnY);
				}
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}
}
