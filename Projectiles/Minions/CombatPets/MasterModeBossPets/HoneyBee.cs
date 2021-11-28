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
		public HoneyBeeMinionBuff() : base(ProjectileType<HoneyBeeMinion>()) { }

		public override int VanillaBuffId => BuffID.QueenBeePet;

		public override string VanillaBuffName => "QueenBeePet";
	}

	public class HoneyBeeMinionItem : CombatPetMinionItem<HoneyBeeMinionBuff, HoneyBeeMinion>
	{
		internal override int VanillaItemID => ItemID.QueenBeePetItem;
		internal override int AttackPatternUpdateTier => 4;
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
			SoundEngine.PlaySound(new LegacySoundStyle(13, 0).WithVolume(0.5f), Projectile.Center);
			Gore.NewGore(Projectile.position, Vector2.Zero, Mod.Find<ModGore>("HoneyPotBottomGore").Type);
			Gore.NewGore(Projectile.position, Vector2.Zero, Mod.Find<ModGore>("HoneyPotLidGore").Type);
			for(int i = 0; i < 3; i++)
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

		public override string Texture => "Terraria/Images/NPC_" + NPCID.Bee;
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
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
	}


	public class HoneyBeeMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<HoneyBeeMinionBuff>();

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.QueenBeePet;
		internal override int? FiredProjectileId => ProjectileType<HoneyPotProjectile>();
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
			if(leveledPetPlayer.PetLevel >= 4)
			{
			} else
			{
				hsHelper.AfterFiringProjectile = null;
			}
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

		public override bool PreDraw(ref Color lightColor)
		{
			if(leveledPetPlayer.PetLevel >= 4)
			{
				Texture2D texture = TextureAssets.Item[ItemID.BeeKeeper].Value;
				weaponDrawer.Draw(texture, lightColor);
			}
			return true;
		}

		public override void PostDraw(Color lightColor)
		{
			if(vectorToTarget is not null && leveledPetPlayer.PetLevel >= 4)
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
