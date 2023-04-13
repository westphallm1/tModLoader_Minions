using AmuletOfManyMinions.Items.Accessories.TechnoCharm;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.SquireSkull
{
	class SquireSkullAccessory : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().squireSkullAccessory = true;
			player.GetModPlayer<SquireModPlayer>().SquireAttackSpeedMultiplier *= 0.95f;
			player.GetModPlayer<SquireModPlayer>().squireDamageMultiplierBonus += 0.08f;
		}
		public override bool CanEquipAccessory(Player player, int slot, bool modded)
		{
			// don't allow side by side with squire skull, so their debuffs don't overwrite each other
			int skullType = ItemType<TechnoCharmAccessory>();
			return !modded && slot > 9 || !player.armor.Skip(3).Take(5 + player.GetAmountOfExtraAccessorySlotsToShow()).Any(a => !a.IsAir && a.type == skullType);
		}
	}

	class SquireSkullProjectile : SquireAccessoryMinion
	{

		int DebuffCycleFrames = 360;
		int AnimationFrames = 120;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 24;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
		}

		private int debuffCycle => (animationFrame % DebuffCycleFrames) / (DebuffCycleFrames / 3);


		public override Vector2 IdleBehavior()
		{
			Vector2 idleVector = base.IdleBehavior();
			if (debuffCycle == 0)
			{
				squirePlayer.squireDebuffOnHit = BuffID.Bleeding;
				squirePlayer.squireDebuffTime = 180;
				Lighting.AddLight(Projectile.position, Color.Red.ToVector3() * 0.25f);
			}
			else if (debuffCycle == 1)
			{
				squirePlayer.squireDebuffOnHit = BuffID.OnFire;
				squirePlayer.squireDebuffTime = 180;
				Lighting.AddLight(Projectile.position, Color.Orange.ToVector3() * 0.25f);
			}
			else
			{
				squirePlayer.squireDebuffOnHit = BuffID.Poisoned;
				squirePlayer.squireDebuffTime = 180;
				Lighting.AddLight(Projectile.position, Color.Aquamarine.ToVector3() * 0.25f);
			}
			int angleFrame = animationFrame % AnimationFrames;
			float angle = 2 * (float)(Math.PI * angleFrame) / AnimationFrames;
			Vector2 angleVector = 32 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			return idleVector + angleVector;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (debuffCycle == 0)
			{

				minFrame = 0;
				maxFrame = 8;
			}
			else if (debuffCycle == 1)
			{
				minFrame = 8;
				maxFrame = 16;
			}
			else
			{
				minFrame = 16;
				maxFrame = 24;
			}
			base.Animate(minFrame, maxFrame);
		}

		protected override bool IsEquipped(SquireModPlayer player)
		{
			return player.squireSkullAccessory;
		}
	}
}
