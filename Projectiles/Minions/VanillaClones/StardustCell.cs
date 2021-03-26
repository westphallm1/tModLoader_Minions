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
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class StardustCellMinionBuff : MinionBuff
	{
		public StardustCellMinionBuff() : base(ProjectileType<StardustCellMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("StardustCell");
			Description.SetDefault("A winged acorn will fight for you!");
		}
	}

	public class StardustCellMinionItem : MinionItem<StardustCellMinionBuff, StardustCellMinion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.StardustCellStaff;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("StardustCell Staff");
			Tooltip.SetDefault("Summons a winged acorn to fight for you!");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.StardustCellStaff);
			base.SetDefaults();
		}
	}

	/// <summary>
	/// Uses ai[0] for cling target
	/// </summary>
	public class StardustCellClinger : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.StardustCellMinionShot;
		public override string GlowTexture => "Terraria/Glow_190";
		Vector2 clingOffset;
		NPC clingTarget;
		float baseVelocity;
		bool hasHitTarget = false;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 4;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.usesLocalNPCImmunity = true;
			projectile.timeLeft = 300;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return clingTarget == null;
		}

		public override void AI()
		{
			base.AI();
			if(projectile.ai[0] == 0)
			{
				return; // failsafe in case we got a bad NPC index
			}
			if(clingTarget == null)
			{
				clingTarget = Main.npc[(int)projectile.ai[0]];
				baseVelocity = projectile.velocity.Length();
			}
			if(!clingTarget.active)
			{
				projectile.Kill();
				return;
			}
			if(hasHitTarget)
			{
				projectile.velocity = Vector2.Zero;
				projectile.Center = clingTarget.position + clingOffset;
			} else {
				Vector2 vectorToTarget = clingTarget.Center - projectile.Center;
				float distanceToTarget = vectorToTarget.Length();
				if(distanceToTarget < 24 && projectile.owner != Main.myPlayer)
				{
					attachToTarget();
				} else
				{
					vectorToTarget.Normalize();
					vectorToTarget *= baseVelocity;
					projectile.velocity = vectorToTarget;
				}
			}
		}

		private void attachToTarget()
		{
			DebuffGlobalNPC globalTarget = clingTarget.GetGlobalNPC<DebuffGlobalNPC>();
			if(globalTarget.cellStack < 10)
			{
				hasHitTarget = true;
				clingOffset = new Vector2(Main.rand.NextFloat(clingTarget.width), Main.rand.NextFloat(clingTarget.height));
				projectile.friendly = false;
				globalTarget.cellStack++;
			} else
			{
				projectile.Kill();
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			// make sure we're hitting the right target
			projectile.ai[0] = target.whoAmI;
			attachToTarget();
		}

		public override void Kill(int timeLeft)
		{
			if(clingTarget.active)
			{
				clingTarget.GetGlobalNPC<DebuffGlobalNPC>().cellStack --;
			}
			for (int i = 0; i < 10; i++)
			{
				int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 229);
				Dust dust = Main.dust[dustId];
				dust.noGravity = true;
				dust.velocity *= 3f;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(GlowTexture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, Color.White, r,
				origin, 1, effects, 0);
		}
	}

	public class StardustCellMinion : HoverShooterMinion
	{
		protected override int BuffId => BuffType<StardustCellMinionBuff>();

		public override string Texture => "Terraria/Projectile_" + ProjectileID.StardustCellMinion;
		public override string GlowTexture => "Terraria/Glow_189";
		internal override int? FiredProjectileId => ProjectileType<StardustCellClinger>();
		internal override LegacySoundStyle ShootSound => SoundID.Item17;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Flying StardustCell");
			Main.projFrames[projectile.type] = 4;
			IdleLocationSets.circlingHead.Add(projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			drawOffsetX = (projectile.width - 44) / 2;
			targetSearchDistance = 1100;
			attackFrames = 40;
			hsHelper.travelSpeed = 14;
			hsHelper.projectileVelocity = 16;
			hsHelper.targetInnerRadius = 128;
			hsHelper.targetOuterRadius = 176;
			hsHelper.targetShootProximityRadius = 256;
			hsHelper.FireProjectile = FireProjectile;
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.75f);
			return base.IdleBehavior();
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
			if(projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			}
			else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
			projectile.rotation = projectile.velocity.X * 0.05f;
		}
		internal void FireProjectile(Vector2 lineOfFire, int projId, float ai0 = 0)
		{
			if(targetNPCIndex is int idx)
			{
				Projectile.NewProjectile(
					projectile.Center,
					lineOfFire,
					projId,
					projectile.damage,
					projectile.knockBack,
					Main.myPlayer,
					ai0: idx);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(animationFrame - hsHelper.lastShootFrame > 6)
			{
				base.TargetedMovement(vectorToTargetPosition);
			}
		}

		internal override void AfterFiringProjectile()
		{
			base.AfterFiringProjectile();
			for (int i = 0; i < 5; i++)
			{
				int width = projectile.width;
				int dustId = Dust.NewDust(projectile.Center - Vector2.One * width/4, width/2, width/2, 88);
				Dust dust = Main.dust[dustId];
				Vector2 offset = Vector2.Normalize(dust.position - projectile.Center);
				dust.position = projectile.Center + offset * width/ 4  - new Vector2(4f);
				dust.velocity = offset * dust.velocity.Length() * 2f;
				dust.noGravity = true;
				dust.scale = 0.7f + Main.rand.NextFloat();
			}
			Vector2 target = (Vector2)vectorToTarget;
			target.Normalize();
			target *= -4;
			projectile.velocity = target;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 1, effects, 0);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(GlowTexture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, Color.White, r,
				origin, 1, effects, 0);
		}
	}

}
