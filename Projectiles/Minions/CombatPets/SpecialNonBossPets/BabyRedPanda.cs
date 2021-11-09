using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Core.Minions.Effects;
using System;
using System.Collections.Generic;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.SpecialNonBossPets
{
	public class BabyRedPandaMinionBuff : CombatPetVanillaCloneBuff
	{
		public BabyRedPandaMinionBuff() : base(ProjectileType<BabyRedPandaMinion>()) { }
		public override string VanillaBuffName => "BabyRedPanda";
		public override int VanillaBuffId => BuffID.BabyRedPanda;
	}

	public class BabyRedPandaMinionItem : CombatPetMinionItem<BabyRedPandaMinionBuff, BabyRedPandaMinion>
	{
		internal override string VanillaItemName => "BambooLeaf";
		internal override int VanillaItemID => ItemID.BambooLeaf;

		internal override int AttackPatternUpdateTier => 4;
	}

	/// <summary>
	/// Uses ai[0] to track target NPC
	/// </summary>
	public class BabyRedPandaBambooSpike: ModProjectile
	{
		public override string Texture => "Terraria/Images/Tiles_" + TileID.Bamboo;

		private int[] frames;
		private Vector2 direction;
		private float brightness;
		private int length;
		private int startOffset;
		private NPC targetNPC;
		private Vector2 targetOffset;

		private readonly int TimeToLive = 22;
		private readonly int SegmentCount = 12;
		private readonly int ShrinkDelay = 8;
		private readonly int GrowthRate = 12;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			Projectile.timeLeft = TimeToLive;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 45;
		}
		protected Rectangle GetFrame(int idx, bool isLast)
		{
			return new Rectangle(18 * frames[idx], 0, 18, 16);
		}

		public override void AI()
		{
			// set up frame data
			if(frames == default)
			{
				frames = new int[SegmentCount];
				frames[0] = Main.rand.Next(1, 4);
				frames[^1] = Main.rand.Next(15, 19);
				for(int i = 1; i < frames.Length - 1; i++)
				{
					frames[i] = Main.rand.Next(5, 14);
				}
			}
			if(targetNPC == default)
			{
				targetNPC = Main.npc[(int)Projectile.ai[0]];
				targetOffset = targetNPC.Center - Projectile.Center;
			}
			if(targetNPC.active)
			{
				Projectile.Center = targetNPC.Center + targetOffset;
			}
			if(direction == default)
			{
				direction = Projectile.velocity;
				direction.SafeNormalize();
				Projectile.velocity = Vector2.Zero;
			}
			if(Projectile.timeLeft > 10)
			{
				brightness = Math.Min(1f, (TimeToLive - Projectile.timeLeft) / 10f);
			} else
			{
				brightness = Projectile.timeLeft / 10f;
			}
			length = Math.Min(frames.Length * 16 - 1, GrowthRate * (TimeToLive - Projectile.timeLeft));
			int startLength = Math.Min(frames.Length * 16 - 1, GrowthRate * (TimeToLive - Projectile.timeLeft - ShrinkDelay));
			startOffset = Math.Max(0, startLength);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			ChainDrawer drawer = new ChainDrawer(GetFrame);
			Vector2 startPoint = Projectile.Center + direction * startOffset;
			Vector2 endPoint = Projectile.Center + direction * length;
			drawer.DrawChain(texture, startPoint, endPoint, lightColor * brightness);
			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// lazy, only check start and end points
			Vector2 startPoint = Projectile.Center + direction * startOffset;
			Vector2 endPoint = Projectile.Center + direction * length;
			return targetHitbox.Contains(endPoint.ToPoint()) || targetHitbox.Contains(startPoint.ToPoint());
		}

	}

	/// <summary>
	/// Uses ai[0] for target NPC
	/// </summary>
	public class BabyRedPandaBambooSpikeController: ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_0";
		private NPC targetNPC;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			Projectile.timeLeft = 90;
			Projectile.friendly = false;
		}

		public override void AI()
		{
			if(targetNPC == default)
			{
				targetNPC = Main.npc[(int)Projectile.ai[0]];
			} 
			if(!targetNPC.active)
			{
				Projectile.Kill();
				return;
			}
			Projectile.Center = targetNPC.Center;
			if(Projectile.timeLeft <= 60 && Projectile.timeLeft > 30 && Projectile.timeLeft % 10 == 0 && Projectile.owner == Main.myPlayer)
			{
				int npcSize = (targetNPC.width + targetNPC.height) / 4;
				Vector2 offset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * (64 + npcSize);
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					targetNPC.Center + offset,
					offset, 
					ProjectileType<BabyRedPandaBambooSpike>(),
					(int)(1.5f * Projectile.damage),
					Projectile.knockBack,
					Projectile.owner,
					ai0: Projectile.ai[0]);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}

	public class BabyRedPandaMinion : CombatPetGroundedMeleeMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BabyRedPanda;
		internal override int BuffId => BuffType<BabyRedPandaMinionBuff>();

		int lastSpawnedFrame;
		int spawnRate = 60;
		List<int> markedNPCs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -18, -1);
			ConfigureFrames(26, (0, 0), (12, 19), (12, 12), (20, 25));
			frameSpeed = 8;
			markedNPCs = new List<int>();
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			int yawnCycle = 292;
			int idleFrame = animationFrame % yawnCycle;
			frameInfo[GroundAnimationState.STANDING] = idleFrame < yawnCycle - 80 ? (0, 0) : (0, 11);
			base.Animate(minFrame, maxFrame);
		}

		public override Vector2 IdleBehavior()
		{
			markedNPCs.Clear();
			int projType = ProjectileType<BabyRedPandaBambooSpikeController>();
			for(int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == Main.myPlayer && p.type == projType)
				{
					markedNPCs.Add((int)p.ai[0]); // don't re-attack marked npcs
				}
			}
			if(oldTargetNpcIndex is int idx && markedNPCs.Contains(idx))
			{
				// need to clear all of this out
				oldTargetNpcIndex = null;
				framesSinceHadTarget = noLOSPursuitTime;
			}
			return base.IdleBehavior();
		}

		public override bool ShouldIgnoreNPC(NPC npc)
		{
			return base.ShouldIgnoreNPC(npc) || markedNPCs.Contains(npc.whoAmI);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = 1;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int projType = ProjectileType<BabyRedPandaBambooSpikeController>();
			if(player.whoAmI == Main.myPlayer && animationFrame - lastSpawnedFrame > spawnRate && !markedNPCs.Contains(target.whoAmI))
			{
				lastSpawnedFrame = animationFrame;
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					target.Center,
					Vector2.Zero,
					projType,
					Projectile.damage,
					Projectile.knockBack,
					player.whoAmI,
					ai0: target.whoAmI);
			}
		}
	}
}
