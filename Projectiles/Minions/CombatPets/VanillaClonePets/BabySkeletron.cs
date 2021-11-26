using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabySkeletronHeadMinionBuff : CombatPetVanillaCloneBuff
	{
		public BabySkeletronHeadMinionBuff() : base(ProjectileType<BabySkeletronHeadMinion>()) { }
		public override string VanillaBuffName => "BabySkeletronHead";
		public override int VanillaBuffId => BuffID.BabySkeletronHead;
	}

	public class BabySkeletronHeadMinionItem : CombatPetMinionItem<BabySkeletronHeadMinionBuff, BabySkeletronHeadMinion>
	{
		internal override string VanillaItemName => "BoneKey";
		internal override int VanillaItemID => ItemID.BoneKey;
	}

	public class BabySkeletronHeadMinion : CombatPetHoverDasherMinion
	{
		internal override int BuffId => BuffType<BabySkeletronHeadMinionBuff>();
		public override string Texture => "Terraria/Projectile_" + ProjectileID.BabySkeletronHead;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(vectorToTarget is null)
			{
				Projectile.rotation = 0.05f * Projectile.velocity.X;
			} else
			{
				Projectile.rotation += MathHelper.TwoPi / 15 * Math.Sign(Projectile.velocity.X);
			}
		}

	}
}
