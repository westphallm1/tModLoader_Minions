using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.BoneSerpent;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class EaterOfWormsMinionBuff : CombatPetVanillaCloneBuff
	{
		public EaterOfWormsMinionBuff() : base(ProjectileType<EaterOfWormsMinion>()) { }

		public override int VanillaBuffId => BuffID.EaterOfWorldsPet;

		public override string VanillaBuffName => "EaterOfWorldsPet";
	}

	public class EaterOfWormsMinionItem : CombatPetMinionItem<EaterOfWormsMinionBuff, EaterOfWormsMinion>
	{
		internal override int VanillaItemID => ItemID.EaterOfWorldsPetItem;

		internal override string VanillaItemName => "EaterOfWorldsPetItem";
	}

	public class EaterOfWormsMinion : CombatPetGroundedWormMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.EaterOfWorldsPet;
		internal override int BuffId => BuffType<EaterOfWormsMinionBuff>();
		public override int CounterType => -1;
		protected override int dustType => 135;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.EaterOfWorms") + " (AoMM Version)");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			wormDrawer = new EaterOfWormsDrawer();
		}
	}

	public class EaterOfWormsDrawer : VerticalWormDrawer
	{

		protected override void DrawHead()
		{
			AddSprite(2, new(0, 0, 28, 20));
		}

		protected override void DrawBody()
		{
			for (int i = 0; i < SegmentCount; i++)
			{
				AddSprite(18 + 14 * i, new(0, 32, 28, 16));
			}
		}

		protected override void DrawTail()
		{
			int dist = 18 + 14 * SegmentCount;
			lightColor = new Color(
				Math.Max(lightColor.R, (byte)25),
				Math.Max(lightColor.G, (byte)25),
				Math.Max(lightColor.B, (byte)25));
			AddSprite(dist, new(0, 60, 28, 22));
		}
	}
}
