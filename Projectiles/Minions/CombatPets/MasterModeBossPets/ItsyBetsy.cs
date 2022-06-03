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
	public class ItsyBetsyMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<ItsyBetsyMinion>() };

		public override int VanillaBuffId => BuffID.DD2BetsyPet;

		public override string VanillaBuffName => "DD2BetsyPet";
	}
	public class ItsyBetsyMinionItem : CombatPetMinionItem<ItsyBetsyMinionBuff, ItsyBetsyMinion>
	{
		internal override int VanillaItemID => ItemID.DD2BetsyPetItem;

		internal override string VanillaItemName => "DD2BetsyPetItem";
	}

	public class ItsyBetsyFire : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EyeFire;
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Flames);
			Projectile.aiStyle = 0; // unset default flames AI
			base.SetDefaults();
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.timeLeft = 42;
		}
		public override void AI()
		{
			base.AI();
			Projectile.friendly = Projectile.ai[0] == 0;
			Projectile.localAI[0]++;
			if(Projectile.localAI[0] < 8 || !Main.rand.NextBool(2))
			{
				return;
			}
			float dustScale = Math.Min(1, 0.25f * (Projectile.localAI[0] - 7));
			int dustType = 6;
			int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100);
			Main.dust[dustId].scale *= 1.5f * dustScale;
			Main.dust[dustId].velocity.X *= 1.2f;
			Main.dust[dustId].velocity.Y *= 1.2f;
			Main.dust[dustId].noGravity = true;
			if (Main.rand.NextBool(3))
			{
				Main.dust[dustId].scale *= 1.25f;
				Main.dust[dustId].velocity.X *= 2f;
				Main.dust[dustId].velocity.Y *= 2f;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 300);
		}
	}

	public class ItsyBetsyMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<ItsyBetsyMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2BetsyPet;
		internal override int? FiredProjectileId => ProjectileType<ItsyBetsyFire>();

		internal override int GetProjectileVelocity(ICombatPetLevelInfo info) => 6;
		internal override SoundStyle? ShootSound => SoundID.Item34 with { Volume = 0.5f };

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.ItsyBetsy"));
			Main.projFrames[Projectile.type] = 12;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			int attackCycleFrame = animationFrame - hsHelper.lastShootFrame;
			if(attackCycleFrame < attackFrames / 2 && attackFrames % 6 == 0)
			{
				Vector2 lineOfFire = vectorToTargetPosition;
				lineOfFire.SafeNormalize();
				lineOfFire *= hsHelper.projectileVelocity;
				lineOfFire += Projectile.velocity / 3;
				if(player.whoAmI == Main.myPlayer)
				{
					hsHelper.FireProjectile(lineOfFire, (int)FiredProjectileId, attackCycleFrame % 18);
				}
				AfterFiringProjectile();
			}
		}
	}
}
