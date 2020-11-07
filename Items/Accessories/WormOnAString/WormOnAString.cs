using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories.WormOnAString
{
	class WormOnAString : NecromancerAccessory
	{
		protected override float baseDamage => 8;
		protected override int bossLifePerSpawn => 200;
		protected override int maxTransientMinions => 3;
		protected override float onKillChance => 0.67f;

		protected override int projType => ProjectileType<WormProjectile>();

		protected override float spawnVelocity => 4;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Can of Worms on a String");
			Tooltip.SetDefault("Increases your minion damage by 1\n" +
				"Has a chance to create a temporary worm minion on defeating an enemy");
		}

		public override void SetDefaults()
		{
			item.width = 32;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 5);
			item.rare = ItemRarityID.White;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<NecromancerAccessoryPlayer>().wormOnAStringEquipped = true;
		}

		internal override void ModifyPlayerWeaponDamage(NecromancerAccessoryPlayer necromancerAccessoryPlayer, Item item, ref float add, ref float mult, ref float flat)
		{
			flat += 1;
		}

		internal override bool IsEquipped(NecromancerAccessoryPlayer player)
		{
			return player.wormOnAStringEquipped;
		}
	}
	public class WormProjectile : TransientMinion
	{
		public const int TIME_TO_LIVE = 60 * 4;

		bool hasLanded;
		int framesToTurn;
		readonly Random random = new Random();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			frameSpeed = 15;
			projectile.width = 14;
			projectile.height = 8;
			projectile.tileCollide = true;
			hasLanded = false;
			projectile.timeLeft = TIME_TO_LIVE;
			framesToTurn = 400 + 30 * random.Next(-3, 3);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (oldVelocity.Y != 0 && projectile.velocity.Y == 0)
			{
				hasLanded = true;
			}
			if (oldVelocity.X != 0 && projectile.velocity.X == 0)
			{
				projectile.velocity.X = -Math.Sign(oldVelocity.X);
			}
			return false;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (projectile.timeLeft < TIME_TO_LIVE - 30)
			{
				projectile.velocity.Y += .5f;
			}
			if (hasLanded)
			{
				projectile.velocity.X = Math.Sign(projectile.velocity.X);
			}
			if (hasLanded && projectile.timeLeft % framesToTurn == 0) // turn around every so often
			{
				projectile.velocity.X *= -3;
			}
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				Dust.NewDust(projectile.Center - Vector2.One * 16, 32, 32, DustID.Dirt);
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (!hasLanded)
			{
				maxFrame = 1;
				projectile.rotation += 0.2f;
			}
			else
			{
				maxFrame = 2;
				projectile.rotation = 0;
				projectile.spriteDirection = projectile.direction;
			}
			base.Animate(minFrame, maxFrame);
		}
	}
}
