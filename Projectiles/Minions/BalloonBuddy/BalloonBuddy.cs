using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.BalloonBuddy
{
	public class BalloonBuddyMinionBuff : MinionBuff
	{
		public BalloonBuddyMinionBuff() : base(ProjectileType<BalloonBuddyCounterMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Balloon Buddy");
			Description.SetDefault("A balloon buddy will fight for you!");
		}
	}

	public class BalloonBuddyMinionItem : MinionItem<BalloonBuddyMinionBuff, BalloonBuddyCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Sorbet Staff");
			Tooltip.SetDefault("Summons an enchanted balloon animal to fight for you!");

		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 32;
			item.damage = 18;
			item.height = 32;
			item.value = Item.buyPrice(0, 3, 0, 0);
			item.rare = ItemRarityID.Orange;
		}
	}


	public class BalloonBuddyCounterMinion : CounterMinion
	{
		internal override int BuffId => BuffType<BalloonBuddyMinionBuff>();
		protected override int MinionType => ProjectileType<BalloonBuddyMinion>();
	}

	public class BalloonBuddyMinion : WormMinion
	{
		internal override int BuffId => BuffType<BalloonBuddyMinionBuff>();
		protected override int CounterType => ProjectileType<BalloonBuddyCounterMinion>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Balloon Buddy");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.tileCollide = false;
			projectile.localNPCHitCooldown = 20;
			wormDrawer = new BalloonBuddyDrawer();
		}


		protected override float ComputeSearchDistance()
		{
			return 600 + 25 * GetSegmentCount();
		}

		protected override float ComputeInertia()
		{
			return Math.Max(18, 22 - GetSegmentCount());
		}

		protected override float ComputeTargetedSpeed()
		{
			return Math.Min(13, 3 + 2 * GetSegmentCount());
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}
	}

	internal class BalloonBuddyDrawer : WormHelper
	{
		protected override void DrawHead()
		{
			Rectangle head = new Rectangle(0, 0, 28, 44);
			AddSprite(2, head);
		}
		protected override void DrawBody()
		{
			Rectangle body = new Rectangle(0, 44, 14, 44);
			for (int i = 0; i < SegmentCount + 1; i++)
			{
				AddSprite(22 + 14 * i, body);
			}
		}

		protected override void DrawTail()
		{
			Rectangle tail = new Rectangle(0, 88, 22, 44);
			int dist = 22 + 14 * (SegmentCount + 1);
			AddSprite(dist, tail);
		}
	}
}
