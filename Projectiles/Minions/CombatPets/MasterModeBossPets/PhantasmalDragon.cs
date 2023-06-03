using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class PhantasmalDragonMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<PhantasmalDragonMinion>() };

		public override int VanillaBuffId => BuffID.LunaticCultistPet;

		public override string VanillaBuffName => "LunaticCultistPet";
	}

	public class PhantasmalDragonMinionItem : CombatPetMinionItem<PhantasmalDragonMinionBuff, PhantasmalDragonMinion>
	{
		internal override int VanillaItemID => ItemID.LunaticCultistPetItem;
		internal override int AttackPatternUpdateTier => (int)CombatPetTier.Hallowed;
		internal override string VanillaItemName => "LunaticCultistPetItem";
	}

	/// <summary>
	/// Uses ai[0] for NPC id to target
	/// </summary>
	public class AncientVisionProjectile : ModProjectile
	{
		private NPC targetNPC;
		private float maxSpeed = 8;
		private int orphanedFrames;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.frameCounter++;
			Projectile.frame = (Projectile.frameCounter / 5) % 8;
			if(targetNPC == null && Projectile.ai[0] >= 0)
			{
				targetNPC = Main.npc[(int)Projectile.ai[0]];
			}
			if(targetNPC == null || !targetNPC.active)
			{
				targetNPC = null;
				Projectile.ai[0] = -1;
				if(orphanedFrames++ > 60)
				{
					Projectile.Kill();
				}
				return;
			}
			orphanedFrames = 0;
			int inertia = 8;
			Vector2 target = targetNPC.Center - Projectile.Center;
			target.Normalize();
			target *= maxSpeed;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + target) / inertia;

			// dust effect while moving
			Vector2 position = Projectile.position;
			int width = 22;
			int height = 22;
			int dustIdx = Dust.NewDust(position, width, height, 87, 0f, 0f, 100, default, 1.5f);
			Main.dust[dustIdx].velocity *= 0.75f;
			Main.dust[dustIdx].noGravity = true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			lightColor = Color.White * 0.75f;
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : 0;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				bounds.GetOrigin(), 0.75f, effects, 0);
			return false;
		}

		public override void Kill(int timeLeft)
		{
			Vector2 position = Projectile.Center;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 5; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, 87, 0f, 0f, 100, default, 2f);
				Main.dust[dustIdx].noGravity = true;
				Main.dust[dustIdx].velocity *= 1.5f;
				dustIdx = Dust.NewDust(position, width, height, 87, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 0.75f;
				Main.dust[dustIdx].noGravity = true;
			}
		}
	}

	public class PhantasmalDragonMinion : CombatPetWormMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.LunaticCultistPet;
		public override int BuffId => BuffType<PhantasmalDragonMinionBuff>();
		protected override int dustType => 135;

		private int lastShootFrame;
		private readonly int fireRate = 60;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type]
				.WithCode(Preview);
		}

		private static void Preview(Projectile proj, bool walking)
		{
			//TODO 1.4.4 need to do something with this to make it work
			var worm = (PhantasmalDragonMinion)proj.ModProjectile;
			worm.wormDrawer.AddPosition(proj.position);
			worm.wormDrawer.Update(proj.frame);
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			wormDrawer = new PhantasmalDragonDrawer();
		}

		public override Vector2 IdleBehavior()
		{
			base.IdleBehavior();
			Vector2 idlePosition = Player.Top;
			int radius = Math.Abs(Player.velocity.X) < 4 ? 140 : 24;
			float idleAngle = IdleLocationSets.GetAngleOffsetInSet(IdleLocationSets.circlingHead, Projectile)
				+ 2 * MathHelper.Pi * GroupAnimationFrame / GroupAnimationFrames;
			idlePosition.X += radius * (float)Math.Cos(idleAngle);
			idlePosition.Y += radius * (float)Math.Sin(idleAngle);
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			TeleportToPlayer(ref vectorToIdlePosition, 2000f);
			return vectorToIdlePosition;
		}

		public override Vector2? FindTarget()
		{
			float searchDistance = ComputeSearchDistance();
			if (PlayerTargetPosition(searchDistance, Player.Center, searchDistance, losCenter: Player.Center) is Vector2 target)
			{
				return target - Projectile.Center;
			}
			else if (SelectedEnemyInRange(searchDistance, searchDistance, losCenter: Player.Center) is Vector2 target2)
			{
				return target2 - Projectile.Center;
			}
			else
			{
				return null;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			int projType = ProjectileType<AncientVisionProjectile>();
			// redirect any orphaned ancient visions to this new target
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if(p.active && p.owner == Main.myPlayer && p.type == projType && p.ai[0] < 0)
				{
					p.ai[0] = target.whoAmI;
				}
			}
			if(Projectile.owner == Main.myPlayer && AnimationFrame - lastShootFrame > fireRate && leveledPetPlayer.PetLevel >= (int)CombatPetTier.Hallowed)
			{
				lastShootFrame = AnimationFrame;
				// spawn projectile slightly off the top of the screen
				Vector2 spawnPos = Main.screenPosition + new Vector2(Main.rand.Next(0, Main.screenWidth), -16);
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					spawnPos,
					Projectile.velocity,
					projType,
					3 * Projectile.damage / 2,
					Projectile.knockBack,
					Main.myPlayer,
					ai0: target.whoAmI);
			}
		}
	}

	public class PhantasmalDragonDrawer : VerticalWormDrawer
	{

		protected override void DrawHead()
		{
			AddSprite(2, new(0, 0, 40, 40));
		}

		protected override void DrawBody()
		{
			for (int i = 0; i < SegmentCount; i++)
			{
				if(i == 1 || i == 4)
				{
					AddSprite(24 + 18 * i, new(0, 54, 40, 24));
				} else
				{
					AddSprite(24 + 18 * i, new(0, 104, 40, 20));
				}
			}
		}

		protected override void DrawTail()
		{
			int dist = 24 + 18 * SegmentCount;
			lightColor = Color.White;
			lightColor.A = 128;
			AddSprite(dist, new(0, 150, 40, 32));
		}
	}
}
