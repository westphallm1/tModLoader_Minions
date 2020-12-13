using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.SquireBat
{
	class SquireBatBuff: ModBuff
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Feral Bite (Squire)");
			Description.SetDefault("A Feral Bite is enhancing your squire's attack and movement speed!");
			Main.debuff[Type] = true; // can't cancel it, even if it's a 'buff'
			Main.buffNoSave[Type] = true;
			SquireGlobalProjectile.squireBuffTypes.Add(Type);
		}
	}
	class SquireBatDebuff: ModBuff
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Feral Bite (Squire)");
			Description.SetDefault("A Feral Bite is reducing your squire's damage!");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			SquireGlobalProjectile.squireDebuffTypes.Add(Type);
		}
	}
	class SquireBatAccessory : ModItem
	{
		internal static int BuffTime = 60 * 8;
		internal static int DebuffTime = 60 * 5;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Summons a helpful bat to afflict your squire with a feral bite!\n" +
				"Greatly increases attack and move speed for 15 seconds, then reduces damage for 10 seconds.\n" +
				"Use <Activate Set Bonus> to activate.");
			DisplayName.SetDefault("Vial of Feral Blood");
		}

		public override void SetDefaults()
		{
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.LightRed;
			
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<SquireModPlayer>().squireBatAccessory = true;
		}
	}

	class SquireBatProjectile : SquireAccessoryMinion
	{

		int AnimationFrames = 60;

		public override void SetStaticDefaults()
		{
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 28;
			projectile.height = 32;
			frameSpeed = 5;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			if(squire == null)
			{
				return Vector2.Zero;
			}
			int angleFrame = animationFrame % AnimationFrames;
			float angle = 2 * (float)(Math.PI * angleFrame) / AnimationFrames;
			float radius = 28;
			int buffType = BuffType<SquireBatBuff>();
			int debuffType = BuffType<SquireBatDebuff>();
			if (player.HasBuff(buffType))
			{
				int buffTime = player.buffTime[player.FindBuffIndex(buffType)];
				int buffFrame = SquireBatAccessory.BuffTime - buffTime;
				if (buffFrame < 30)
				{
					radius = 28 - 20 * buffFrame / 30f;
				} else if (buffFrame < 60)
				{
					radius = 28 - 20 * (60 - buffFrame) / 30f;
				}
			} 
			Vector2 angleVector = radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			return (squire.Center + angleVector) - projectile.Center;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			projectile.spriteDirection = animationFrame % AnimationFrames < AnimationFrames / 2 ?
				1 : -1;
			base.Animate(minFrame, maxFrame);
		}

		protected override bool IsEquipped(SquireModPlayer player)
		{
			return player.squireBatAccessory;
		}
	}
}
