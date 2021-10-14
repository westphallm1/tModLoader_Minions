using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
	public class TinyFishronMinionBuff : CombatPetVanillaCloneBuff
	{
		public TinyFishronMinionBuff() : base(ProjectileType<TinyFishronMinion>()) { }

		public override int VanillaBuffId => BuffID.DukeFishronPet;

		public override string VanillaBuffName => "DukeFishronPet";
	}

	public class TinyFishronMinionItem : CombatPetMinionItem<TinyFishronMinionBuff, MiniRetinazerMinion>
	{
		internal override int VanillaItemID => ItemID.DukeFishronPetItem;

		internal override string VanillaItemName => "DukeFishronPetItem";
	}

	public class TinyFishronBubble : BaseMinionBubble
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}
	}


	public class TinyFishronMinion : CombatPetHoverDasherMinion
	{
		internal override int BuffId => BuffType<TinyFishronMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DukeFishronPet;

		internal override int? FiredProjectileId => ProjectileType<TinyFishronBubble>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 90;
			circleHelper.idleBumbleFrames = 90;
			frameSpeed = 5;
			forwardDir = -1;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int framesSinceLastShot = animationFrame - hsHelper.lastShootFrame;
			int bubbleFrequency = (int)Math.Ceiling(attackFrames / 3f);
			hsHelper.projectileVelocity = 3;
			base.TargetedMovement(vectorToTargetPosition);
			if(player.whoAmI == Main.myPlayer && framesSinceLastShot > 0 && framesSinceLastShot % bubbleFrequency == 0)
			{
				Vector2 launchVector = vectorToTargetPosition;
				launchVector.SafeNormalize();
				launchVector *= hsHelper.projectileVelocity;
				hsHelper.FireProjectile(launchVector, (int)FiredProjectileId);
			} 
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = 0.05f * Projectile.velocity.X;
		}
	}
}
