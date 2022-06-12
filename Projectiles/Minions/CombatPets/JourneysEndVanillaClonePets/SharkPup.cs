using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using System;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
{
	public class SharkPupMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SharkPupMinion>() };
		public override string VanillaBuffName => "SharkPup";
		public override int VanillaBuffId => BuffID.SharkPup;
	}

	public class SharkPupMinionItem : CombatPetMinionItem<SharkPupMinionBuff, SharkPupMinion>
	{
		internal override string VanillaItemName => "SharkBait";
		internal override int VanillaItemID => ItemID.SharkBait;
	}

	public class SharkPupBubble : BaseMinionBubble
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SeaSquire/SeaSquireBubble";
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		public override void AI()
		{
			base.AI();
			int speed = 12;
			int inertia = 30;
			if(Projectile.timeLeft < 150 && 
				Minion.GetClosestEnemyToPosition(Projectile.Center, 200f, requireLOS: true) is NPC target)
			{
				Vector2 targetVector = target.Center - Projectile.Center;
				targetVector.SafeNormalize();
				targetVector *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + targetVector) / inertia;
			}
		}
	}

	public class SharkPupMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<SharkPupMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SharkPup;
		internal override int? FiredProjectileId => ProjectileType<SharkPupBubble>();
		internal override SoundStyle? ShootSound => SoundID.Item17;

		internal override int GetProjectileVelocity(ICombatPetLevelInfo info) => Math.Min(8, 4 + info.Level);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			forwardDir = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(4, 8);
		}

	}
}
