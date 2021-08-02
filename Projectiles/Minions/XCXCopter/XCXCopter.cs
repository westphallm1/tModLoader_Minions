using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.XCXCopter
{
	public class XCXCopterMinionBuff : MinionBuff
	{
		public XCXCopterMinionBuff() : base(ProjectileType<XCXCopterCounterMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copter-X");
			Description.SetDefault("A flexible helicopter will fight for you!");
		}
	}

	public class XCXCopterMinionItem : MinionItem<XCXCopterMinionBuff, XCXCopterCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copter-X Staff");
			Tooltip.SetDefault("Summons a flexible helicopter to fight for you!");

		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 32;
			Item.damage = 43;
			Item.height = 34;
			Item.value = Item.buyPrice(0, 15, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.MechanicalLens, 1).AddIngredient(ItemID.HallowedBar, 12).AddTile(TileID.MythrilAnvil).Register();
		}
	}

	public class XCXCopterCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<XCXCopterMinionBuff>();
		protected override int MinionType => ProjectileType<XCXCopterMinion>();
	}
	public class XCXCopterMinion : WormMinion
	{
		internal override int BuffId => BuffType<XCXCopterMinionBuff>();
		protected override int CounterType => ProjectileType<XCXCopterCounterMinion>();
		protected override int dustType => 72;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Copter-X");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			frameSpeed = 5;
			wormDrawer = new CopterDrawer();
		}

		protected override float ComputeSearchDistance()
		{
			return 700 + 30 * GetSegmentCount();
		}

		protected override float ComputeInertia()
		{
			return Math.Max(12, 22 - GetSegmentCount());
		}

		protected override float ComputeTargetedSpeed()
		{
			return Math.Min(16, 3 + 3f * GetSegmentCount());
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

	public class CopterDrawer : WormDrawer
	{
		protected override void DrawHead()
		{
			Rectangle head = new Rectangle(206, 0, 24, 36);
			AddSprite(2, head);
		}

		protected override void DrawBody()
		{
			Rectangle body;
			for (int i = 0; i < SegmentCount + 1; i++)
			{
				if (i % 3 == 0)
				{
					body = GetRotorFrame();
				}
				else
				{
					body = new Rectangle(174, 0, 24, 36);
				}
				AddSprite(22 + 22 * i, body);
			}
		}

		private Rectangle GetRotorFrame()
		{
			int frame = this.frame == 3 ? 1 : this.frame;
			return new Rectangle(38 + 46 * frame, 0, 44, 36);
		}

		protected override void DrawTail()
		{
			Rectangle tail = new Rectangle(0, 0, 36, 36);
			int dist = 26 + 22 * (SegmentCount + 1);
			AddSprite(dist, tail);
		}
	}
}
