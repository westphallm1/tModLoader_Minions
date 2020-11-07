using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Squires
{
	public abstract class SquireMinionItem<TBuff, TProj> : MinionItem<TBuff, TProj> where TBuff : ModBuff where TProj : SquireMinion<TBuff>
	{

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.autoReuse = true;
			item.useStyle = ItemUseStyleID.HoldingUp;
			item.channel = true;
			item.noUseGraphic = false;
			item.mana = 0;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			if (player.ownedProjectileCounts[item.shoot] > 0 || player.altFunctionUse == 2)
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
			player.AddBuff(item.buffType, 2);
			Projectile.NewProjectile(player.Center, Vector2.Zero, item.shoot, damage, item.knockBack, player.whoAmI);
			return false;
		}

		public override bool CanUseItem(Player player)
		{
			base.CanUseItem(player);
			if (player.ownedProjectileCounts[item.shoot] > 0)
			{
				item.UseSound = null;
				item.noUseGraphic = true;
				item.useStyle = ItemUseStyleID.HoldingOut;
			}
			else
			{
				SetDefaults();
			}
			return true;
		}
	}

}
