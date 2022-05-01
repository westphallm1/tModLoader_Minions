using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class TikiSpiritMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<TikiSpiritMinion>() };
		public override string VanillaBuffName => "TikiSpirit";
		public override int VanillaBuffId => BuffID.TikiSpirit;
	}

	public class TikiSpiritMinionItem : CombatPetMinionItem<TikiSpiritMinionBuff, TikiSpiritMinion>
	{
		internal override string VanillaItemName => "TikiTotem";
		internal override int VanillaItemID => ItemID.TikiTotem;
	}

	public class TikiSpiritMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<TikiSpiritMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.TikiSpirit;
		internal override int? FiredProjectileId => ProjectileType<PygmySpear>();
		internal override LegacySoundStyle ShootSound => new LegacySoundStyle(2, 17);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			hsHelper.CustomFireProjectile = FireProjectile;
		}

		private void FireProjectile(Vector2 target, int projType, float ai0)
		{
			hsHelper.FireProjectile(target, projType, targetNPCIndex ?? -1);
		}
	}
}
