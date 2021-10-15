using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class SlimePrincessMinionBuff : CombatPetVanillaCloneBuff
	{
		public SlimePrincessMinionBuff() : base(ProjectileType<SlimePrincessMinion>()) { }

		public override int VanillaBuffId => BuffID.QueenSlimePet;

		public override string VanillaBuffName => "QueenSlimePet";
	}

	public class SlimePrincessMinionItem : CombatPetMinionItem<SlimePrincessMinionBuff, SlimePrincessMinion>
	{
		internal override int VanillaItemID => ItemID.QueenSlimePetItem;

		internal override string VanillaItemName => "QueenSlimePetItem";
	}

	public class SlimePrincessMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.QueenSlimePet;
		internal override int BuffId => BuffType<SlimePrincessMinionBuff>();

		private bool wasFlyingThisFrame = false;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.QueenSlimePet") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 12;
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 30;
			Projectile.height = 30;
			DrawOriginOffsetY = -4;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			if(!wasFlyingThisFrame && gHelper.isFlying)
			{
				Gore.NewGore(Projectile.Center, Vector2.Zero, GoreID.QueenSlimePetCrown);
			}
			wasFlyingThisFrame = gHelper.isFlying;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = gHelper.isFlying ? 6 : 0;
			maxFrame = gHelper.isFlying ? 12 : 6;
			if(gHelper.isFlying)
			{
				Projectile.rotation = Projectile.velocity.X * 0.05f;
			} else
			{
				Projectile.rotation = 0;
			}
			base.Animate(minFrame, maxFrame);
		}
	}
}
