using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones.Pirate;
using AmuletOfManyMinions.Core.BackportUtils;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class CinderHenMinionBuff : CombatPetBuff
	{
		public CinderHenMinionBuff() : base(ProjectileType<CinderHenMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Cinder Hen");
			Description.SetDefault("A tiny phoenix has joined your adventure!");
		}

	}

	public class CinderHenMinionItem : CombatPetCustomMinionItem<CinderHenMinionBuff, CinderHenMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bold Bow of Friendship");
			Tooltip.SetDefault("Summons a pet Cinder Hen!");
		}
	}

	internal class FlareVortexDebuff : BackportModBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.OnFire;
		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
		}
	}

	public class FlareVortexProjectile : BaseImpFireball
	{
		internal static int TimeToLive = 180;

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = TimeToLive;
			Projectile.penetrate = 1;
		}

		public override void PostAI()
		{
			int frame = TimeToLive - Projectile.timeLeft;
			float baseAngle = -MathHelper.TwoPi * frame / 30f;
			int radius = Math.Min(20, frame/ 2);
			for(float offset = 0; offset < MathHelper.TwoPi; offset += 2 * MathHelper.Pi / 3f)
			{
				float angle = baseAngle + offset;
				Vector2 dustPos = Projectile.Center + radius * angle.ToRotationVector2();
				AddDust(dustPos, Projectile.width, Projectile.height);
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffType<FlareVortexDebuff>(), 240);
			DebuffGlobalNPC debuffNPC = target.GetGlobalNPC<DebuffGlobalNPC>();
			debuffNPC.flameVortexStack = (short)Math.Min(debuffNPC.flameVortexStack + 1, 5);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = 1;
		}

		public override void Kill(int timeLeft)
		{
			// just a flame effect
			for (int i = 0; i < 10; i++)
			{
				int dustIdx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100, default, 2f);
				Main.dust[dustIdx].noGravity = true;
				Main.dust[dustIdx].velocity *= 5f;
				dustIdx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100, default, 1.25f);
				Main.dust[dustIdx].velocity *= 2.5f;
			}
			if(Projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(
					Projectile.Center,
					Vector2.Zero,
					ProjectileType<PirateCannonballExplosion>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner);
			}
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			hitbox.Inflate(16, 16);
		}

	}

	public class CinderHenMinion : CombatPetGroundedRangedMinion
	{
		internal override int BuffId => BuffType<CinderHenMinionBuff>();
		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Spectre ? 
			ProjectileType<FlareVortexProjectile>() :
			ProjectileType<ImpFireball>();

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -4, -6, -1);
			ConfigureFrames(14, (0, 1), (2, 9), (2, 2), (10, 13));
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = gHelper.GetAnimationState();
			frameSpeed = (state == GroundAnimationState.STANDING) ? 10 : 5;
			base.Animate(minFrame, maxFrame);
		}
	}
}
