using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class StardustCellMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<StardustCellMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.StardustMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.StardustMinion"));
		}
	}

	public class StardustCellMinionItem : VanillaCloneMinionItem<StardustCellMinionBuff, StardustCellMinion>
	{
		internal override int VanillaItemID => ItemID.StardustCellStaff;

		internal override string VanillaItemName => "StardustCellStaff";
	}

	/// <summary>
	/// Uses ai[0] for cling target
	/// </summary>
	public class StardustCellClinger : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinionShot;
		public override string GlowTexture => "Terraria/Images/Glow_190";
		Vector2 clingOffset;
		NPC clingTarget;
		float baseVelocity;
		bool hasHitTarget = false;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 4;
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.timeLeft = 300;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return clingTarget == null;
		}

		public override void AI()
		{
			base.AI();
			if(Projectile.ai[0] == 0)
			{
				return; // failsafe in case we got a bad NPC index
			}
			if(clingTarget == null)
			{
				clingTarget = Main.npc[(int)Projectile.ai[0]];
				baseVelocity = Projectile.velocity.Length();
			}
			if(!clingTarget.active)
			{
				Projectile.Kill();
				return;
			}
			if(hasHitTarget)
			{
				Projectile.velocity = Vector2.Zero;
				Projectile.Center = clingTarget.position + clingOffset;
			} else {
				Vector2 vectorToTarget = clingTarget.Center - Projectile.Center;
				float distanceToTarget = vectorToTarget.Length();
				if(distanceToTarget < 24 && Projectile.owner != Main.myPlayer)
				{
					attachToTarget();
				} else
				{
					vectorToTarget.Normalize();
					vectorToTarget *= baseVelocity;
					Projectile.velocity = vectorToTarget;
				}
			}
		}

		private void attachToTarget()
		{
			if(clingTarget == null)
			{
				Projectile.Kill();
				return;
			}
			DebuffGlobalNPC globalTarget = clingTarget.GetGlobalNPC<DebuffGlobalNPC>();
			if(globalTarget.cellStack < 10)
			{
				hasHitTarget = true;
				clingOffset = new Vector2(Main.rand.NextFloat(clingTarget.width), Main.rand.NextFloat(clingTarget.height));
				Projectile.friendly = false;
				globalTarget.cellStack++;
			} else
			{
				Projectile.Kill();
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			// make sure we're hitting the right target
			Projectile.ai[0] = target.whoAmI;
			attachToTarget();
		}

		public override void Kill(int timeLeft)
		{
			if(clingTarget != null && clingTarget.active)
			{
				clingTarget.GetGlobalNPC<DebuffGlobalNPC>().cellStack --;
			}
			for (int i = 0; i < 10; i++)
			{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 229);
				Dust dust = Main.dust[dustId];
				dust.noGravity = true;
				dust.velocity *= 3f;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}
		public override void PostDraw(Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Request<Texture2D>(GlowTexture).Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, Color.White, r,
				origin, 1, effects, 0);
		}
	}

	public class StardustCellMinion : HoverShooterMinion
	{
		internal override int BuffId => BuffType<StardustCellMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinion;
		public override string GlowTexture => "Terraria/Images/Glow_189";
		internal override int? FiredProjectileId => ProjectileType<StardustCellClinger>();
		internal override SoundStyle? ShootSound => SoundID.NPCDeath7 with { Volume = 0.5f };

		internal int baseSpeed = 14;
		internal int baseInertia = 10;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying StardustCell");
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.StardustCellMinion") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 4;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			DrawOffsetX = (Projectile.width - 44) / 2;
			targetSearchDistance = 1100;
			attackFrames = 40;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 14;
			hsHelper.projectileVelocity = 16;
			hsHelper.targetInnerRadius = 128;
			hsHelper.targetOuterRadius = 176;
			hsHelper.targetShootProximityRadius = 256;
			hsHelper.CustomFireProjectile = FireProjectile;
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.75f);
			return base.IdleBehavior();
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
			if(Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = -1;
			}
			else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = 1;
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
		internal void FireProjectile(Vector2 lineOfFire, int projId, float ai0 = 0)
		{
			if(targetNPCIndex is int idx)
			{
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					VaryLaunchVelocity(lineOfFire),
					projId,
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer,
					ai0: idx);
			}
		}


		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(animationFrame - hsHelper.lastShootFrame <= 6)
			{
				return;
			}
			// "teleport" functionality (but not really since it's just moving fast)
			float teleportLength = 2.5f * hsHelper.targetOuterRadius;
			if(vectorToTargetPosition.LengthSquared() > teleportLength * teleportLength)
			{
				int speedMult = 4;
				hsHelper.travelSpeed = speedMult * baseSpeed;
				hsHelper.inertia = 1;
				Vector2 stepVector = vectorToTargetPosition;
				stepVector.SafeNormalize();
				for(int i = 0; i < hsHelper.travelSpeed; i += baseSpeed / 2)
				{
					Vector2 posVector = Projectile.Center + stepVector * i;
					int dustId = Dust.NewDust(posVector, 8, 8, 229);
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].velocity *= 0.25f;
				}
			} else
			{
				hsHelper.travelSpeed = baseSpeed;
				if(Projectile.velocity.LengthSquared() < baseSpeed)
				{
					hsHelper.inertia = baseInertia;
				}
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		internal override void AfterFiringProjectile()
		{
			base.AfterFiringProjectile();
			for (int i = 0; i < 5; i++)
			{
				int width = Projectile.width;
				int dustId = Dust.NewDust(Projectile.Center - Vector2.One * width/4, width/2, width/2, 88);
				Dust dust = Main.dust[dustId];
				Vector2 offset = Vector2.Normalize(dust.position - Projectile.Center);
				dust.position = Projectile.Center + offset * width/ 4  - new Vector2(4f);
				dust.velocity = offset * dust.velocity.Length() * 2f;
				dust.noGravity = true;
				dust.scale = 0.7f + Main.rand.NextFloat();
			}
			Vector2 target = (Vector2)vectorToTarget;
			target.Normalize();
			target *= -4;
			Projectile.velocity = target;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}
		public override void PostDraw(Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Request<Texture2D>(GlowTexture).Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, Color.White, r,
				origin, 1, effects, 0);
		}
	}

}
