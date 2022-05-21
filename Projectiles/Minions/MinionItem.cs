using AmuletOfManyMinions.Core.Minions.Pathfinding;
using AmuletOfManyMinions.UI;
using AmuletOfManyMinions.UI.TacticsUI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	public abstract class MinionItem<TBuff, TProj> : ModItem where TBuff : ModBuff where TProj : Minion
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
			UnifiedCrossModChanges();
		}

		public override void SetDefaults()
		{
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item44;
			// These below are needed for a minion weapon
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = BuffType<TBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			Item.shoot = ProjectileType<TProj>();
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;
		}

		protected void ApplyBuff(Player player)
		{
			player.AddBuff(Item.buffType, 3);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);

			var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			projectile.originalDamage = Item.damage; // using damage directly appears to double-dip into damage multipliers
			return false;
		}

		public override bool AltFunctionUse(Player player)
		{
			return false;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI)
			{
				return false;
			}
			return base.CanUseItem(player);
		}
		
		void UnifiedCrossModChanges()
		{
			CrossMod.HookBuffToItemCrossMod(BuffType<TBuff>(), Item.type);
			ApplyCrossModChanges();
		}
		
		public virtual void ApplyCrossModChanges() { }
	}
}
