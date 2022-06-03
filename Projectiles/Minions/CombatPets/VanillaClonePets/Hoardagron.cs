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
	public class HoardagronHeadMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<HoardagronHeadMinion>() };
		public override string VanillaBuffName => "PetDD2Dragon";
		public override int VanillaBuffId => BuffID.PetDD2Dragon;
	}

	public class HoardagronHeadMinionItem : CombatPetMinionItem<HoardagronHeadMinionBuff, HoardagronHeadMinion>
	{
		internal override string VanillaItemName => "DD2PetDragon";
		internal override int VanillaItemID => ItemID.DD2PetDragon;
	}

	public class HoardagronHeadMinion : CombatPetHoverDasherMinion
	{
		internal override int BuffId => BuffType<HoardagronHeadMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2PetDragon;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			IdleLocationSets.circlingHead.Add(Projectile.type);
			forwardDir = -1;
			Main.projFrames[Projectile.type] = 3;
		}
	}
}
