using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;
using ReLogic.Content;

namespace AmuletOfManyMinions.Projectiles.Squires.BoneSquire
{
	public class BoneSquireMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<BoneSquireMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Squire");
			Description.SetDefault("A bone squire will follow your orders!");
		}
	}

	public class BoneSquireMinionItem : SquireMinionItem<BoneSquireMinionBuff, BoneSquireMinion>
	{
		protected override string SpecialName => "Flaming Flail";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Crest of Bones");
			Tooltip.SetDefault("Summons a squire\nA bone squire will fight for you!\nClick and hold to guide its attacks");
		}
		
		public override void ApplyCrossModChanges()
		{
			CrossMod.SummonersShineMinionPowerCollection minionCollection = new CrossMod.SummonersShineMinionPowerCollection();
			minionCollection.AddMinionPower(3.75f);
			CrossMod.BakeSummonersShineMinionPower_NoHooks(Item.type, minionCollection);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 7.0f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 40;
			Item.value = Item.sellPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.Orange;
		}
	}


	public class BoneSquireMinion : WeaponHoldingSquire
	{
		internal override int BuffId => BuffType<BoneSquireMinionBuff>();
		protected override int ItemType => ItemType<BoneSquireMinionItem>();
		protected override int AttackFrames => usingSpecial ? 20 : 35;
		protected override string WingTexturePath => "AmuletOfManyMinions/Projectiles/Squires/Wings/BoneWings";

		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/BoneSquire/BoneSquireFlailBall";
		protected string FlamingFlailPath = "Terraria/Images/NPC_72";

		protected static string ChainTexturePath = "AmuletOfManyMinions/Projectiles/Squires/BoneSquire/BoneSquireFlailChain";
		protected static string FlamingChainTexturePath = ChainTexturePath + "_Flaming";
		// swing weapon in a full circle
		protected override float SwingAngle1 => SwingAngle0 - 2 * (float)Math.PI;

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override float knockbackSelf => 5f;
		protected override Vector2 WingOffset => new Vector2(-4, 2);

		protected override int SpecialDuration => 4 * 60;
		protected override int SpecialCooldown => 10 * 60;

		protected override Asset<Texture2D> WeaponTexture => usingSpecial ? ExtraTextures[3] : base.WeaponTexture;

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 30;
			Projectile.localNPCHitCooldown = AttackFrames / 3;
		}

		public override void LoadAssets()
		{
			base.LoadAssets();
			AddTexture(ChainTexturePath);
			AddTexture(FlamingFlailPath);
			AddTexture(FlamingChainTexturePath);
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Bone Squire");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
			CrossMod.SetSummonersShineProjMaxEnergy(Projectile.type, 0);
		}
		
		public override void ApplyCrossModChanges()
		{
			CrossMod.SetSummonersShineProjMaxEnergy(Projectile.type, 0);
		}

		protected override Rectangle GetWeaponTextureBounds(Texture2D texture)
		{
			if(usingSpecial)
			{
				int frame = (specialFrame / 5) % 4;
				int frameHeight = texture.Height / 4;
				return new Rectangle(0, frameHeight * frame, texture.Width, frameHeight);
			} else
			{
				return base.GetWeaponTextureBounds(texture);
			}
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			if(usingSpecial && attackFrame == 0)
			{
				SoundEngine.PlaySound(attackSound, Projectile.Center);
			}
			base.StandardTargetedMovement(vectorToTargetPosition);
		}
		protected override void DrawWeapon(Color lightColor)
		{
			base.DrawWeapon(usingSpecial ? Color.White : lightColor);
		}
		public override void PostDraw(Color lightColor)
		{
			if (IsAttacking())
			{
				Texture2D chainTexture = ExtraTextures[usingSpecial ? 4 : 2].Value;
				ChainDrawer drawer = new ChainDrawer(chainTexture.Bounds);
				Vector2 center = Projectile.Center + WeaponCenterOfRotation;
				Vector2 chainVector = UnitVectorFromWeaponAngle() * WeaponDistanceFromCenter();
				drawer.DrawChain(chainTexture, center, center + chainVector, usingSpecial ? Color.White : default);
			}
			base.PostDraw(lightColor);
		}

		private void DrawFlailFlames(int iterations)
		{
			for (int i = 0; i < iterations; i++)
			{
				int dustId = Dust.NewDust(
					lastWeaponPos - new Vector2(16, 16), 
					32, 32, 6, 
					Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 
					100, default, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity.X *= 0.3f;
				Main.dust[dustId].velocity.Y *= 0.3f;
				Main.dust[dustId].noLight = true;
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
			if(usingSpecial)
			{
				damage = 5 * damage / 4; // 25% damage boost
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			base.OnHitNPC(target, damage, knockback, crit);
			if(usingSpecial)
			{
				target.AddBuff(BuffID.OnFire, 300);
			}
		}

		public override void OnStartUsingSpecial()
		{
			DrawFlailFlames(10);
		}

		public override void AfterMoving()
		{
			if(usingSpecial) 
			{ 
				DrawFlailFlames(2);
			}
		}

		public override void OnStopUsingSpecial() => OnStartUsingSpecial();


		protected override float WeaponDistanceFromCenter() => 16 * CrossMod.ApplyCrossModScaling(3.75f, Projectile, 0);

		protected override int WeaponHitboxStart() => (int)WeaponDistanceFromCenter() - 10;
		protected override int WeaponHitboxEnd() => (int)WeaponDistanceFromCenter() + 10;

		public override float ComputeIdleSpeed() => usingSpecial ? 11 : 9;

		public override float ComputeTargetedSpeed() => usingSpecial ? 11 : 9;

		public override float MaxDistanceFromPlayer() => usingSpecial ? 260 : 224;
	}
}
