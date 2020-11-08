using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SoulofNight, 10);
			recipe.AddIngredient(ItemID.ThrowingKnife, 50);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}


	public class VoidKnifeMinion : GroupAwareMinion<VoidKnifeMinionBuff>
	{

		private int framesInAir;
		private float idleAngle;
		private int maxFramesInAir = 60;
		private float travelVelocity;
		private NPC targetNPC = null;
		private float distanceFromFoe = default;
		private float teleportAngle = default;
		private Vector2 teleportDirection;
		private int phaseFrames;
		private int maxPhaseFrames = 60;
		private int lastHitFrame = 0;
		private Random random = new Random();
		private int framesWithoutTarget;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Void Dagger");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
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
			animationFrames = 120;
			attackThroughWalls = true;
			useBeacon = false;
			travelVelocity = 16;
		}


		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			List<Projectile> minions = GetActiveMinions();
			Vector2 idlePosition = player.Center;
			int minionCount = minions.Count;
			int order = minions.IndexOf(projectile);
			idleAngle = (float)(2 * Math.PI * order) / minionCount;
			idleAngle += (2 * (float)Math.PI * minions[0].ai[1]) / animationFrames;
			idlePosition.X += 2 + 30 * (float)Math.Cos(idleAngle);
			idlePosition.Y += -12 + 5 * (float)Math.Sin(idleAngle);
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
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
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				bounds, translucentColor, projectile.rotation,
				origin, 1, 0, 0);
			return false;
		}

		public override Vector2? FindTarget()
		{
			if (FindTargetInTurnOrder(800f, projectile.Center, 600f) is Vector2 target)
			{
				framesWithoutTarget = 0;
				return target;
			}
			else
			{
				projectile.friendly = false;
				return null;
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if (targetNPC is null && targetNPCIndex is int index)
			{
				targetNPC = Main.npc[index];
				distanceFromFoe = default;
				phaseFrames = 0;
				framesInAir = 0;
				lastHitFrame = -10;
			}
			else if (targetNPC != null && phaseFrames++ < maxPhaseFrames / 2)
			{
				// do nothing, preDraw will do phase out animation
				IdleMovement(vectorToIdle);
			}

			else if (phaseFrames > maxPhaseFrames / 2 && phaseFrames < maxPhaseFrames)
			{
				if (distanceFromFoe == default)
				{
					distanceFromFoe = 80 + random.Next(-20, 20);
					teleportAngle = (float)(random.NextDouble() * 2 * Math.PI);
					teleportDirection = new Vector2((float)Math.Cos(teleportAngle), (float)Math.Sin(teleportAngle));
				}
				else
				{
					vectorToTargetPosition.SafeNormalize();
					projectile.rotation = vectorToTargetPosition.ToRotation() + (float)Math.PI / 2;
				}
				// move to fixed position relative to NPC, preDraw will do phase in animation
				projectile.position = targetNPC.Center + teleportDirection * (distanceFromFoe + phaseFrames);
				projectile.friendly = false;
			}
			else if (framesInAir++ > maxFramesInAir || framesWithoutTarget == 10)
			{
				targetNPC = null;
				attackState = AttackState.RETURNING;
			}
			else if (framesInAir - lastHitFrame > 10)
			{
				projectile.friendly = true;
				vectorToTargetPosition.SafeNormalize();
				projectile.velocity = vectorToTargetPosition * travelVelocity;
				projectile.rotation = vectorToTargetPosition.ToRotation() + (float)Math.PI / 2;
			}
			if (phaseFrames >= maxPhaseFrames)
			{
				Dust.NewDust(projectile.Center, 8, 8, DustID.Shadowflame);
			}
			Lighting.AddLight(projectile.position, Color.Purple.ToVector3() * 0.75f);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			lastHitFrame = framesInAir;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			if (attackState == AttackState.ATTACKING)
			{
				framesWithoutTarget++;
				if (phaseFrames < maxPhaseFrames && targetNPC != null)
				{
					TargetedMovement(targetNPC.Center - projectile.Center);
				}
				else
				{
					TargetedMovement(projectile.velocity);
				}
				return;
			}
			projectile.rotation = (float)Math.PI;
			// alway clamp to the idle position
			projectile.tileCollide = false;

			if (vectorToIdlePosition.Length() > 32 && vectorToIdlePosition.Length() < 1000)
			{
				projectile.position += vectorToIdlePosition;
			}
			else
			{
				attackState = AttackState.IDLE;
				projectile.rotation = (player.Center - projectile.Center).X * -0.01f;
				projectile.position += vectorToIdlePosition;
				projectile.velocity = Vector2.Zero;
			}
		}
	}
}
