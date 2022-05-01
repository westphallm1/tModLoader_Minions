using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets
{
	public class HoneyBeeMinionBuff : CombatPetVanillaCloneBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<HoneyBeeMinion>() };

		public override int VanillaBuffId => BuffID.QueenBeePet;

		public override string VanillaBuffName => "QueenBeePet";
	}

	public class HoneyBeeMinionItem : CombatPetMinionItem<HoneyBeeMinionBuff, HoneyBeeMinion>
	{
		internal override int VanillaItemID => ItemID.QueenBeePetItem;
		internal override int AttackPatternUpdateTier => (int)CombatPetTier.Spectre;
		internal override string VanillaItemName => "QueenBeePetItem";
	}

	public class HoneyPotProjectile : WeakPumpkinBomb
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.penetrate = 2;
		}

		public override void Kill(int timeLeft)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				var source = Projectile.GetSource_Death();
				Gore.NewGore(source, Projectile.position, Vector2.Zero, Mod.Find<ModGore>("HoneyPotBottomGore").Type);
				Gore.NewGore(source, Projectile.position, Vector2.Zero, Mod.Find<ModGore>("HoneyPotLidGore").Type);
			}

			SoundEngine.PlaySound(new LegacySoundStyle(13, 0).WithVolume(0.5f), Projectile.Center);
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 32, 32, DustID.Honey2);
			}
		}
	}

	public class HoneyBeeBee : BumblingTransientMinion
	{
		protected override float inertia => 20;
		protected override float idleSpeed => 10;
		protected override int timeToLive => 120;
		protected override float distanceToBumbleBack => 2000f; // don't bumble back
		protected override float searchDistance => 220f;

		public override string Texture => "Terraria/Images/NPC_" + NPCID.BeeSmall;
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 2;
		}

		protected override void Move(Vector2 vector2Target, bool isIdle = false)
		{
			base.Move(vector2Target, isIdle);
			Projectile.rotation = 0.05f * Projectile.velocity.X;
			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Math.Abs(Projectile.velocity.Y) < Math.Abs(oldVelocity.Y))
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			} else if (Math.Abs(Projectile.velocity.X) < Math.Abs(oldVelocity.X))
			{
				Projectile.velocity.X = -oldVelocity.X;
			} else
			{
				// don't really understand what's going on in this case but that's ok
				return false; 
			}
			return false;
		}

		public override void Kill(int timeLeft)
		{
			for(int i = 0; i < 6; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
			}
		}
	}


	public class HoneyBeeMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<HoneyBeeMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.QueenBeePet;
		internal override int? FiredProjectileId => ProjectileType<HoneyPotProjectile>();
		internal bool UsingKnightAI => (leveledPetPlayer?.PetLevel ?? 0) >= (int)CombatPetTier.Spectre;

		internal override int GetAttackFrames(ICombatPetLevelInfo info) => UsingKnightAI ?
			Math.Max(16, 32 - 2 * info.Level) : base.GetAttackFrames(info);

		internal override float DamageMult => UsingKnightAI ? 0.75f : 1f;
		internal override LegacySoundStyle ShootSound => SoundID.Item17;

		internal WeaponHoldingDrawer weaponDrawer;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(Language.GetTextValue("ProjectileName.HoneyBee"));
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void LoadAssets()
		{
			Main.instance.LoadItem(ItemID.BeeKeeper);
			Main.instance.LoadItem(ItemID.AnkhShield);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.height = 24;
			DrawOriginOffsetY = -12;
			DrawOffsetX = -4;
			forwardDir = -1;
			weaponDrawer = new WeaponHoldingDrawer()
			{
				WeaponOffset = Vector2.Zero,
				WeaponHoldDistance = 32,
				ForwardDir = -1,
				AimMode = WeaponAimMode.FIXED,
				SpriteOrientation = WeaponSpriteOrientation.DIAGONAL,
				AttackDuration = 10
			};
		}

		public override Vector2 IdleBehavior()
		{
			Vector2 target = base.IdleBehavior();
			weaponDrawer.Update(Projectile, animationFrame);
			hsHelper.firedProjectileId = UsingKnightAI ? null : FiredProjectileId;
			return target;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.TargetedMovement(vectorToTargetPosition);
			if(hsHelper.lastShootFrame == animationFrame)
			{
				weaponDrawer.StartAttack(vectorToTargetPosition);
			}
		}

		internal override void AfterFiringProjectile()
		{
			if(UsingKnightAI && player.whoAmI == Main.myPlayer && vectorToTarget is Vector2 target) 
			{
				int projId = ProjectileType<HoneyBeeBee>();
				int extraCount = 1 + Main.rand.Next(2);
				for(int i = 0; i < extraCount; i++)
				{
					Vector2 launchTarget = target.RotatedByRandom(MathHelper.Pi / 3);
					launchTarget.SafeNormalize();
					launchTarget *= hsHelper.projectileVelocity / 2;
					hsHelper.FireProjectile(launchTarget, projId);
				}
				SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if(UsingKnightAI)
			{
				Texture2D texture = TextureAssets.Item[ItemID.BeeKeeper].Value;
				weaponDrawer.Draw(texture, lightColor);
			}
			return true;
		}

		public override void PostDraw(Color lightColor)
		{
			if(vectorToTarget is not null && UsingKnightAI)
			{
				Texture2D texture = TextureAssets.Item[ItemID.AnkhShield].Value;
				Vector2 holdOffset = new(-forwardDir * Projectile.spriteDirection * 12, 4);
				Rectangle bounds = new(0, 0, texture.Width, texture.Height);
				Vector2 origin = new(bounds.Width / 2, bounds.Height / 2); // origin should hopefully be more or less center of squire
				Vector2 pos = Projectile.Center + holdOffset;
				float r = Projectile.rotation + MathHelper.Pi / 2 * forwardDir * Projectile.spriteDirection;
				Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
					bounds, lightColor, r,
					origin, 0.5f, 0, 0);
			}
		}
	}
}
