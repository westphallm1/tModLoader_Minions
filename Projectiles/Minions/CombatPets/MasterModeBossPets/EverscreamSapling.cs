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
	public class EverscreamSaplingMinionBuff : CombatPetVanillaCloneBuff
	{
		public EverscreamSaplingMinionBuff() : base(ProjectileType<EverscreamSaplingMinion>()) { }

		public override int VanillaBuffId => BuffID.EverscreamPet;

		public override string VanillaBuffName => "EverscreamPet";
	}

	public class EverscreamSaplingMinionItem : CombatPetMinionItem<EverscreamSaplingMinionBuff, EverscreamSaplingMinion>
	{
		internal override int VanillaItemID => ItemID.EverscreamPetItem;

		internal override string VanillaItemName => "EverscreamPetItem";
	}

	public class EverscreamSaplingOrnament : WeakPumpkinBomb
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.OrnamentFriendly;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.frame = Main.rand.Next(4);
		}

		public override void Kill(int timeLeft)
		{
			// TODO dust
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
			for (int i = 0; i < 10; i++)
			{
				int dustType = 90 - Projectile.frame;
				int dustIdx = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
				Main.dust[dustIdx].noLight = true;
				Main.dust[dustIdx].scale = 0.8f;
			}
		}
	}

	public class EverscreamSaplingMinion : CombatPetGroundedRangedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EverscreamPet;
		internal override int BuffId => BuffType<EverscreamSaplingMinionBuff>();

		internal override int? ProjId => ProjectileType<EverscreamSaplingOrnament>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.EverscreamSapling"));
			Main.projFrames[Projectile.type] = 11;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			// DrawOffsetX = -2;
			DrawOriginOffsetY = -26;
			attackFrames = 12;
			frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
			{
				[GroundAnimationState.FLYING] = (8, 11),
				[GroundAnimationState.JUMPING] = (1, 1),
				[GroundAnimationState.STANDING] = (0, 0),
				[GroundAnimationState.WALKING] = (2, 8),
			};
		}
	}
}
