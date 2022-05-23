using AmuletOfManyMinions.NPCs;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class PygmyMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] {  ProjectileType<Pygmy1Minion>(), ProjectileType<Pygmy2Minion>(), ProjectileType<Pygmy3Minion>(), ProjectileType<Pygmy4Minion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.Pygmies") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.Pygmies"));
		}

	}

	public class PygmyMinionItem : VanillaCloneMinionItem<PygmyMinionBuff, Pygmy1Minion>
	{
		internal override int VanillaItemID => ItemID.PygmyStaff;

		internal override string VanillaItemName => "PygmyStaff";
		[CloneByReference] //projTypes is fine to be shared across instances
		public int[] projTypes;

		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player); 

			if (projTypes == null)
			{
				projTypes = new int[]
				{
					ProjectileType<Pygmy1Minion>(),
					ProjectileType<Pygmy2Minion>(),
					ProjectileType<Pygmy3Minion>(),
					ProjectileType<Pygmy4Minion>()
				};
			}
			int spawnCycle = projTypes.Select(v => player.ownedProjectileCounts[v]).Sum();
			var p = Projectile.NewProjectileDirect(source, position, Vector2.Zero, projTypes[spawnCycle % 4], damage, knockback, player.whoAmI);
			p.originalDamage = Item.damage;
			return false;
		}
	}

	/// <summary>
	/// Uses ai[0] for target npc
	/// </summary>
	public class PygmySpear : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PygmySpear;

		public Vector2 stickOffset;
		public NPC stickNPC = null;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.PygmySpear);
			base.SetDefaults();
			Projectile.timeLeft = 540;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.tileCollide = true;
		}

		public override bool PreAI()
		{
			if(stickNPC == null && Projectile.ai[0] > -1)
			{
				stickNPC = Main.npc[(int)Projectile.ai[0]];
			}
			if(stickNPC != null && !stickNPC.active)
			{
				Projectile.Kill();
			}
			if(stickOffset != default)
			{
				Projectile.position = stickNPC.position + stickOffset;
				Projectile.velocity = Vector2.Zero;
			}
			if(stickNPC != null && Projectile.owner != Main.myPlayer && stickNPC.Hitbox.Contains(Projectile.Center.ToPoint()))
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
				Projectile.tileCollide = false;
				Projectile.friendly = false;
				// move back a little bit to stick closer to the exterior
				Projectile.position -= Projectile.velocity / 2;
				stickOffset = Projectile.position - target.position;
				// stop using the spear AI
				Projectile.aiStyle = 0;
				Projectile.rotation += Main.rand.NextFloat(-MathHelper.Pi / 16, MathHelper.Pi / 16);
			} else
			{
				Projectile.Kill();
			}
			
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Collision.HitTiles(Projectile.position + oldVelocity, oldVelocity, Projectile.width, Projectile.height);
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

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
		}
	}

	public abstract class BasePygmyMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<PygmyMinionBuff>();
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
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Pygmy"));
			Main.projFrames[Projectile.type] = 18;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 26;
			DrawOffsetX = -2;
			DrawOriginOffsetY = -18;
			attackFrames = 30;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			searchDistance = 900;
			maxJumpVelocity = 12;
			dealsContactDamage = false;
		}

		protected override void IdleFlyingMovement(Vector2 vector)
		{
			if(animationFrame - lastFiredFrame < 10)
			{
				// don't fly while throwing the spear
				gHelper.didJustLand = false;
				gHelper.isFlying = false;
				gHelper.ApplyGravity();
			} else
			{
				base.IdleFlyingMovement(vector);
			}
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -3 * Projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 8;
			int xMaxSpeed = 11;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			// only change speed if the target is a decent distance away
			if (Math.Abs(vector.X) < 4 && targetNPCIndex is int idx && Math.Abs(Main.npc[idx].velocity.X) < 7)
			{
				Projectile.velocity.X = Main.npc[idx].velocity.X;
			}
			else
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
		}

		private void FireSpear()
		{
			int spearVelocity = 16;
			lastFiredFrame = animationFrame;
			SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
			if (player.whoAmI == Main.myPlayer)
			{
				Vector2 angleToTarget = (Vector2)vectorToTarget;
				angleToTarget.SafeNormalize();
				angleToTarget *= spearVelocity;
				if(targetNPCIndex is int idx)
				{
					Vector2 targetVelocity = Main.npc[idx].velocity;
					if(targetVelocity.Length() > 32)
					{
						targetVelocity.Normalize();
						targetVelocity *= 32;
					}
					angleToTarget += targetVelocity / 4;
				}
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					VaryLaunchVelocity(angleToTarget),
					ProjectileType<PygmySpear>(),
					Projectile.damage,
					Projectile.knockBack,
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

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (animationFrame - lastFiredFrame < 5)
			{
				Projectile.frame = 1;
			} else if (animationFrame - lastFiredFrame < 10)
			{
				Projectile.frame = 2;
			} else if (animationFrame - lastFiredFrame < 15)
			{
				Projectile.frame = 3;
			} else
			{
				GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			}
			if (vectorToTarget is Vector2 target && Math.Abs(target.X) < 1.5 * preferredDistanceFromTarget)
			{
				Projectile.spriteDirection = -Math.Sign(target.X);
			} else if (Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = -1;
			} else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = 1;
			}
		}
	}

	public class Pygmy1Minion : BasePygmyMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Pygmy;
	}

	public class Pygmy2Minion : BasePygmyMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Pygmy2;
	}

	public class Pygmy3Minion : BasePygmyMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Pygmy3;
	}

	public class Pygmy4Minion : BasePygmyMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Pygmy4;
	}
}
