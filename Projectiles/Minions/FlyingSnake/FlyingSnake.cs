using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.FlyingSnake
{
	public class FlyingSnakeMinionBuff : MinionBuff
	{
		public FlyingSnakeMinionBuff() : base(ProjectileType<FlyingSnakeMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Flying Snake");
			Description.SetDefault("A flying snake will fight for you!");
		}
	}

	public class FlyingSnakeMinionItem : EmpoweredMinionItem<FlyingSnakeMinionBuff, FlyingSnakeMinion>
	{

		protected override int dustType => 39;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Snake Staff");
			Tooltip.SetDefault("Summons a flying snake to fight for you!");

		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.damage = 42;
			item.height = 32;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.Lime;
		}
	}

	public class FlyingSnakeMinion : WormMinion<FlyingSnakeMinionBuff>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying Snake");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.tileCollide = false;
			frameSpeed = 5;
		}

		protected override void DrawHead()
		{
			Rectangle head;
			if (vectorToTarget is null)
			{
				head = new Rectangle(0, 0, 20, 28);
			}
			else
			{
				head = new Rectangle(0, 28, 20, 28);
			}
			AddSprite(2, head);
		}

		protected override void DrawBody()
		{
			Rectangle wings = GetWingsFrame();
			AddSprite(20, wings);
			for (int i = 0; i < GetSegmentCount() + 1; i++)
			{
				Rectangle body = new Rectangle(0, 56, 18, 28);
				AddSprite(40 + 16 * i, body);
			}
		}

		private Rectangle GetWingsFrame()
		{
			return new Rectangle(0, 112 + 28 * projectile.frame, 22, 28);
		}

		protected override void DrawTail()
		{
			Rectangle tail = new Rectangle(0, 84, 20, 28);
			int dist = 40 + 16 * (GetSegmentCount() + 1);
			AddSprite(dist, tail);
		}

		protected override float ComputeSearchDistance()
		{
			return 800 + 30 * GetSegmentCount();
		}

		protected override float ComputeInertia()
		{
			return 22 - GetSegmentCount();
		}

		protected override float ComputeTargetedSpeed()
		{
			return Math.Min(20, 4 + 3.5f * GetSegmentCount());
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 4;
		}
	}
}
