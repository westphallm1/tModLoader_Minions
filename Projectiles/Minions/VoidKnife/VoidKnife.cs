using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VoidKnife
{
	public class VoidKnifeMinionBuff : MinionBuff
	{
		public VoidKnifeMinionBuff() : base(ProjectileType<VoidKnifeMinion>(), ProjectileType<VoidKnifeMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Void Dagger");
			Description.SetDefault("An ethereal dagger will fight for you!");
		}
	}

	public class VoidKnifeMinionItem : MinionItem<VoidKnifeMinionBuff, VoidKnifeMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Void Dagger");
			Tooltip.SetDefault("Summons an ethereal dagger to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 27;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 34;
			item.height = 34;
			item.value = Item.sellPrice(0, 1, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
			Projectile.NewProjectile(position - new Vector2(5, 0), new Vector2(speedX, speedY), item.shoot, damage, knockBack, Main.myPlayer);
			return false;
		}
	}


	public class VoidKnifeMinion : TeleportingWeaponMinion
	{

		protected override int BuffId => BuffType<VoidKnifeMinionBuff>();
		protected override Vector3 lightColor => Color.Purple.ToVector3() * 0.75f;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Void Dagger");
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
			attackState = AttackState.IDLE;
			projectile.minionSlots = 1;
			attackFrames = 120;
			attackThroughWalls = true;
			useBeacon = false;
			travelVelocity = 16;
			targetIsDead = false;
		}


		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			int alpha = 128;
			float phaseLength = maxPhaseFrames / 2;
			if (phaseFrames > 0 && phaseFrames < phaseLength)
			{
				alpha -= (int)(128 * phaseFrames / phaseLength);
			}
			else if (phaseFrames >= phaseLength && phaseFrames < maxPhaseFrames)
			{
				alpha = (int)(128 * (phaseFrames - phaseLength) / phaseLength);
			}
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, alpha);
			Texture2D texture = Main.projectileTexture[projectile.type];


			int height = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * height, texture.Width, height);
			Vector2 origin = bounds.Size() / 2;
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				bounds, translucentColor, projectile.rotation,
				origin, 1, 0, 0);
			return false;
		}

		public override void WindUpBehavior(ref Vector2 vectorToTargetPosition)
		{
			//TODO void knife ai
			//This section might require a slight change of the behavior regarding the teleporting to work properly for MP
			//Randomized stuff should only be decided by the client
			//That would require a change of the ai so it doesnt move for other clients during this phase
			if (Main.myPlayer == player.whoAmI)
			{
				if (distanceFromFoe == default)
				{
					distanceFromFoe = 80 + Main.rand.Next(-20, 20);
					teleportAngle = Main.rand.NextFloat(MathHelper.TwoPi);
					teleportDirection = teleportAngle.ToRotationVector2();
					// move to fixed position relative to NPC, preDraw will do phase in animation
					projectile.Center = targetNPC.Center + teleportDirection * (distanceFromFoe + phaseFrames);
					projectile.netUpdate = true;
				}
				else
				{
					vectorToTargetPosition.SafeNormalize();
					projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.PiOver2;
				}
				//Don't change position continuously, bandaid fix until a proper way for it to work in MP is figured out
			}
		}

		public override void SwingBehavior(ref Vector2 vectorToTargetPosition)
		{
			if (framesInAir++ > maxFramesInAir || framesWithoutTarget == 10)
			{
				targetNPC = null;
				attackState = AttackState.RETURNING;
			}
			else if (framesInAir - lastHitFrame > 10 && !targetIsDead)
			{
				projectile.friendly = true;
				vectorToTargetPosition.SafeNormalize();
				projectile.velocity = vectorToTargetPosition * travelVelocity;
				projectile.rotation = vectorToTargetPosition.ToRotation() + MathHelper.PiOver2;
			}
			Dust.NewDust(projectile.Center, 8, 8, DustID.Shadowflame);
		}

		public override void OnLoseTarget(ref Vector2 vectorToTargetPosition)
		{
			framesInAir = Math.Max(framesInAir, maxFramesInAir - 15);
			float r = projectile.rotation + 3 * (float)Math.PI / 2;
			projectile.velocity = new Vector2((float)Math.Cos(r), (float)Math.Sin(r));
			projectile.velocity *= travelVelocity;
		}
	}
}
