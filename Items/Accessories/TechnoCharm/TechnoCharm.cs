using AmuletOfManyMinions.Items.Accessories.SquireSkull;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.TechnoCharm
{
	class TechnoCharmAccessory : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Enchants your squire with a mechanical skull!\n" +
				"Increases squire damage and adds a rotating debuff to squire attacks.");
			DisplayName.SetDefault("Techno Pendant");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SoulofFright, 10);
			recipe.AddIngredient(ItemID.SummonerEmblem, 1);
			recipe.AddIngredient(ItemType<SquireSkullAccessory>(), 1);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().squireTechnoSkullAccessory = true;
			player.GetModPlayer<SquireModPlayer>().squireAttackSpeedMultiplier *= 0.90f;
			player.GetModPlayer<SquireModPlayer>().squireDamageMultiplierBonus += 0.12f;
		}

		public override bool CanEquipAccessory(Player player, int slot)
		{
			// don't allow side by side with squire skull, so their debuffs don't overwrite each other
			int skullType = ItemType<SquireSkullAccessory>();
			return slot > 9 || !player.armor.Skip(3).Take(5 + player.extraAccessorySlots).Any(a=>!a.IsAir && a.type == skullType);
		}
	}

	class TechnoCharmProjectile : SquireAccessoryMinion
	{

		int DebuffCycleFrames = 360;
		int AnimationFrames = 120;

		public override void SetStaticDefaults()
		{
			Main.projFrames[projectile.type] = 8;
		}

		private int debuffCycle => (animationFrame % DebuffCycleFrames) / (DebuffCycleFrames / 3);


		public override Vector2 IdleBehavior()
		{
			Vector2 idleVector = base.IdleBehavior();
			if(debuffCycle == 0)
			{
				squirePlayer.squireDebuffOnHit = BuffID.Frostburn;
				squirePlayer.squireDebuffTime = 180;
				Lighting.AddLight(projectile.position, Color.Cyan.ToVector3() * 0.33f);
			} else if (debuffCycle == 1)
			{
				squirePlayer.squireDebuffOnHit = BuffID.Ichor;
				squirePlayer.squireDebuffTime = 60;
				Lighting.AddLight(projectile.position, Color.Gold.ToVector3() * 0.33f);
			} else
			{
				squirePlayer.squireDebuffOnHit = BuffID.CursedInferno;
				squirePlayer.squireDebuffTime = 180;
				Lighting.AddLight(projectile.position, Color.LimeGreen.ToVector3() * 0.33f);
			}
			int angleFrame = animationFrame % AnimationFrames;
			float angle = 2 * (float)(Math.PI * angleFrame) / AnimationFrames;
			Vector2 angleVector = 32 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			return idleVector + angleVector;
		}

		protected override bool IsEquipped(SquireModPlayer player)
		{
			return player.squireTechnoSkullAccessory;
		}
	}
}
