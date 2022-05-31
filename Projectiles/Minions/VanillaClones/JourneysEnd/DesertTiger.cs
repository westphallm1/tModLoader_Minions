using static AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses.CombatPetConvenienceMethods;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Items.Accessories;
using System;
using Terraria.Audio;
using AmuletOfManyMinions.Core.Minions.Effects;
using Microsoft.Xna.Framework.Graphics;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones.JourneysEnd
{
	public class DesertTigerMinionBuff : MinionBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.StormTiger;

		internal override int[] ProjectileTypes => new int[] { ProjectileType<DesertTigerMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.StormTiger") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.StormTiger"));
		}

	}

	public class DesertTigerMinionItem : VanillaCloneMinionItem<DesertTigerMinionBuff, DesertTigerMinion>
	{
		internal override int VanillaItemID => ItemID.StormTigerStaff;

		internal override string VanillaItemName => "StormTigerStaff";
	}
	

	public class DesertTigerMinion : SimpleGroundBasedMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StormTigerTier1;
		internal override int BuffId => BuffType<DesertTigerMinionBuff>();

		private MotionBlurDrawer blurDrawer;
		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(this, 30, 30, -8, -4);
			ConfigureFrames(10, (0, 0), (1, 7), (4, 4), (8, 8));
			blurDrawer = new MotionBlurDrawer(5);
			xMaxSpeed = 8;
		}

		public override Vector2 IdleBehavior()
		{
			return base.IdleBehavior();
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			float nextRotation = Projectile.rotation + MathHelper.TwoPi / 15 * Math.Sign(Projectile.velocity.X);
			var animState = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			if(animState == GroundAnimationState.FLYING)
			{
				Projectile.rotation = nextRotation;
			} else
			{
				Projectile.rotation = 0;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height / Main.projFrames[Type];
			Rectangle bounds = new(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width, bounds.Height) / 2;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			// motion blur
			for (int k = 0; k < blurDrawer.BlurLength; k++)
			{
				if(!blurDrawer.GetBlurPosAndColor(k, lightColor, out Vector2 blurPos, out Color blurColor)) { break; }
				Main.EntitySpriteDraw(texture, blurPos - Main.screenPosition, bounds, blurColor * 0.5f, 
					Projectile.rotation, origin, 1, effects, 0);
			}
			return true;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			DoDefaultGroundedMovement(vector);
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			blurDrawer.Update(Projectile.Center, Projectile.velocity.LengthSquared() > 2);
		}
	}
}
