using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using AmuletOfManyMinions.Core.Minions.Effects;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BlackCatMinionBuff : CombatPetVanillaCloneBuff
	{
		public BlackCatMinionBuff() : base(ProjectileType<BlackCatMinion>()) { }
		public override int VanillaBuffId => BuffID.BlackCat;
		public override string VanillaBuffName => "BlackCat";
	}

	public class BlackCatMinionItem : CombatPetMinionItem<BlackCatMinionBuff, BlackCatMinion>
	{
		internal override int VanillaItemID => ItemID.UnluckyYarn;
		internal override string VanillaItemName => "UnluckyYarn";
	}

	public abstract class BlackCatRicochetProjectile : ModProjectile
	{

		int bouncesLeft;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.timeLeft = 180;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.penetrate = 10;
			Projectile.tileCollide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			bouncesLeft = 6;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Math.Abs(Projectile.velocity.Y) < Math.Abs(oldVelocity.Y))
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			} else if (Math.Abs(Projectile.velocity.X) < Math.Abs(oldVelocity.X))
			{
				Projectile.velocity.X = -oldVelocity.X;
			} else
			{
				// don't really understand what's going on in this case but that's ok
				return false; 
			}
			return !(bouncesLeft-- > 0);
		}
	}

	public class BlackCatWaterBolt : BlackCatRicochetProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WaterBolt;

		public override void AI()
		{
			for (int i = 0; i < 5; i++)
			{
				Vector2 dustOffset = i * Projectile.velocity / 3f;
				int dustId = Dust.NewDust(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.DungeonWater, Scale: 1.2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity *= 0.1f;
				Main.dust[dustId].position -= dustOffset;
			}
		}

		public override void Kill(int timeLeft)
		{
			for(int i = 0; i < 30; i++)
			{
				int dustId = Dust.NewDust(
					Projectile.position, Projectile.width, Projectile.height, 
					DustID.DungeonWater, Projectile.velocity.X / 10, Projectile.velocity.Y / 10, 100);
				Main.dust[dustId].noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}

	public class BlackCatMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BlackCat;
		internal override int BuffId => BuffType<BlackCatMinionBuff>();

		// scale attack type rather than attack speed
		internal override int GetAttackFrames(CombatPetLevelInfo info) => Math.Max(45, 60 - 4 * info.Level);

		internal override int? ProjId => ProjectileType<BlackCatWaterBolt>();

		internal WeaponHoldingDrawer weaponDrawer;
		public override void LoadAssets()
		{
			Main.instance.LoadItem(ItemID.WaterBolt);
			Main.instance.LoadItem(ItemID.MagicalHarp);
			Main.instance.LoadItem(ItemID.ShadowbeamStaff);
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			SetAttackStyleSpecificBehaviors();
			weaponDrawer.Update(Projectile, animationFrame);
			return target;
		}

		private void SetAttackStyleSpecificBehaviors()
		{
			launchVelocity = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 24, -20, -16, -1);
			ConfigureFrames(11, (0, 0), (1, 5), (1, 1), (6, 10));
			weaponDrawer = new WeaponHoldingDrawer()
			{
				WeaponOffset = Vector2.Zero,
				WeaponHoldDistance = 16,
				ForwardDir = -1,
			};
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if(animationFrame == lastFiredFrame)
			{
				weaponDrawer.StartAttack(vectorToTargetPosition);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Item[ItemID.WaterBolt].Value;
			weaponDrawer.Draw(texture, lightColor);
			return true;
		}
	}
}
