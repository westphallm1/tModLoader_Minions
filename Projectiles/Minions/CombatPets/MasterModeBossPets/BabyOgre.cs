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
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class BabyOgreMinionBuff : CombatPetVanillaCloneBuff
	{
		public BabyOgreMinionBuff() : base(ProjectileType<BabyOgreMinion>()) { }

		public override int VanillaBuffId => BuffID.DD2OgrePet;

		public override string VanillaBuffName => "DD2OgrePet";
	}

	public class BabyOgreMinionItem : CombatPetMinionItem<BabyOgreMinionBuff, BabyOgreMinion>
	{
		internal override int VanillaItemID => ItemID.DD2OgrePetItem;

		internal override string VanillaItemName => "DD2OgrePetItem";
	}

	public class BabyOgreMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2OgrePet;
		internal override int BuffId => BuffType<BabyOgreMinionBuff>();

		// fire a spike ball instead every 4th projectile
		int fireCount;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.BabyOgre"));
			Main.projFrames[Projectile.type] = 14;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			// massive amount of whitespace around most frames
			DrawOriginOffsetY = -42;
			DrawOffsetX = -32;
			frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
			{
				[GroundAnimationState.FLYING] = (10, 14),
				[GroundAnimationState.JUMPING] = (1, 1),
				[GroundAnimationState.STANDING] = (0, 0),
				[GroundAnimationState.WALKING] = (2, 9),
			};
		}
	}
}
