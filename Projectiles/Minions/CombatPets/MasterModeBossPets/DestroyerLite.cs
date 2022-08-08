using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions.BoneSerpent;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class DestroyerLiteMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<DestroyerLiteMinion>() };

		public override int VanillaBuffId => BuffID.DestroyerPet;

		public override string VanillaBuffName => "DestroyerPet";
	}

	public class DestroyerLiteMinionItem : CombatPetMinionItem<DestroyerLiteMinionBuff, DestroyerLiteMinion>
	{
		internal override int VanillaItemID => ItemID.DestroyerPetItem;

		internal override int AttackPatternUpdateTier => (int)CombatPetTier.Hallowed;

		internal override string VanillaItemName => "DestroyerPetItem";
	}

	/// <summary>
	/// Uses ai[0] for NPC to target
	/// </summary>
	public class DestroyerLiteProbeProjectile : ModProjectile
	{
		private NPC targetNPC;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.timeLeft = 90;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void AI()
		{
			if(targetNPC == null || !targetNPC.active)
			{
				targetNPC = MinionBehavior.GetClosestEnemyToPosition(Projectile.Center, 300);
				return;
			}
			Vector2 target = targetNPC.Center - Projectile.Center;
			Projectile.rotation = target.ToRotation() + MathHelper.PiOver2;
			Projectile.velocity *= 0.95f; // gradually come to a halt
			bool shouldShootThisFrame = Projectile.timeLeft == 50 || Projectile.timeLeft == 30;
			if(Projectile.owner == Main.myPlayer && shouldShootThisFrame)
			{
				target.SafeNormalize();
				target *= 12;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					target,
					ProjectileType<MiniTwinsLaser>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
				SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.position);
			}
		}

		public override void Kill(int timeLeft)
		{
			Vector2 position = Projectile.Center;
			int width = 22;
			int height = 22;
			for (int i = 0; i < 3; i++)
			{
				int dustIdx = Dust.NewDust(position, width, height, 60, 0f, 0f, 100, default, 2f);
				Main.dust[dustIdx].noGravity = true;
				Main.dust[dustIdx].velocity *= 1.5f;
				dustIdx = Dust.NewDust(position, width, height, 60, 0f, 0f, 100, default, 1.5f);
				Main.dust[dustIdx].velocity *= 0.75f;
				Main.dust[dustIdx].noGravity = true;
			}
		}
	}

	public class DestroyerLiteMinion : CombatPetGroundedWormMinion
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DestroyerPet;
		public override int BuffId => BuffType<DestroyerLiteMinionBuff>();
		public override int CounterType => -1;
		protected override int dustType => 135;

		private int lastHitFrame = 0;
		private int ProbeSpawnRate => Math.Max(20, 60 - 6 * leveledPetPlayer.PetLevel);

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			wormDrawer = new DestroyerLiteDrawer();
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if(Player.whoAmI == Main.myPlayer && AnimationFrame - lastHitFrame > ProbeSpawnRate && leveledPetPlayer.PetLevel >= (int)CombatPetTier.Hallowed)
			{
				lastHitFrame = AnimationFrame;
				Vector2 launchVector = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * 6;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					target.Center,
					launchVector,
					ProjectileType<DestroyerLiteProbeProjectile>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);

			}
		}
	}

	public class DestroyerLiteDrawer : VerticalWormDrawer
	{

		protected override void DrawHead()
		{
			AddSprite(2, new(0, 0, 28, 26));
		}

		protected override void DrawBody()
		{
			for (int i = 0; i < SegmentCount; i++)
			{
				AddSprite(24 + 14 * i, new(0, 44, 28, 16));
			}
		}

		protected override void DrawTail()
		{
			int dist = 24 + 14 * SegmentCount;
			lightColor = new Color(
				Math.Max(lightColor.R, (byte)25),
				Math.Max(lightColor.G, (byte)25),
				Math.Max(lightColor.B, (byte)25));
			AddSprite(dist, new(0, 78, 28, 22));
		}
	}
}
