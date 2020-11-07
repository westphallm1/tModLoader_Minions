using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.SpiritCallerCharm
{
	class SpiritCallerCharm : NecromancerAccessory
	{
		public override string Texture => "Terraria/Item_" + ItemID.NecromanticScroll;
		protected override float baseDamage => 28;
		protected override int bossLifePerSpawn => 800;
		protected override int maxTransientMinions => 2;
		protected override float onKillChance => .2f;

		protected override int projType => ProjectileType<SpiritProjectile>();

		protected override float spawnVelocity => 6;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Charm of Spirits");
			Tooltip.SetDefault("Increases your minion damage by 8%\n" +
				"Spirits will rise from your fallen foes!");
		}

		public override void SetDefaults()
		{
			item.width = 32;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<NecromancerAccessoryPlayer>().spiritCallerCharmEquipped = true;
			player.minionDamageMult += 0.08f;
		}

		internal override bool IsEquipped(NecromancerAccessoryPlayer player)
		{
			return player.spiritCallerCharmEquipped;
		}
	}

	public class SpiritProjectile : BumblingTransientMinion
	{
		public override string Texture => "Terraria/NPC_" + NPCID.Ghost;

		protected override int timeToLive => 60 * 10; // 10 seconds;
		protected override float inertia => 16;
		protected override float idleSpeed => 3;
		protected override float searchDistance => 400f;
		protected override float distanceToBumbleBack => 400f;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			frameSpeed = 15;
			projectile.width = 32;
			projectile.height = 32;
			projectile.tileCollide = true;
		}

		protected override void Move(Vector2 vector2Target, bool isIdle = false)
		{
			if (Math.Abs(vector2Target.Y) > 2)
			{
				vector2Target.Y = 2 * Math.Sign(vector2Target.Y);
			}
			base.Move(vector2Target, isIdle);
		}
	}
}
