using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class BabyEaterMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyEaterMinion>() };
		public override int VanillaBuffId => BuffID.BabyEater;
		public override string VanillaBuffName => "BabyEater";
	}

	public class BabyEaterMinionItem : CombatPetMinionItem<BabyEaterMinionBuff, BabyEaterMinion>
	{
		internal override int VanillaItemID => ItemID.EatersBone;
		internal override string VanillaItemName => "EatersBone";
	}

	public class BabyEaterMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<BabyEaterMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabyEater;
		internal override bool DoBumblingMovement => true;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 2;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 90;
			circleHelper.idleBumbleFrames = 90;
			frameSpeed = 10;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			Projectile.spriteDirection = 1;
			float targetRotation = Projectile.velocity.ToRotation();
			Projectile.rotation = targetRotation - MathHelper.PiOver2;
		}
	}
}
