using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class SharknadoMinionBuff : MinionBuff
	{
		public SharknadoMinionBuff() : base(ProjectileType<SharknadoMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.SharknadoMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.SharknadoMinion"));
		}
	}

	public class SharknadoMinionItem : VanillaCloneMinionItem<SharknadoMinionBuff, SharknadoMinion>
	{
		internal override int VanillaItemID => ItemID.TempestStaff;

		internal override string VanillaItemName => "TempestStaff";
	}

	public class MiniSharknado : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.MiniSharkron;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
			Main.projFrames[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.MiniSharkron);
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			projectile.localNPCHitCooldown = 30;
			projectile.usesLocalNPCImmunity = true;
			projectile.penetrate = 3;
		}

		public override void AI()
		{
			base.AI();
			projectile.rotation = projectile.velocity.ToRotation();
			projectile.frameCounter++;
			if(projectile.frameCounter >= 5)
			{
				projectile.frame += 1;
				projectile.frame %= 2;
				projectile.frameCounter = 0;
			}
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 15; i++)
			{
				int dustId = Dust.NewDust(projectile.Center - Vector2.One * 10f, 50, 50, 5, 0f, -2f);
				Main.dust[dustId].velocity /= 2f;
			}
			int goreId = Gore.NewGore(projectile.Center, projectile.velocity * 0.8f, 584);
			Main.gore[goreId].timeLeft /= 4;
			goreId = Gore.NewGore(projectile.Center, projectile.velocity * 0.9f, 585);
			Main.gore[goreId].timeLeft /= 4;
			goreId = Gore.NewGore(projectile.Center, projectile.velocity, 586);
			Main.gore[goreId].timeLeft /= 4;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			// need to draw sprites manually for some reason
			Color translucentColor = new Color(lightColor.R/5, lightColor.G/5, lightColor.B, 128);
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// motion blur
			for(int i = 0; i < 2; i ++)
			{
				Vector2 blurPos = pos - projectile.velocity * 2 * (2 - i);
				float scale = 0.7f * 0.125f * i;
				spriteBatch.Draw(texture, blurPos - Main.screenPosition,
					bounds, translucentColor, r,
					origin, scale, effects, 0);
			}

			// regular
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}
	}

	public class SharknadoMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<SharknadoMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.Tempest;

		internal override int? FiredProjectileId => ProjectileType<MiniSharknado>();

		internal override LegacySoundStyle ShootSound => new LegacySoundStyle(4, 19).WithVolume(0.5f);

		// if more than 4 copies are spawned, transform into a single big one
		internal bool isBeingUsedAsToken;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Tempest"));
			Main.projFrames[projectile.type] = 6;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 32;
			projectile.height = 32;
			drawOffsetX = (projectile.width - 44) / 2;
			attackFrames = 60;
			frameSpeed = 4;
			targetSearchDistance = 900;
			hsHelper.travelSpeed = 12;
			hsHelper.projectileVelocity = 24;
			hsHelper.targetInnerRadius = 108;
			hsHelper.targetOuterRadius = 164;
			hsHelper.targetShootProximityRadius = 196;
			idleBumble = true;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type])
				{
					projectile.frame = 0;
				}
			}
			projectile.rotation = projectile.velocity.X * 0.05f;

			if (Main.rand.Next(5) == 0)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 217, 0f, 0f, 100, default, 2f);
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			return !isBeingUsedAsToken;
		}

		public override void AI()
		{
			if(isBeingUsedAsToken)
			{
				projectile.friendly = false;
				projectile.position = player.position;
				CheckActive();
				AfterMoving();
			} else
			{
				base.AI();
			}
		}

		public override void AfterMoving()
		{
			int minionType = ProjectileType<BigSharknadoMinion>();
			bool lastBeingUsedAsToken = isBeingUsedAsToken;
			isBeingUsedAsToken = player.ownedProjectileCounts[projectile.type] > 3;
			if(isBeingUsedAsToken != lastBeingUsedAsToken)
			{
				for(int i = 0; i < 3; i++)
				{
					int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 217, 0f, 0f, 100, default, 2f);
					Main.dust[dustId].velocity *= 0.3f;
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].noLight = true;
				}
			}
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projectile.type] > 3 && player.ownedProjectileCounts[minionType] == 0)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(projectile.type)[0].whoAmI == projectile.whoAmI)
				{
					Projectile.NewProjectile(player.Top, Vector2.Zero, minionType, projectile.damage, projectile.knockBack, Main.myPlayer);
				}
			}
		}
	}

	public class BigSharknadoMinion : EmpoweredMinion
	{
		protected override int BuffId => BuffType<SharknadoMinionBuff>();
		protected override int CounterType => ProjectileType<SharknadoMinion>();
		protected override int dustType => 135;

		protected Vector2[] offsets = { };
		protected float[] scales = { };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Tempest"));
			Main.projFrames[projectile.type] = 6;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.tileCollide = false;
			attackThroughWalls = true;
			projectile.width = 64;
			projectile.height = 128;
			frameSpeed = 5;
			// can hit many npcs at once, so give it a relatively high on hit cooldown
			projectile.localNPCHitCooldown = 20;
		}
		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			projectile.localNPCHitCooldown = Math.Max(10, 25 - EmpowerCount);
			if(animationFrame > 60)
			{
				int radius = 160;
				float idleAngle = 2 * PI * groupAnimationFrame / groupAnimationFrames;
				idlePosition.X += radius * (float)Math.Cos(idleAngle);
				idlePosition.Y += radius * (float)Math.Sin(idleAngle);
			} else
			{
				idlePosition.Y -= 48;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		protected override float ComputeSearchDistance()
		{
			return 950 + 50 * EmpowerCount;
		}

		protected override float ComputeInertia()
		{
			return Math.Max(12, 22 - EmpowerCount);
		}

		protected override float ComputeTargetedSpeed()
		{
			return Math.Min(22, 16 + EmpowerCount);
		}

		protected override float ComputeIdleSpeed()
		{
			return ComputeTargetedSpeed() + 3;
		}

		public override Vector2? FindTarget()
		{
			if(animationFrame < 60)
			{
				return null;
			}
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center, searchDistance, losCenter: player.Center) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, player.Center, searchDistance, losCenter: player.Center) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			for(int i = 0; i < offsets.Length; i++)
			{
				if(GetWhirlpoolBox(i).Intersects(targetHitbox))
				{
					return true;
				}
			}
			return false;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			frameHeight = GetTexture(Texture).Height / Main.projFrames[projectile.type];
			frameWidth = GetTexture(Texture).Width;
			// create some dust to show that we've spawned in
			for(int i = 0; i < 6; i++)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 217, 0f, 0f, 100, default, 1.25f);
				Main.dust[dustId].velocity = (i * MathHelper.TwoPi / 6).ToRotationVector2();
				Main.dust[dustId].velocity *= 0.5f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			}
		}
		public override void AfterMoving()
		{
			base.AfterMoving();
			CalculateWhirlpoolPositions();
			AddWhirlpoolEffects();
			if(EmpowerCount > 0 && EmpowerCount < 4)
			{
				projectile.Kill();
			}
		}


		// parameters for drawing the whirlpool
		static int animationLength = 60;
		float scale0 = 0.25f;
		float scale1 = 1f;
		int normalStackMax = 12;
		int hardStackMax = 60;
		float hardMaxScale = 1.75f;
		float baseXOffset = 24f;
		float cyclesPerStack = 1.5f;
		float frameHeight; // set on spawn
		float frameWidth; // set on spawn
		private int cooldownAfterHitFrames = 16;
		private int framesSinceLastHit = 0;

		private void CalculateWhirlpoolPositions()
		{
			// parameters for whirlpool stack
			int frame = animationFrame % animationLength;
			float scalePerIdx = (scale1 - scale0) / normalStackMax;
			int stackHeight = Math.Min(hardStackMax, 3 + EmpowerCount);
			stackHeight = Math.Min(stackHeight, animationFrame / 10);
			offsets = new Vector2[stackHeight];
			scales = new float[stackHeight];
			projectile.height = (int)(stackHeight * frameHeight * 0.67f);
			float yPos = projectile.Bottom.Y;
			float xCenter = projectile.Center.X;

			// draw stacks
			for(int i = 0; i < stackHeight; i++)
			{
				scales[i] = Math.Min(hardMaxScale, scale0 + i* scalePerIdx);
				float angle = (i * cyclesPerStack * MathHelper.TwoPi / stackHeight) + MathHelper.TwoPi * frame / (float) animationLength;
				float xOffset = scales[i] * baseXOffset * (float)Math.Cos(angle);
				offsets[i] = new Vector2(xCenter + xOffset, yPos);
				yPos -= frameHeight * scales[i] - 1;
			}

		}

		private Rectangle GetWhirlpoolBox(int i)
		{
			float width = frameWidth * scales[i];
			float height = frameHeight * scales[i];
			return new Rectangle((int)(offsets[i].X - width / 2), (int)(offsets[i].Y - height / 2), (int)width, (int)height);
		}

		private void AddWhirlpoolEffects()
		{
			for(int i = 0; i < offsets.Length; i++)
			{
				Lighting.AddLight(offsets[i], Color.Aquamarine.ToVector3() * 0.25f);
				if (Main.rand.Next(10) == 0)
				{
					Rectangle dustRect = GetWhirlpoolBox(i);
					int dustId = Dust.NewDust(dustRect.TopLeft(), dustRect.Width, dustRect.Height, 217, 0f, 0f, 100, default, 2f);
					Main.dust[dustId].velocity *= 0.3f;
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].noLight = true;
				}
			}

		}

		protected override int ComputeDamage()
		{
			return (baseDamage * EmpowerCount) / 2;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = Main.projFrames[projectile.type];
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			
			lightColor = new Color(150, 150, 150, 128);
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);

			// draw stacks
			for(int i = 0; i < offsets.Length; i++)
			{
				spriteBatch.Draw(texture, offsets[i] - Main.screenPosition,
					bounds, lightColor, 0, origin, scales[i], 0, 0);
			}
			return false;
		}
		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			float inertia = 18;
			vectorToTargetPosition.SafeNormalize();
			vectorToTargetPosition *= ComputeTargetedSpeed();
			framesSinceLastHit++;
			if (framesSinceLastHit < cooldownAfterHitFrames && framesSinceLastHit > cooldownAfterHitFrames / 2)
			{
				// start turning so we don't double directly back
				Vector2 turnVelocity = new Vector2(-projectile.velocity.Y, projectile.velocity.X) / 8;
				turnVelocity *= Math.Sign(projectile.velocity.X);
				projectile.velocity += turnVelocity;
			}
			else if (framesSinceLastHit++ > cooldownAfterHitFrames)
			{
				projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			else
			{
				projectile.velocity.SafeNormalize();
				projectile.velocity *= 8; // kick it away from enemies that it's just hit
			}
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}
	}
}
