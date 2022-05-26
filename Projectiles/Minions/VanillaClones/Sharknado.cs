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
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SharknadoMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
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
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MiniSharkron;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.MiniSharkron);
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.localNPCHitCooldown = 30;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.penetrate = 3;
		}

		public override void AI()
		{
			base.AI();
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.frameCounter++;
			if(Projectile.frameCounter >= 5)
			{
				Projectile.frame += 1;
				Projectile.frame %= 2;
				Projectile.frameCounter = 0;
			}
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 15; i++)
			{
				int dustId = Dust.NewDust(Projectile.Center - Vector2.One * 10f, 50, 50, 5, 0f, -2f);
				Main.dust[dustId].velocity /= 2f;
			}
			var source = Projectile.GetSource_Death();
			int goreId = Gore.NewGore(source, Projectile.Center, Projectile.velocity * 0.8f, 584);
			Main.gore[goreId].timeLeft /= 4;
			goreId = Gore.NewGore(source, Projectile.Center, Projectile.velocity * 0.9f, 585);
			Main.gore[goreId].timeLeft /= 4;
			goreId = Gore.NewGore(source, Projectile.Center, Projectile.velocity, 586);
			Main.gore[goreId].timeLeft /= 4;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually for some reason
			Color translucentColor = new Color(lightColor.R/5, lightColor.G/5, lightColor.B, 128);
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			// motion blur
			for(int i = 0; i < 2; i ++)
			{
				Vector2 blurPos = pos - Projectile.velocity * 2 * (2 - i);
				float scale = 0.7f * 0.125f * i;
				Main.EntitySpriteDraw(texture, blurPos - Main.screenPosition,
					bounds, translucentColor, r,
					origin, scale, effects, 0);
			}

			// regular
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}
	}

	public class SharknadoMinion : HoverShooterMinion
	{
		internal override int BuffId => BuffType<SharknadoMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Tempest;

		internal override int? FiredProjectileId => ProjectileType<MiniSharknado>();

		internal override SoundStyle? ShootSound => SoundID.NPCDeath19 with { Volume = 0.5f };

		// if more than 4 copies are spawned, transform into a single big one
		internal bool isBeingUsedAsToken;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Tempest"));
			Main.projFrames[Projectile.type] = 6;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 32;
			Projectile.height = 32;
			DrawOffsetX = (Projectile.width - 44) / 2;
			attackFrames = 60;
			frameSpeed = 4;
			targetSearchDistance = 900;
			hsHelper.travelSpeed = 12;
			hsHelper.projectileVelocity = 24;
			hsHelper.targetInnerRadius = 108;
			hsHelper.targetOuterRadius = 164;
			hsHelper.targetShootProximityRadius = 196;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{

			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type])
				{
					Projectile.frame = 0;
				}
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			if (Main.rand.NextBool(5))
			{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 217, 0f, 0f, 100, default, 2f);
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return !isBeingUsedAsToken;
		}

		public override void AI()
		{
			if(isBeingUsedAsToken)
			{
				Projectile.friendly = false;
				Projectile.Center = player.Center;
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
			bool selfCountAboveThreshold = player.ownedProjectileCounts[Projectile.type] > 3;
			isBeingUsedAsToken = selfCountAboveThreshold;
			if(isBeingUsedAsToken != lastBeingUsedAsToken)
			{
				for(int i = 0; i < 3; i++)
				{
					int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 217, 0f, 0f, 100, default, 2f);
					Main.dust[dustId].velocity *= 0.3f;
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].noLight = true;
				}
			}
			if (player.whoAmI == Main.myPlayer && selfCountAboveThreshold && player.ownedProjectileCounts[minionType] == 0)
			{
				// hack to prevent multiple 
				if (GetMinionsOfType(Projectile.type)[0].whoAmI == Projectile.whoAmI)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Top, Vector2.Zero, minionType, Projectile.damage, Projectile.knockBack, Main.myPlayer);
				}
			}
		}
	}

	public class WhirlpoolDrawer
	{
		internal Vector2[] offsets = { };
		internal float[] scales = { };

		// parameters for drawing the whirlpool
		internal int animationLength = 60;
		internal float scale0 = 0.25f;
		internal float scale1 = 1f;
		internal int normalStackMax = 12;
		internal int hardStackMax = 60;
		internal float hardMaxScale = 1.75f;
		internal float baseXOffset = 24f;
		internal float cyclesPerStack = 1.5f;
		internal float frameHeight; // set on spawn
		internal float frameWidth; // set on spawn

		internal void CalculateWhirlpoolPositions(Projectile projectile, int animationFrame, int currentStackHeight, out int calculatedHeight)
		{
			// parameters for whirlpool stack
			int frame = animationFrame % animationLength;
			float scalePerIdx = (scale1 - scale0) / normalStackMax;
			int stackHeight = Math.Min(hardStackMax, currentStackHeight);
			stackHeight = Math.Min(stackHeight, animationFrame / 10);
			offsets = new Vector2[stackHeight];
			scales = new float[stackHeight];
			calculatedHeight = (int)(stackHeight * frameHeight * 0.67f);
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

		internal Rectangle GetWhirlpoolBox(int i)
		{
			float width = frameWidth * scales[i];
			float height = frameHeight * scales[i];
			return new Rectangle((int)(offsets[i].X - width / 2), (int)(offsets[i].Y - height / 2), (int)width, (int)height);
		}

		internal void AddWhirlpoolEffects()
		{
			for(int i = 0; i < offsets.Length; i++)
			{
				Lighting.AddLight(offsets[i], Color.Aquamarine.ToVector3() * 0.25f);
				if (Main.rand.NextBool(10))
				{
					Rectangle dustRect = GetWhirlpoolBox(i);
					int dustId = Dust.NewDust(dustRect.TopLeft(), dustRect.Width, dustRect.Height, 217, 0f, 0f, 100, default, 2f);
					Main.dust[dustId].velocity *= 0.3f;
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].noLight = true;
				}
			}
		}

		internal void DrawWhirlpoolStack(Texture2D texture, Color lightColor, int frame, int frameCount)
		{
			int frameHeight = texture.Height / frameCount;
			Rectangle bounds = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);

			// draw stacks
			for(int i = 0; i < offsets.Length; i++)
			{
				Main.EntitySpriteDraw(texture, offsets[i] - Main.screenPosition,
					bounds, lightColor, 0, origin, scales[i], 0, 0);
			}
		}

		internal void AddSpawnDust(Projectile projectile)
		{
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
	}

	public class BigSharknadoMinion : EmpoweredMinion
	{
		internal override int BuffId => BuffType<SharknadoMinionBuff>();
		public override int CounterType => ProjectileType<SharknadoMinion>();
		protected override int dustType => 135;

		private WhirlpoolDrawer whirlpoolDrawer;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Tempest"));
			Main.projFrames[Projectile.type] = 6;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			attackThroughWalls = true;
			Projectile.width = 64;
			Projectile.height = 128;
			frameSpeed = 5;
			// can hit many npcs at once, so give it a relatively high on hit cooldown
			Projectile.localNPCHitCooldown = 20;
			whirlpoolDrawer = new WhirlpoolDrawer();
		}
		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = player.Top;
			Projectile.localNPCHitCooldown = Math.Max(10, 25 - EmpowerCount);
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
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
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

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			for(int i = 0; i < whirlpoolDrawer.offsets.Length; i++)
			{
				if(whirlpoolDrawer.GetWhirlpoolBox(i).Intersects(targetHitbox))
				{
					return true;
				}
			}
			return false;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			whirlpoolDrawer.frameHeight = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
			whirlpoolDrawer.frameWidth = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Width;
			whirlpoolDrawer.AddSpawnDust(Projectile);
		}
		public override void AfterMoving()
		{
			base.AfterMoving();
			whirlpoolDrawer.CalculateWhirlpoolPositions(Projectile, animationFrame, EmpowerCount + 3, out int height);
			Projectile.height = height;
			whirlpoolDrawer.AddWhirlpoolEffects();
			if(EmpowerCount > 0 && EmpowerCount < 4)
			{
				Projectile.Kill();
			}
		}


		private int cooldownAfterHitFrames = 16;
		private int framesSinceLastHit = 0;


		protected override int ComputeDamage()
		{
			return (baseDamage * EmpowerCount) / 2;
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = Main.projFrames[Projectile.type];
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			lightColor = new Color(150, 150, 150, 128);
			whirlpoolDrawer.DrawWhirlpoolStack(texture, lightColor, Projectile.frame, Main.projFrames[Projectile.type]);
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
				Projectile.velocity *= 8; // kick it away from enemies that it's just hit
			}
		}

		public override void OnHitTarget(NPC target)
		{
			framesSinceLastHit = 0;
		}
	}
}
