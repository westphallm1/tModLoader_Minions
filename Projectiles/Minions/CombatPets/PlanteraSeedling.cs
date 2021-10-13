using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets
{
	public class PlanteraSeedlingMinionBuff : CombatPetVanillaCloneBuff
	{
		public PlanteraSeedlingMinionBuff() : base(ProjectileType<PlanteraSeedlingMinion>()) { }

		public override int VanillaBuffId => BuffID.PlanteraPet;

		public override string VanillaBuffName => "PlanteraPet";
	}

	public class PlanteraSeedlingMinionItem : CombatPetMinionItem<PlanteraSeedlingMinionBuff, PlanteraSeedlingMinion>
	{
		internal override int VanillaItemID => ItemID.PlanteraPetItem;

		internal override string VanillaItemName => "PlanteraPetItem";
	}

	public class PlanteraSeedlingSeed : StingerProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PoisonSeedPlantera;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 2;
		}

		public override void AI()
		{
			base.AI();
			Projectile.frame = Projectile.timeLeft % 10 < 5 ? 0 : 1;
		}
	}

	public class PlanteraSeedlingMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PlanteraPet;
		internal override int BuffId => BuffType<PlanteraSeedlingMinionBuff>();

		// fire a spike ball instead every 4th projectile
		int fireCount;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.PlanteraSeedling"));
			Main.projFrames[Projectile.type] = 12;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			// DrawOffsetX = -2;
			DrawOriginOffsetY = -16;
			attackFrames = 12;
			frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
			{
				[GroundAnimationState.FLYING] = (9, 12),
				[GroundAnimationState.JUMPING] = (1, 1),
				[GroundAnimationState.STANDING] = (0, 0),
				[GroundAnimationState.WALKING] = (0, 8),
			};
		}

		public override void LaunchProjectile(Vector2 launchVector)
		{
			int projId = ProjectileType<PlanteraSeedlingSeed>();
			Projectile.NewProjectile(
				Projectile.GetProjectileSource_FromThis(),
				Projectile.Center,
				VaryLaunchVelocity(launchVector),
				projId,
				Projectile.damage,
				Projectile.knockBack,
				player.whoAmI,
				ai0: Projectile.whoAmI);
		}
	}
}
