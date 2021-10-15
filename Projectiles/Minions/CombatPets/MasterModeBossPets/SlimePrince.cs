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
	public class SlimePrinceMinionBuff : CombatPetVanillaCloneBuff
	{
		public SlimePrinceMinionBuff() : base(ProjectileType<SlimePrinceMinion>()) { }

		public override int VanillaBuffId => BuffID.KingSlimePet;

		public override string VanillaBuffName => "KingSlimePet";
	}

	public class SlimePrinceMinionItem : CombatPetMinionItem<SlimePrinceMinionBuff, SlimePrinceMinion>
	{
		internal override int VanillaItemID => ItemID.KingSlimePetItem;

		internal override string VanillaItemName => "KingSlimePetItem";
	}

	public class SlimePrinceMinion : CombatPetSlimeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.KingSlimePet;
		internal override int BuffId => BuffType<SlimePrinceMinionBuff>();

		private bool wasFlyingThisFrame = false;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.KingSlimePet") + " (AoMM Version)");
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
				Gore.NewGore(Projectile.Center, Vector2.Zero, GoreID.KingSlimePetCrown);
			}
			wasFlyingThisFrame = gHelper.isFlying;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			minFrame = gHelper.isFlying ? 6 : 0;
			maxFrame = gHelper.isFlying ? 12 : 6;
			if(gHelper.isFlying && Projectile.velocity.LengthSquared() > 2)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else
			{
				Projectile.rotation = 0;
			}
			base.Animate(minFrame, maxFrame);
		}
	}
}
