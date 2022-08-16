using static AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses.CombatPetConvenienceMethods;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Items.Accessories;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd
{
	public class FlinxMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.FlinxMinion;

		internal override int[] ProjectileTypes => new int[] { ProjectileType<FlinxMinion>(), ProjectileType<BonusFlinxMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.FlinxMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.FlinxMinion"));
		}

	}

	public class FlinxMinionItem : VanillaCloneMinionItem<FlinxMinionBuff, FlinxMinion>
	{
		internal override int VanillaItemID => ItemID.FlinxStaff;

		internal override string VanillaItemName => "FlinxStaff";
	}

	public abstract class BaseFlinxMinion : SimpleGroundBasedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FlinxMinion;
		public override int BuffId => BuffType<FlinxMinionBuff>();
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(this, 24, 30, -8, -8);
			ConfigureFrames(12, (0, 0), (2, 11), (1, 1), (1, 1));
			xMaxSpeed = 8;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GHelper.DoGroundAnimation(frameInfo, base.Animate);
			DoSimpleFlyingDust();
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			DoDefaultGroundedMovement(vector);
		}
	}
	public class FlinxMinion : BaseFlinxMinion
	{

	}

	public class BonusFlinxMinion : BaseFlinxMinion
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
		}

		public override bool CheckActive()
		{
			if (base.CheckActive() && !Player.GetModPlayer<MinionSpawningItemPlayer>().flinxArmorSetEquipped)
			{
				Projectile.Kill();
				return false;
			}

			return true;
		}
	}
}
