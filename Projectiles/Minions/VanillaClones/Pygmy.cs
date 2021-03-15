using AmuletOfManyMinions.NPCs;
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
	public class PygmyMinionBuff : MinionBuff
	{
		public PygmyMinionBuff() : base(
			ProjectileType<Pygmy1Minion>(),
			ProjectileType<Pygmy2Minion>(),
			ProjectileType<Pygmy3Minion>(),
			ProjectileType<Pygmy4Minion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
		}

	}

	public class PygmyMinionItem : MinionItem<PygmyMinionBuff, Pygmy1Minion>
	{
		public override string Texture => "Terraria/Item_" + ItemID.PygmyStaff;
		public int[] projTypes = new int[]
		{
			ProjectileType<Pygmy1Minion>(),
			ProjectileType<Pygmy2Minion>(),
			ProjectileType<Pygmy3Minion>(),
			ProjectileType<Pygmy4Minion>()
		};
		int spawnCycle = 0;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ItemName.SlimeStaff"));
			Tooltip.SetDefault("Summons a vampire slime to fight for you!\nIgnores 10 enemy defense");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.PygmyStaff);
			base.SetDefaults();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			item.shoot = projTypes[spawnCycle++ % 4];
			return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
		}
	}

	/// <summary>
	/// Uses ai[0] for target npc
	/// </summary>
	public class PygmySpear : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.PygmySpear;

		public Vector2 stickOffset;
		public NPC stickNPC = null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.PygmySpear);
			base.SetDefaults();
			projectile.timeLeft = 240;
			projectile.tileCollide = true;
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 10;
			projectile.tileCollide = true;
		}

		public override bool PreAI()
		{
			if(stickNPC == null && projectile.ai[0] > -1)
			{
				stickNPC = Main.npc[(int)projectile.ai[0]];
			}
			if(stickNPC != null && !stickNPC.active)
			{
				projectile.Kill();
			}
			if(stickOffset != default)
			{
				projectile.position = stickNPC.position + stickOffset;
				projectile.velocity = Vector2.Zero;
			}
			if(stickNPC != null && projectile.owner != Main.myPlayer && stickNPC.Hitbox.Contains(projectile.Center.ToPoint()))
			{
				OnHitNPC(stickNPC, 0, 0, false);
			}
			return true;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			stickNPC = target;
			if (stickNPC.GetGlobalNPC<DebuffGlobalNPC>().pygmySpearStack < 5)
			{
				stickNPC.GetGlobalNPC<DebuffGlobalNPC>().pygmySpearStack++;
				projectile.tileCollide = false;
				projectile.friendly = false;
				// move back a little bit to stick closer to the exterior
				projectile.position -= projectile.velocity / 2;
				stickOffset = projectile.position - target.position;
				// stop using the spear AI
				projectile.aiStyle = 0;
				projectile.rotation += Main.rand.NextFloat(-MathHelper.Pi / 16, MathHelper.Pi / 16);
			} else
			{
				projectile.Kill();
			}
			
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Collision.HitTiles(projectile.position + oldVelocity, oldVelocity, projectile.width, projectile.height);
			return true;
		}

		public override void Kill(int timeLeft)
		{
			if(stickOffset != default)
			{
				stickNPC.GetGlobalNPC<DebuffGlobalNPC>().pygmySpearStack--;
			}
			base.Kill(timeLeft);
		}

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
			drawCacheProjsBehindNPCs.Add(index);
		}
	}

	public abstract class BasePygmyMinion : SimpleGroundBasedMinion
	{
		protected override int BuffId => BuffType<PygmyMinionBuff>();
		int lastFiredFrame = 0;
		// don't get too close
		int preferredDistanceFromTarget = 128;
		private Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (13, 13),
			[GroundAnimationState.JUMPING] = (4, 4),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (5, 11),
		};

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("PricklyPear");
			Main.projFrames[projectile.type] = 18;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 24;
			projectile.height = 26;
			drawOffsetX = -2;
			drawOriginOffsetY = -18;
			attackFrames = 30;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			maxJumpVelocity = 12;
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 11;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && targetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				projectile.velocity.X = (projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		private void FireSpear()
		{
			int spearVelocity = 16;
			lastFiredFrame = animationFrame;
			Main.PlaySound(new LegacySoundStyle(6, 1), projectile.position);
			if (player.whoAmI == Main.myPlayer)
			{
				Vector2 angleToTarget = (Vector2)vectorToTarget;
				angleToTarget.SafeNormalize();
				angleToTarget *= spearVelocity;
				Projectile.NewProjectile(
					projectile.Center,
					angleToTarget,
					ProjectileType<PygmySpear>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI,
					ai0: targetNPCIndex ?? -1);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if (Math.Abs(vectorToTargetPosition.X) < 4 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 4 * preferredDistanceFromTarget &&
				animationFrame - lastFiredFrame >= attackFrames)
			{
				FireSpear();
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.X) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X = 0;
			}
			if(Math.Abs(vectorToTargetPosition.Y) < 2 * preferredDistanceFromTarget && Math.Abs(vectorToTargetPosition.Y) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override void AfterMoving()
		{
			projectile.friendly = false;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (animationFrame - lastFiredFrame < 5)
			{
				projectile.frame = 1;
			} else if (animationFrame - lastFiredFrame < 10)
			{
				projectile.frame = 2;
			} else if (animationFrame - lastFiredFrame < 15)
			{
				projectile.frame = 3;
			} else
			{
				GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			}
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				projectile.spriteDirection = -Math.Sign(target.X);
			} else if (projectile.velocity.X > 1)
			{
				projectile.spriteDirection = -1;
			} else if (projectile.velocity.X < -1)
			{
				projectile.spriteDirection = 1;
			}
		}
	}

	public class Pygmy1Minion : BasePygmyMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Pygmy;
	}

	public class Pygmy2Minion : BasePygmyMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Pygmy2;
	}

	public class Pygmy3Minion : BasePygmyMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Pygmy3;
	}

	public class Pygmy4Minion : BasePygmyMinion
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.Pygmy4;
	}
}
