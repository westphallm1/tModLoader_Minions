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
	public class AlienSkaterMinionBuff : CombatPetVanillaCloneBuff
	{
		public AlienSkaterMinionBuff() : base(ProjectileType<AlienSkaterMinion>()) { }

		public override int VanillaBuffId => BuffID.MartianPet;

		public override string VanillaBuffName => "MartianPet";
	}

	public class AlienSkaterMinionItem : CombatPetMinionItem<AlienSkaterMinionBuff, AlienSkaterMinion>
	{
		internal override int VanillaItemID => ItemID.MartianPetItem;

		internal override string VanillaItemName => "MartianPetItem";
	}

	public class AlienSkaterMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MartianPet;
		internal override int BuffId => BuffType<AlienSkaterMinionBuff>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 14;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			DrawOriginOffsetY = -16;
			frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
			{
				[GroundAnimationState.FLYING] = (10, 14),
				[GroundAnimationState.JUMPING] = (1, 1),
				[GroundAnimationState.STANDING] = (0, 0),
				[GroundAnimationState.WALKING] = (2, 9),
			};
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
		}
	}
}
