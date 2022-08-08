using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.BalloonMonkey;
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
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BalloonBuddyCounterMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
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
		public override void ApplyCrossModChanges()
		{
			CrossMod.WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, CrossMod.SummonersShineDefaultSpecialWhitelistType.MELEE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 32;
			Item.damage = 18;
			Item.height = 32;
			Item.value = Item.buyPrice(0, 3, 0, 0);
			Item.rare = ItemRarityID.Orange;
		}
	}


	public class BalloonBuddyCounterMinion : CounterMinion
	{
		public override int BuffId => BuffType<BalloonBuddyMinionBuff>();
		protected override int MinionType => ProjectileType<BalloonBuddyMinion>();
	}

	public class BalloonBuddyMinion : WormMinion
	{
		public override int BuffId => BuffType<BalloonBuddyMinionBuff>();
		public override int CounterType => ProjectileType<BalloonBuddyCounterMinion>();
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Balloon Buddy");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 1;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = 20;
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

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (PartyHatSystem.IsParty && Main.rand.NextBool(3))
			{
				Vector2 launchVector = Projectile.velocity;
				launchVector.SafeNormalize();
				launchVector *= 4;
				// only called for owner, no need to check ownership
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					launchVector,
					ProjectileType<BalloonMonkeyBalloon>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner);
			}
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

	internal class BalloonBuddyDrawer : WormDrawer
	{
		protected override void DrawHead()
		{
			Rectangle head = new(PartyHatSystem.IsParty ? 28 : 0, 0, 28, 44);
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
