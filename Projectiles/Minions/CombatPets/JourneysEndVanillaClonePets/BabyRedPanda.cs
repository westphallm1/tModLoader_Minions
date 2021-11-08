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

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.JourneysEndVanillaClonePets
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
		private NPC targetNPC;
		private Vector2 targetOffset;

		private readonly int TimeToLive = 30;
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
				frames = new int[8];
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
			brightness = Projectile.timeLeft > 10 ? Math.Min(1f, (TimeToLive - Projectile.timeLeft)/10f) : Projectile.timeLeft / 10f;
			length = Math.Min(frames.Length * 16 - 1, 8 * (TimeToLive - Projectile.timeLeft));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			ChainDrawer drawer = new ChainDrawer(GetFrame);
			Vector2 startPoint = Projectile.Center;
			Vector2 endPoint = Projectile.Center + direction * length;
			drawer.DrawChain(texture, startPoint, endPoint, lightColor * brightness);
			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// lazy, only check start and end points
			Vector2 endPoint = Projectile.Center + direction * length;
			return targetHitbox.Contains(endPoint.ToPoint()) || targetHitbox.Contains(Projectile.Center.ToPoint());
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
			Projectile.timeLeft = 180;
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
			if(Projectile.timeLeft % 20 == 0 && Projectile.owner == Main.myPlayer)
			{
				int npcSize = (targetNPC.width + targetNPC.height) / 4;
				Vector2 offset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * (64 + npcSize);
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(),
					targetNPC.Center + offset,
					-offset, 
					ProjectileType<BabyRedPandaBambooSpike>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner);

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

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(24, 30, -16, -18, -1);
			ConfigureFrames(26, (0, 0), (12, 19), (12, 12), (20, 25));
			frameSpeed = 8;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			int yawnCycle = 292;
			int idleFrame = animationFrame % yawnCycle;
			frameInfo[GroundAnimationState.STANDING] = idleFrame < yawnCycle - 80 ? (0, 0) : (0, 11);
			base.Animate(minFrame, maxFrame);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int projType = ProjectileType<BabyRedPandaBambooSpikeController>();
			if(player.whoAmI == Main.myPlayer && animationFrame - lastSpawnedFrame > spawnRate && player.ownedProjectileCounts[projType] == 0)
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
