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
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.VanillaClones
{
	public class SpiderMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] {  ProjectileType<JumperSpiderMinion>(), ProjectileType<VenomSpiderMinion>(), ProjectileType<DangerousSpiderMinion>()  };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("BuffName.SpiderMinion") + " (AoMM Version)");
			Description.SetDefault(Language.GetTextValue("BuffDescription.SpiderMinion"));
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (projectileTypes.Select(p => player.ownedProjectileCounts[p]).Sum() > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}

	}

	public class SpiderMinionItem : VanillaCloneMinionItem<SpiderMinionBuff, VenomSpiderMinion>
	{
		public int[] projTypes;

		public override bool IsCloneable => true; //projTypes is fine to be shared across instances
		internal override int VanillaItemID => ItemID.SpiderStaff;

		internal override string VanillaItemName => "SpiderStaff";

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			ApplyBuff(player);

			if (projTypes == null)
			{
				projTypes = new int[]
				{
					ProjectileType<JumperSpiderMinion>(),
					ProjectileType<VenomSpiderMinion>(),
					ProjectileType<DangerousSpiderMinion>(),
				};
			}
			int spawnCycle = projTypes.Select(v => player.ownedProjectileCounts[v]).Sum();
			var p = Projectile.NewProjectileDirect(source, position, Vector2.Zero, projTypes[spawnCycle % 3], damage, knockback, player.whoAmI);
			p.originalDamage = Item.damage;
			return false;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.UseSound = SoundID.Item83;
		}
	}
	public abstract class BaseSpiderMinion : SimpleGroundBasedMinion
	{
		internal override int BuffId => BuffType<SpiderMinionBuff>();

		internal bool isClinging = false;
		internal bool onWall = false;
		internal int xMaxSpeed = 10;
		internal (int, int) wallFrames = (4, 8);

		float clingDistanceTolerance = 24f;
		Vector2 targetOffset = default;

		internal Dictionary<GroundAnimationState, (int, int?)> frameInfo = new Dictionary<GroundAnimationState, (int, int?)>
		{
			[GroundAnimationState.FLYING] = (8, 11),
			[GroundAnimationState.JUMPING] = (0, 0),
			[GroundAnimationState.STANDING] = (0, 0),
			[GroundAnimationState.WALKING] = (0, 4),
		};


		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.Spider") + " (AoMM Version)");
			Main.projFrames[Projectile.type] = 11;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 26;
			Projectile.height = 26;
			DrawOffsetX = -2;
			DrawOriginOffsetY = -6;
			attackFrames = 60;
			noLOSPursuitTime = 300;
			startFlyingAtTargetHeight = 96;
			startFlyingAtTargetDist = 64;
			defaultJumpVelocity = 4;
			searchDistance = 800;
			maxJumpVelocity = 12;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			// AI is much more consistent than vanilla, so drop base damage down a bit
			Projectile.originalDamage = (int)(0.9f * Projectile.originalDamage);
		}

		// Use flying movement if we're on a wall
		protected override void IdleGroundedMovement(Vector2 vector)
		{
			if(onWall)
			{
				IdleFlyingMovement(vector);
			} else
			{
				base.IdleGroundedMovement(vector);
			}
		}

		// tweak idle flying movement a bit so the spider doesn't bounce back and forth so much
		// while idling on a wall
		protected override void IdleFlyingMovement(Vector2 vector)
		{
			if(onWall && vectorToTarget is null && vector.Length() < 4)
			{
				Projectile.velocity = Vector2.Zero;
			} else
			{
				base.IdleFlyingMovement(vector);
			}
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{

			if (vector.Y < -Projectile.height && Math.Abs(vector.X) < startFlyingAtTargetHeight)
			{
				gHelper.DoJump(vector);
			}
			float xInertia = gHelper.stuckInfo.overLedge && !gHelper.didJustLand && Math.Abs(Projectile.velocity.X) < 2 ? 1.25f : 7;
			if (vectorToTarget is null && Math.Abs(vector.X) < 8)
			{
				Projectile.velocity.X = player.velocity.X;
				return;
			}
			DistanceFromGroup(ref vector);
			if (animationFrame - lastHitFrame > 10)
			{
				Projectile.velocity.X = (Projectile.velocity.X * (xInertia - 1) + Math.Sign(vector.X) * xMaxSpeed) / xInertia;
			}
			else
			{
				Projectile.velocity.X = Math.Sign(Projectile.velocity.X) * xMaxSpeed * 0.75f;
			}
		}

		public override Vector2 IdleBehavior()
		{
			Tile tile = Framing.GetTileSafely((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);
			onWall = (tile.HasTile && tile.BlockType == BlockType.Solid) || tile.WallType > 0;
			return base.IdleBehavior();
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(vectorToTargetPosition.Length() < clingDistanceTolerance)
			{
				// slowly decrease the distance that we're allowed to cling
				if(clingDistanceTolerance > 8f)
				{
					clingDistanceTolerance *= 0.99f;
				}
				onWall = true;
				isClinging = true;
				Projectile.Center += vectorToTargetPosition;
				Projectile.velocity = Vector2.Zero;
			} else
			{
				isClinging = false;
				clingDistanceTolerance = 24;
				int oldMaxSpeed = maxSpeed;
				maxSpeed = 10;
				base.TargetedMovement(vectorToTargetPosition);
				maxSpeed = oldMaxSpeed;
			}
		}

		public override Vector2? FindTarget()
		{
			Vector2? target = base.FindTarget();
			if (targetNPCIndex is int idx && oldTargetNpcIndex != idx)
			{
				// choose a new preferred location on the enemy to cling to
				targetOffset = new Vector2(
					Main.rand.Next(Main.npc[idx].width) - Main.npc[idx].width / 2,
					Main.rand.Next(Main.npc[idx].height) - Main.npc[idx].height / 2);
			}
			if(target is Vector2 tgt)
			{
				return tgt + targetOffset;
			} else
			{
				return null;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.Venom, 300);
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(onWall)
			{
				base.Animate(wallFrames.Item1, wallFrames.Item2);
				if(vectorToTarget != null && isClinging)
				{
					if(animationFrame % 60 > 30)
					{
						Projectile.rotation = MathHelper.PiOver2 + MathHelper.Pi / 8 - (MathHelper.PiOver4 * (animationFrame % 60) / 60f);
					} else
					{
						Projectile.rotation = MathHelper.PiOver2 - MathHelper.Pi / 8 + (MathHelper.PiOver4 * (animationFrame % 60) / 60f);
					}
				} else if (Projectile.velocity.Length() > 0)
				{
					Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				} else
				{
					Projectile.frame = 4;
				}
			} else
			{
				Projectile.rotation = 0;
				GroundAnimationState state = gHelper.DoGroundAnimation(frameInfo, base.Animate);
			}
			if (Projectile.velocity.X > 1)
			{
				Projectile.spriteDirection = -1;
			} else if (Projectile.velocity.X < -1)
			{
				Projectile.spriteDirection = 1;
			}

		}
	}

	public class VenomSpiderMinion: BaseSpiderMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VenomSpider;
	}

	public class JumperSpiderMinion: BaseSpiderMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.JumperSpider;
	}

	public class DangerousSpiderMinion: BaseSpiderMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DangerousSpider;

		public override void SetDefaults()
		{
			base.SetDefaults();
			DrawOriginOffsetY = -2;
		}
	}
}
