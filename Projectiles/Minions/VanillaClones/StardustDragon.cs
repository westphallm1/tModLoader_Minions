using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class StardustDragonMinionBuff : MinionBuff
	{
		public StardustDragonMinionBuff() : base(ProjectileType<StardustDragonCounterMinion>()) { }
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.StardustDragonMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.StardustDragonMinion"));
		}

	}

	public class StardustDragonMinionItem : VanillaCloneMinionItem<StardustDragonMinionBuff, StardustDragonCounterMinion>
	{
		internal override int VanillaItemID => ItemID.StardustDragonStaff;

		internal override string VanillaItemName => "StardustDragonStaff";
	}
	public class StardustDragonCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<StardustDragonMinionBuff>();
		protected override int MinionType => ProjectileType<StardustDragonMinion>();
	}

	public class StardustDragonMinion : WormMinion
	{
		public override string Texture => "Terraria/Images/Item_0";
		internal override int BuffId => BuffType<StardustDragonMinionBuff>();
		protected override int CounterType => ProjectileType<StardustDragonCounterMinion>();
		protected override int dustType => 135;
		protected override float baseDamageRatio => 1.6f;
		protected override float damageGrowthRatio => 0.45f;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.StardustDragon") + " (AoMM Version)");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			attackThroughWalls = true;
			wormDrawer = new StardustDragonDrawer();
		}
		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			int radius = Math.Abs(player.velocity.X) < 4 ? 160 : 24;
			float idleAngle = 2 * PI * groupAnimationFrame / groupAnimationFrames;
			idlePosition.X += radius * (float)Math.Cos(idleAngle);
			idlePosition.Y += radius * (float)Math.Sin(idleAngle);
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.75f);
			return vectorToIdlePosition;
		}

		protected override float ComputeSearchDistance()
		{
			return 1100 + 50 * GetSegmentCount();
		}

		protected override float ComputeInertia()
		{
			return Math.Max(12, 22 - GetSegmentCount());
		}

		protected override float ComputeTargetedSpeed()
		{
			return Math.Min(20, 12 + GetSegmentCount());
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		public override Vector2? FindTarget()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center, searchDistance, losCenter: player.Center) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, searchDistance, losCenter: player.Center) is Vector2 target2)
			{
				return target2 - Projectile.Center;
			}
			else
			{
				return null;
			}
		}
	}

	public class StardustDragonDrawer : WormDrawer
	{

		public StardustDragonDrawer() : base()
		{
			if(!Main.dedServ)
			{
				Main.instance.LoadProjectile(ProjectileID.StardustDragon1);
				Main.instance.LoadProjectile(ProjectileID.StardustDragon2);
				Main.instance.LoadProjectile(ProjectileID.StardustDragon3);
				Main.instance.LoadProjectile(ProjectileID.StardustDragon4);
			}
		}

		protected override void DrawHead()
		{
			texture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.StardustDragon1];
			AddSprite(2, texture.Frame());
		}

		protected override void DrawBody()
		{
			for (int i = 0; i < 2 * SegmentCount; i++)
			{
				if (i % 2 == 0)
				{
					texture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.StardustDragon2];
				}
				else
				{
					texture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.StardustDragon3];
				}
				
				AddSprite(22 + 16 * i, texture.Frame());
			}
		}

		protected override void DrawTail()
		{
			int dist = 22 + 32 * SegmentCount;
			texture = Terraria.GameContent.TextureAssets.Projectile[ProjectileID.StardustDragon4];
			lightColor = Color.White;
			lightColor.A = 128;
			AddSprite(dist, texture.Frame());
		}

		protected override SpriteEffects GetEffects(float angle)
		{
			SpriteEffects effects = SpriteEffects.FlipVertically;
			angle = (angle + 2 * (float)Math.PI) % (2 * (float)Math.PI); // get to (0, 2PI) range
			if (angle > Math.PI / 2 && angle < 3 * Math.PI / 2)
			{
				effects |= SpriteEffects.FlipHorizontally;
			}
			return effects;
		}

		protected override void AddSprite(float dist, Rectangle bounds, Color c = default)
		{
			Vector2 origin = new Vector2(bounds.Width / 2f, bounds.Height / 2f);
			Vector2 angle = new Vector2();
			Vector2 pos = PositionLog.PositionAlongPath(dist, ref angle);
			float r = angle.ToRotation();
			Main.EntitySpriteDraw(texture.Value, pos - Main.screenPosition,
				bounds, c == default ? lightColor : c, r + MathHelper.PiOver2,
				origin, 1, GetEffects(r), 0);
			if (Main.rand.Next(30) == 0)
			{
				int dustId = Dust.NewDust(pos, 8, 8, 135, 0f, 0f, 0, default, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].fadeIn = 2f;
				Main.dust[dustId].noLight = true;
			}
		}
	}
}
