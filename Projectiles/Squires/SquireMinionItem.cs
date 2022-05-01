using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Squires
{
	public abstract class SquireMinionItem<TBuff, TProj> : MinionItem<TBuff, TProj> where TBuff : ModBuff where TProj : SquireMinion
	{

		protected abstract string SpecialName { get; }
		protected virtual string SpecialDescription => null;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.channel = true;
			Item.noUseGraphic = false;
			Item.mana = 0;
		}

		public override bool CanShoot(Player player)
		{
			if (player.ownedProjectileCounts[Item.shoot] > 0 || player.altFunctionUse == 2)
			{
				return false;
			}
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);

			if (player.ownedProjectileCounts[Item.shoot] > 0 || player.altFunctionUse == 2)
			{
				return false;
			}

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && SquireMinionTypes.Contains(p.type))
				{
					p.Kill();
				}
			}

			var projectile = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, Item.shoot, damage, Item.knockBack, player.whoAmI);
			projectile.originalDamage = damage;
			return false;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.ownedProjectileCounts[Item.shoot] > 0)
			{
				Item.UseSound = null;
				Item.noUseGraphic = true;
				Item.useStyle = ItemUseStyleID.Shoot;
			}
			else
			{
				Item.useStyle = ItemUseStyleID.HoldUp;
				Item.noUseGraphic = false;
				Item.UseSound = SoundID.Item44;
			}
			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "SquireSpecialName", "Right-Click Special: " + SpecialName)
			{
				OverrideColor = Color.LimeGreen
			});
			if(SpecialDescription != null)
			{
				tooltips.Add(new TooltipLine(Mod, "SquireSpecialDescription", SpecialDescription));
			}
		}
	}

}
