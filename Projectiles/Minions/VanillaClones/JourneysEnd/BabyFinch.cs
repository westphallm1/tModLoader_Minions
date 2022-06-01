using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd
{
	public class BabyFinchMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.BabyBird;
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BabyFinchMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.BabyBird") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.BabyBird"));
		}

	}

	public class BabyFinchMinionItem : VanillaCloneMinionItem<BabyFinchMinionBuff, BabyFinchMinion>
	{
		internal override int VanillaItemID => ItemID.BabyBirdStaff;

		internal override string VanillaItemName => "BabyBirdStaff";
	}

	public class BabyFinchMinion : HeadCirclingGroupAwareMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabyBird;
		private int framesSinceLastHit;
		private int cooldownAfterHitFrames = 12;
		internal override int BuffId => BuffType<BabyFinchMinionBuff>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.BabyFinch") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 60;
			targetSearchDistance = 600;
			circleHelper.idleBumbleFrames = 60;
			bumbleSpriteDirection = -1;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(0, 4);
			if(Math.Abs(Projectile.velocity.X) > 1)
			{
				Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			bool isNested = Vector2.DistanceSquared(player.Top, Projectile.Center) < 24 * 24;
			if(!isNested)
			{
				return true;
			}
			int myOrder = GetMinionsOfType(Type)
				.Where(p=>Vector2.DistanceSquared(player.Top, p.Center) < 24 * 24)
				.ToList().FindIndex(p=>p.whoAmI == Projectile.whoAmI);

			Vector2 offset = Projectile.AI_158_GetHomeLocation(player, myOrder) - new Vector2(0, 6);
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			Rectangle bounds = new(8, 106, 16, 12);
			SpriteEffects effects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(texture, offset - Main.screenPosition,
				bounds, lightColor, 0,
				new Vector2(bounds.Width, bounds.Height) / 2, 1f, effects, 0);

			return false;
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 offset = 16 * (MathHelper.TwoPi * animationFrame / 60).ToRotationVector2();
			return player.Top + offset - Projectile.Center;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			overPlayers.Add(index);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float inertia = 18;
			float speed = 9;
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			framesSinceLastHit++;
			if (framesSinceLastHit < cooldownAfterHitFrames && framesSinceLastHit > cooldownAfterHitFrames / 2)
			{
				// start turning so we don't double directly back
				Vector2 turnVelocity = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(Projectile.velocity.X);
				Projectile.velocity += turnVelocity;
			}
			else if (framesSinceLastHit++ > cooldownAfterHitFrames)
			{
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				Projectile.velocity.SafeNormalize();
				Projectile.velocity *= 10; // kick it away from enemies that it's just hit
			}
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if(Vector2.DistanceSquared(Projectile.Center, player.Top) < 24 * 24)
			{
				Projectile.position += vectorToIdlePosition;
				Projectile.velocity = Vector2.Zero;
			} else
			{
				base.IdleMovement(vectorToIdlePosition);
			}
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}
	}
}
