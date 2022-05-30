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
	public class EnchantedDaggerMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Smolstar;
		internal override int[] ProjectileTypes => new int[] { ProjectileType<EnchantedDaggerMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.Smolstar") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.Smolstar"));
		}

	}

	public class EnchantedDaggerMinionItem : VanillaCloneMinionItem<EnchantedDaggerMinionBuff, EnchantedDaggerMinion>
	{
		internal override int VanillaItemID => ItemID.Smolstar;

		internal override string VanillaItemName => "Smolstar";
	}

	public class EnchantedDaggerMinion : HeadCirclingGroupAwareMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Smolstar;
		private int framesSinceLastHit;
		private int cooldownAfterHitFrames = 12;
		internal override int BuffId => BuffType<EnchantedDaggerMinionBuff>();

		private Texture2D solidTexture;
		private Color outlineColor;
		private MotionBlurDrawer blurDrawer;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Smolstar") + " (AoMM Version)");
			IdleLocationSets.circlingHead.Add(Type);
			Main.projFrames[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			attackFrames = 60;
			targetSearchDistance = 600;
			circleHelper.idleBumble = false;
			bumbleSpriteDirection = -1;
			blurDrawer = new MotionBlurDrawer(5);
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			// run this as late as possible, hope to avoid issues with asset loading
			solidTexture = SolidColorTexture.GetSolidTexture(Type);
		}

		public override Vector2 IdleBehavior()
		{

			List<Projectile> minions = GetMinionsOfType(Type);
			if(minions.Count > 0)
			{
				int myIndex = minions.FindIndex(p => p.whoAmI == Projectile.whoAmI);
				outlineColor = (new Color[] { new(247, 168, 184), new(85, 205, 252), Color.White })[myIndex % 3] * 0.5f;
			}
			return base.IdleBehavior();
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(vectorToTarget != default || vectorToIdle.LengthSquared() > 24 * 24)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else
			{
				Projectile.rotation = MathHelper.Pi;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			Rectangle bounds = new(0, 0, texture.Width, texture.Height/2);
			Vector2 origin = bounds.Center.ToVector2();
			Vector2 pos = Projectile.Center - Main.screenPosition;
			SpriteEffects effects = 0;
			// motion blur
			float blurScale = 1f;
			for (int k = 0; k < blurDrawer.BlurLength; k++)
			{
				if(!blurDrawer.GetBlurPosAndColor(k, outlineColor, out Vector2 blurPos, out Color blurColor)) { break; }
				Main.EntitySpriteDraw(solidTexture, blurPos - Main.screenPosition, bounds, blurColor * 0.5f, 
					Projectile.rotation, origin, blurScale, 0, 0);
				blurScale *= 0.85f;
			}

			// glowy outline
			for(int i = -1; i <= 1; i+= 1)
			{
				for(int j = -1; j <= 1; j+= 1)
				{
					Vector2 offset = 2 * new Vector2(i, j).RotatedBy(Projectile.rotation);
					Main.EntitySpriteDraw(solidTexture, pos + offset,
						bounds, outlineColor, Projectile.rotation, origin, 1, effects, 0);
				}
			}
			// main entity
			Main.EntitySpriteDraw(texture, pos,
				bounds, lightColor, Projectile.rotation, origin, 1, effects, 0);

			return false;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float inertia = 8;
			float speed = 16;
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= speed;
			framesSinceLastHit++;
			if (framesSinceLastHit == cooldownAfterHitFrames)
			{
				// immediately snap back to the target after drifting away
				Projectile.velocity = vectorToTargetPosition;
			}
			else if (framesSinceLastHit > cooldownAfterHitFrames)
			{
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				Projectile.velocity.SafeNormalize();
				Projectile.velocity *= 12; // kick it away from enemies that it's just hit
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if(vectorToTarget == default || framesSinceLastHit > cooldownAfterHitFrames)
			{
				return false;
			}

			// bounce off walls while coasting after hitting enemies
			if (Math.Abs(Projectile.velocity.Y) < Math.Abs(oldVelocity.Y))
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			} else if (Math.Abs(Projectile.velocity.X) < Math.Abs(oldVelocity.X))
			{
				Projectile.velocity.X = -oldVelocity.X;
			} else
			{
				// don't really understand what's going on in this case but that's ok
				return false; 
			}
			return false;
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// manually bypass defense
			// this may not be wholly correct
			int defenseBypass = 25;
			int defense = Math.Min(target.defense, defenseBypass);
			damage += defense / 2;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurDrawer.Update(Projectile.Center, vectorToTarget != default);
		}
	}
}
