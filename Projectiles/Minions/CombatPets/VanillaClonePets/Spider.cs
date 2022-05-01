using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class SpiderPetMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SpiderPetMinion>() };
		public override string VanillaBuffName => "PetSpider";
		public override int VanillaBuffId => BuffID.PetSpider;
	}

	public class SpiderPetMinionItem : CombatPetMinionItem<SpiderPetMinionBuff, SpiderPetMinion>
	{
		internal override string VanillaItemName => "SpiderEgg";
		internal override int VanillaItemID => ItemID.SpiderEgg;
	}

	public class SpiderPetMinion : BaseSpiderMinion
	{
		internal override int BuffId => BuffType<SpiderPetMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Spider;

		private LeveledCombatPetModPlayer leveledPetPlayer;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 12;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			wallFrames = (5, 10);
			DrawOriginOffsetY = -24;
			frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
			{
				[GroundAnimationState.FLYING] = (11, 11),
				[GroundAnimationState.JUMPING] = (4, 4),
				[GroundAnimationState.STANDING] = (0, 0),
				[GroundAnimationState.WALKING] = (0, 4),
			};
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			leveledPetPlayer = player.GetModPlayer<LeveledCombatPetModPlayer>();
			Projectile.originalDamage = leveledPetPlayer.PetDamage;
			searchDistance = leveledPetPlayer.PetLevelInfo.BaseSearchRange;
			xMaxSpeed = (int)leveledPetPlayer.PetLevelInfo.BaseSpeed;
			return target;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if(gHelper.isFlying && !onWall && !isClinging && Projectile.velocity.LengthSquared() > 1)
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
		}
	}
}
