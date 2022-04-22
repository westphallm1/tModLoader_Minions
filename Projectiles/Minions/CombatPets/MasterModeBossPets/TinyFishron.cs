using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.Squires.SeaSquire;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;


namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class TinyFishronMinionBuff : CombatPetVanillaCloneBuff
	{
		public TinyFishronMinionBuff() : base(ProjectileType<TinyFishronMinion>()) { }

		public override int VanillaBuffId => BuffID.DukeFishronPet;

		public override string VanillaBuffName => "DukeFishronPet";
	}

	public class TinyFishronMinionItem : CombatPetMinionItem<TinyFishronMinionBuff, MiniRetinazerMinion>
	{
		internal override int VanillaItemID => ItemID.DukeFishronPetItem;
		internal override int AttackPatternUpdateTier => (int)CombatPetTier.Hallowed;
		internal override string VanillaItemName => "DukeFishronPetItem";
	}

	public class TinyFishronWhirlpool : ModProjectile
	{
		private WhirlpoolDrawer whirlpoolDrawer;
		private int TimeLeft = 150;
		private NPC targetNPC;
		public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/VanillaClones/BigSharknadoMinion";

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 6;
		}
		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.tileCollide = false;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			// can hit many npcs at once, so give it a relatively high on hit cooldown
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
			Projectile.timeLeft = TimeLeft;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if(TimeLeft - Projectile.timeLeft < 10)
			{
				return false;
			}
			for(int i = 0; i < whirlpoolDrawer.offsets.Length; i++)
			{
				if(whirlpoolDrawer.GetWhirlpoolBox(i).Intersects(targetHitbox))
				{
					return true;
				}
			}
			return false;
		}

		public override void AI()
		{
			// visual effects
			base.AI();
			if(whirlpoolDrawer == null)
			{
				whirlpoolDrawer = new WhirlpoolDrawer();
				whirlpoolDrawer.frameHeight = 
					Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
				whirlpoolDrawer.frameWidth = 
					Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Width;
				whirlpoolDrawer.AddSpawnDust(Projectile);
			}
			int animationFrame = TimeLeft - Projectile.timeLeft;
			int whirlpoolHeight;
			if(Projectile.timeLeft > 30)
			{
				whirlpoolHeight = Math.Min(6, 2 + animationFrame / 10);
			} else
			{
				whirlpoolHeight = 3 + Projectile.timeLeft / 10;
			}
			whirlpoolDrawer.CalculateWhirlpoolPositions(Projectile, animationFrame, whirlpoolHeight, out int height);
			int heightChange = height - Projectile.height;
			Projectile.position.Y -= heightChange;
			Projectile.height = height;
			whirlpoolDrawer.AddWhirlpoolEffects();
			// Start moving towards a target eventually 
			if(animationFrame < 30)
			{
				return;
			}
			if(targetNPC == null || !targetNPC.active)
			{
				targetNPC = Minion.GetClosestEnemyToPosition(Projectile.Center, 300);
				return;
			}
			Vector2 target = targetNPC.Center - Projectile.Bottom;
			target.SafeNormalize();
			target *= 6; // slow
			int inertia = 30;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + target) / inertia;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			lightColor = new Color(150, 150, 150, 128);
			int frame = ((TimeLeft - Projectile.timeLeft) / 5) % 6;
			whirlpoolDrawer.DrawWhirlpoolStack(texture, lightColor, frame, Main.projFrames[Projectile.type]);
			return false;
		}
	}

	public class TinyFishronMinion : CombatPetHoverDasherMinion
	{
		internal override int BuffId => BuffType<TinyFishronMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DukeFishronPet;

		internal override float DamageMult => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Hallowed ? 0.75f : 1f;

		int lastHitFrame;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 6;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			circleHelper.idleBumbleFrames = 90;
			frameSpeed = 5;
			forwardDir = -1;
		}

		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			base.IdleMovement(vectorToIdlePosition);
		}


		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(target, damage, knockback, crit);
			if(player.whoAmI == Main.myPlayer && animationFrame - lastHitFrame > attackFrames && 
			   leveledPetPlayer.PetLevel >= (int)CombatPetTier.Hallowed)
			{
				lastHitFrame = animationFrame;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					target.Center, 
					Vector2.Zero, 
					ProjectileType<TinyFishronWhirlpool>(), 
					Projectile.damage, 
					Projectile.knockBack, 
					Main.myPlayer);
			}
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = 0.05f * Projectile.velocity.X;
		}
	}
}
