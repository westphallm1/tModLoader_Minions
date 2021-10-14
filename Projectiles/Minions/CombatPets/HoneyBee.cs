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

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	public class HoneyBeeMinionBuff : CombatPetVanillaCloneBuff
	{
		public HoneyBeeMinionBuff() : base(ProjectileType<HoneyBeeMinion>()) { }

		public override int VanillaBuffId => BuffID.QueenBeePet;

		public override string VanillaBuffName => "QueenBeePet";
	}

	public class HoneyBeeMinionItem : CombatPetMinionItem<HoneyBeeMinionBuff, HoneyBeeMinion>
	{
		internal override int VanillaItemID => ItemID.QueenBeePetItem;

		internal override string VanillaItemName => "QueenBeePetItem";
	}

	public class HoneyPotProjectile : WeakPumpkinBomb
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
			Projectile.penetrate = 2;
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


	public class HoneyBeeMinion : CombatPetHoverShooterMinion
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
	}
}
