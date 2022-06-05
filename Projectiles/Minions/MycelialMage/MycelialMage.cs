using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.MycelialMage
{
	public class MycelialMageMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<MycelialMageMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mycelial Mage");
			Description.SetDefault("A Mycelial Mage is fighting for you!");
		}
	}

	public class MycelialMageMinionItem : MinionItem<MycelialMageMinionBuff, MycelialMageMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mycelial Staff");
			Tooltip.SetDefault("Summons a Mycelial Mage to fight for you!");
		}
		public override void ApplyCrossModChanges()
		{
			CrossMod.WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, CrossMod.SummonersShineDefaultSpecialWhitelistType.RANGED);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.damage = 10;
			Item.knockBack = 0.5f;
			Item.mana = 10;
			Item.width = 34;
			Item.height = 34;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.GlowingMushroom, 15).AddRecipeGroup("AmuletOfManyMinions:Golds", 10).AddTile(TileID.Anvils).Register();
		}
	}

	public class MycelialMageMinion : HoverShooterMinion
	{
		internal override int BuffId => BuffType<MycelialMageMinionBuff>();

		internal override int? FiredProjectileId => ProjectileType<TrufflePetGlowingMushroom>();
		internal override SoundStyle? ShootSound => SoundID.Item17;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mycelial Mage");
			Main.projFrames[Projectile.type] = 8;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			DrawOriginOffsetY = -12;
			targetSearchDistance = 650;
			attackFrames = 50;
			frameSpeed = 20;
			hsHelper.attackFrames = attackFrames;
			hsHelper.travelSpeed = 9;
			hsHelper.projectileVelocity = 12;
			hsHelper.targetInnerRadius = 128;
			hsHelper.targetOuterRadius = 176;
			hsHelper.targetShootProximityRadius = 96;
			// go slower and smaller circle than other minions
			circleHelper.idleBumbleFrames = 90;
			circleHelper.idleBumbleRadius = 96;
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if(vectorToTarget is Vector2 target)
			{
				base.Animate(4, 8);
				Projectile.spriteDirection = Math.Sign(target.X);
			} else
			{
				base.Animate(0, 4);
				if(Math.Abs(Projectile.velocity.X) > 1)
				{
					Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
				}
			}
			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}
}
