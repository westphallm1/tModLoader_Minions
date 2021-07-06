using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.WhackAMole
{
	public class WhackAMoleMinionBuff : MinionBuff
	{
		public WhackAMoleMinionBuff() : base(ProjectileType<WhackAMoleCounterMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Jellybean Mole");
			Description.SetDefault("A magic mole will fight for you!");
		}
	}

	public class WhackAMoleMinionItem : MinionItem<WhackAMoleMinionBuff, WhackAMoleCounterMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Magic Jelly Bean Jar");
			Tooltip.SetDefault("Summons a stack of magic moles to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.damage = 34;
			item.value = Item.buyPrice(0, 15, 0, 0);
			item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SoulofLight, 10);
			recipe.AddIngredient(ItemID.PixieDust, 15);
			recipe.AddIngredient(ItemID.StarinaBottle, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class WhackAMoleMinionProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = true;
			projectile.penetrate = 2;
			projectile.tileCollide = true;
			projectile.timeLeft = 60;
			projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			projectile.rotation += 0.25f;
		}

		public override void Kill(int timeLeft)
		{
			int dustIdx = Dust.NewDust(projectile.Center, 8, 8, 192, newColor: WhackAMoleMinion.shades[(int)projectile.ai[0]], Scale: 1.2f);
			Main.dust[dustIdx].velocity = projectile.velocity / 2;
			Main.dust[dustIdx].noLight = false;
			Main.dust[dustIdx].noGravity = true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);

			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition,
				texture.Bounds, WhackAMoleMinion.shades[(int)projectile.ai[0]], projectile.rotation,
				texture.Bounds.Center.ToVector2(), 1, 0, 0);
			return false;
		}
	}

	public class WhackAMoleCounterMinion : CounterMinion
	{

		internal override int BuffId => BuffType<WhackAMoleMinionBuff>();
		protected override int MinionType => ProjectileType<WhackAMoleMinion>();
	}
	public class WhackAMoleMinion : EmpoweredMinion
	{

		internal override int BuffId => BuffType<WhackAMoleMinionBuff>();
		protected override int CounterType => ProjectileType<WhackAMoleCounterMinion>();

		protected override int dustType => DustID.Dirt;

		protected int idleGroundDistance = 128;
		protected int idleStopChasingDistance = 800;
		protected int lastHitFrame = -1;
		protected int AnimationFrames = 60;
		protected int TeleportFrames = 60;
		private int projectileIndex;
		private GroundAwarenessHelper gHelper;
		private static Vector2[] positionOffsets =
		{
			Vector2.Zero,
			new Vector2(-16, 0),
			new Vector2(-8, -14),
			new Vector2(16, 0),
			new Vector2(8, -14),
			new Vector2(0, -28)
		};

		// x offset of every mole
		private static int[] xOffsets = { 0, 8, 8, 0, 0, 0 };

		private static int[] widths = { 24, 32, 32, 48, 48, 48 };
		private static Rectangle[] platformBounds =
		{
			new Rectangle(0, 0, 28, 22),
			new Rectangle(0, 24, 44, 22),
			new Rectangle(0, 24, 44, 22),
			new Rectangle(0, 48, 52, 24),
			new Rectangle(0, 48, 52, 24),
			new Rectangle(0, 48, 52, 24),
		};

		// color of every mole
		public static Color[] shades =
		{
			new Color(101, 196, 255),
			new Color(153, 221, 146),
			new Color(255, 101, 132),
			new Color(233, 229, 146),
			new Color(173, 101, 255),
			new Color(255, 101, 244),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Goblin Gunner");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 1;
			IdleLocationSets.trailingOnGround.Add(projectile.type);
		}


		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 24;
			projectile.tileCollide = false;
			projectile.friendly = true;
			projectile.localNPCHitCooldown = 30;
			attackThroughWalls = false;
			frameSpeed = 5;
			animationFrame = 0;
			projectile.hide = true;
			projectileIndex = 0;
			gHelper = new GroundAwarenessHelper(this)
			{
				ScaleLedge = ScaleLedge,
				GetUnstuck = DoTeleport,
				IdleFlyingMovement = IdleFlyingMovement,
				IdleGroundedMovement = IdleGroundedMovement
			};
			pathfinder.modifyPath = gHelper.ModifyPathfinding;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			gHelper.DoTileCollide(oldVelocity);
			return false;
		}

		// offset of each individual mole

		private int DrawIndex => Math.Max(0, Math.Min(shades.Length, EmpowerCount) - 1);

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D texture = GetTexture(Texture);
			// draw in reverse order for layering purposes
			float headBobOffset = (float)(2 * Math.PI / Math.Max(DrawIndex, 1));
			for (int i = DrawIndex; i >= 0; i--)
			{
				int offsetPixels;
				Vector2 pos = projectile.Center;
				if (gHelper.teleportStartFrame is int teleportStart)
				{
					int teleportFrame = animationFrame - teleportStart;
					int teleportHalf = TeleportFrames / 2;
					float heightToSink = 24 - positionOffsets[i].Y;
					if (teleportFrame < teleportHalf)
					{
						offsetPixels = -(int)(heightToSink * teleportFrame / teleportHalf);
					}
					else
					{
						offsetPixels = -(int)(heightToSink * (TeleportFrames - teleportFrame) / teleportHalf);
					}
					pos.X += positionOffsets[i].X;
					pos.Y += positionOffsets[i].Y / 2;
				}
				else
				{
					offsetPixels = (int)(3 * Math.Sin(headBobOffset * i + 2 * Math.PI * (animationFrame % AnimationFrames) / AnimationFrames));
					pos += positionOffsets[i];
				}
				Rectangle bounds = new Rectangle(0, 0, 24, 24 + offsetPixels);
				pos.Y += 4 - (offsetPixels / 2);
				pos.X += xOffsets[DrawIndex];
				SpriteEffects effects = projectile.velocity.X < 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				spriteBatch.Draw(texture, pos - Main.screenPosition,
					bounds, shades[i], 0,
					bounds.Center.ToVector2(), 1, effects, 0);
			}
			DrawPlatform(spriteBatch, lightColor);
			return false;
		}

		private void DrawPlatform(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D platform = GetTexture(Texture + "_Ground");
			Rectangle bounds = platformBounds[DrawIndex];
			Vector2 pos = projectile.Bottom + new Vector2(0, bounds.Height / 2);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(platform, pos - Main.screenPosition,
				bounds, lightColor, 0, origin, 1, 0, 0);

		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			noLOSPursuitTime = gHelper.isFlying ? 15 : 300;
			Vector2 idlePosition = gHelper.isFlying ? player.Top : player.Bottom;
			Vector2 idleHitLine = player.Center;
			idlePosition.X += -player.direction * IdleLocationSets.GetXOffsetInSet(IdleLocationSets.trailingOnGround, projectile);
			if (!Collision.CanHitLine(idleHitLine, 1, 1, player.Center, 1, 1))
			{
				idlePosition = player.Bottom;
			}
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			gHelper.SetIsOnGround();
			if (gHelper.offTheGroundFrames == 20 || (gHelper.offTheGroundFrames > 60 && Main.rand.Next(120) == 0) || (gHelper.isOnGround && gHelper.offTheGroundFrames > 20))
			{
				DrawPlatformDust();
			}
			Lighting.AddLight(projectile.Center, Color.PaleGreen.ToVector3() * 0.75f);
			return vectorToIdlePosition;
		}

		private void DrawPlatformDust()
		{
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(projectile.Bottom - new Vector2(8, 0), 16, 16, 47, -projectile.velocity.X / 2, -projectile.velocity.Y / 2);
			}
		}


		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			gHelper.DoIdleMovement(vectorToIdlePosition, vectorToTarget, ComputeSearchDistance(), idleGroundDistance);
		}

		public void IdleFlyingMovement(Vector2 vectorToIdlePosition)
		{
			projectile.tileCollide = false;
			base.IdleMovement(vectorToIdlePosition);
		}

		private void DoTeleport(Vector2 destination, int startFrame, ref bool done)
		{
			projectile.velocity = Vector2.Zero;
			int teleportFrame = animationFrame - startFrame;
			int width = widths[DrawIndex];
			if (teleportFrame == 1 || teleportFrame == 1 + TeleportFrames / 2)
			{
				Collision.HitTiles(projectile.Bottom + new Vector2(-width / 2, 8), new Vector2(0, 8), width, 8);
			}
			if (teleportFrame == TeleportFrames / 2)
			{
				// do the actual teleport
				projectile.position = destination;
			}
			else if (teleportFrame >= TeleportFrames)
			{
				done = true;
			}
		}

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
			drawCacheProjsBehindNPCsAndTiles.Add(index);
		}

		public void IdleGroundedMovement(Vector2 vectorToIdlePosition)
		{
			StuckInfo info = gHelper.GetStuckInfo(vectorToIdlePosition);
			if (info.isStuck)
			{
				gHelper.GetUnstuckByTeleporting(info, vectorToIdlePosition);
			}
			gHelper.ApplyGravity();
			if (vectorToIdlePosition.Y < -projectile.height && Math.Abs(vectorToIdlePosition.X) < 96)
			{
				gHelper.DoJump(vectorToIdlePosition);
			}
			if (animationFrame - lastHitFrame > 10)
			{
				float intendedY = projectile.velocity.Y;
				base.IdleMovement(vectorToIdlePosition);
				projectile.velocity.Y = intendedY;
			}
		}

		public override void OnHitTarget(NPC target)
		{
			lastHitFrame = animationFrame;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			fallThrough = false;
			return true;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			int attackRate = Math.Max(40, 65 - 5 * EmpowerCount);
			bool isAttackFrame = player.whoAmI == Main.myPlayer && animationFrame % attackRate == 0;
			bool canHitTarget = isAttackFrame && Collision.CanHit(projectile.Center, 1, 1, projectile.Center + vectorToTargetPosition, 1, 1);
			bool isAbove = isAttackFrame && Math.Abs(vectorToTargetPosition.X) < 160 && vectorToTargetPosition.Y < -24;
			bool isAttackingFromAir = isAttackFrame && gHelper.isFlying;
			if (player.whoAmI == Main.myPlayer && targetNPCIndex is int targetIdx && isAttackFrame && canHitTarget && (isAbove || isAttackingFromAir))
			{
				Vector2 velocity = vectorToTargetPosition;
				velocity.SafeNormalize();
				velocity *= 12;
				velocity.X += Main.npc[targetIdx].velocity.X;
				Projectile.NewProjectile(
					projectile.Center,
					velocity,
					ProjectileType<WhackAMoleMinionProjectile>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI,
					projectileIndex);
				projectileIndex = (projectileIndex + 1) % (DrawIndex + 1);
			}
			if (gHelper.isFlying)
			{
				// try to stay below target while flying at it
				vectorToTargetPosition.Y += 48;
			}
			IdleMovement(vectorToTargetPosition);
		}

		protected override int ComputeDamage()
		{
			return baseDamage + (baseDamage / 3) * EmpowerCount;
		}

		private Vector2? GetTargetVector()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, player.Center, noLOSRange: searchDistance) is Vector2 target)
			{
				return target - projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance) is Vector2 target2)
			{
				return target2 - projectile.Center;
			}
			else
			{
				return null;
			}
		}
		public override Vector2? FindTarget()
		{
			return GetTargetVector();
		}

		protected override float ComputeSearchDistance()
		{
			return 800 + 25 * EmpowerCount;
		}

		protected override float ComputeInertia()
		{
			return 5;
		}

		protected override float ComputeTargetedSpeed()
		{
			// ComputeTargetedSpeed is never called 
			// since the same AI is used for targetted and non-targetted movement
			return ComputeIdleSpeed();
		}

		protected override float ComputeIdleSpeed()
		{
			return gHelper.isFlying ? 13 : 8 + (vectorToTarget == null ? 0 : Math.Min(2, 0.5f * EmpowerCount));
		}

		protected override void SetMinAndMaxFrames(ref int minFrame, ref int maxFrame)
		{
			minFrame = 0;
			maxFrame = 0;
		}

		public override void AfterMoving()
		{
			gHelper.SetOffTheGroundFrames();
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			if (Math.Abs(projectile.velocity.X) > 4)
			{
				projectile.spriteDirection = projectile.velocity.X > 0 ? -1 : 1;
			}
		}

		public bool ScaleLedge(Vector2 vectorToIdlePosition)
		{
			projectile.velocity.Y = -4;
			gHelper.isOnGround = false;
			if (gHelper.offTheGroundFrames < 20)
			{
				gHelper.offTheGroundFrames = 20;
			}
			return true;
		}
	}
}
