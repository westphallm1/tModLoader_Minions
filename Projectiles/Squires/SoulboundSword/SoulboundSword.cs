using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Squires.SoulboundSword
{
	public class SoulboundSwordMinionBuff : MinionBuff
	{
		internal override int[] ProjectileTypes => new int[] { ProjectileType<SoulboundSwordMinion>() };
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulbound Sword");
			Description.SetDefault("A soulbound sword will follow your orders!");
		}
	}

	public class SoulboundSwordMinionItem : SquireMinionItem<SoulboundSwordMinionBuff, SoulboundSwordMinion>
	{

		protected override string SpecialName => "Soulbound Companion";
		protected override string SpecialDescription => "The Soulbound Bow will briefly assist you";
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundSword/SoulboundSword";
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulbound Sword");
			Tooltip.SetDefault("Summons a squire\nAn enchanted sword will fight for you!\nClick and hold to guide its attacks");
		}
		
		public override void ApplyCrossModChanges()
		{
			CrossMod.WhitelistSummonersShineMinionDefaultSpecialAbility(Item.type, CrossMod.SummonersShineDefaultSpecialWhitelistType.RANGEDNOINSTASTRIKE);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.knockBack = 3f;
			Item.width = 24;
			Item.height = 38;
			Item.damage = 34;
			Item.value = Item.buyPrice(0, 0, 50, 0);
			Item.rare = ItemRarityID.LightRed;
			Item.noUseGraphic = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddRecipeGroup("AmuletOfManyMinions:EvilWoodSwords").AddIngredient(ItemID.SoulofNight, 10).AddTile(TileID.Anvils).Register();
		}

		public override void UseAnimation(Player player)
		{
			base.UseAnimation(player);
			Item.noUseGraphic = true;
		}
	}

	public class SoulboundSwordMinion : WeaponHoldingSquire
	{
		public override int BuffId => BuffType<SoulboundSwordMinionBuff>();
		protected override int ItemType => ItemType<SoulboundSwordMinionItem>();
		protected override int AttackFrames => 15;
		protected override string WingTexturePath => null;
		protected override string WeaponTexturePath => "AmuletOfManyMinions/Projectiles/Squires/SoulboundSword/SoulboundSword";

		public override string Texture => "Terraria/Images/Item_0";

		protected override WeaponAimMode aimMode => WeaponAimMode.FIXED;

		protected override Vector2 WeaponCenterOfRotation => new Vector2(0, 4);

		protected override float knockbackSelf => 4;

		protected override int SpecialDuration => 2 * 60;
		protected override int SpecialCooldown => 9 * 60;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Soulbound Sword");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
		}

		public sealed override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 22;
			Projectile.height = 32;
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * 0.5f);
			return base.IdleBehavior();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!usingWeapon && attackFrame == 0)
			{
				weaponAngle = -9 * (float)Math.PI / 16 +
					Projectile.velocity.X * -Projectile.spriteDirection * 0.01f;
				Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
				DrawWeapon(translucentColor);
			}
			return false;
		}

		public override void PostDraw(Color lightColor)
		{

			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			base.PostDraw(translucentColor);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			// nice little dust effect on hit, but not actually shadowflame
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(target.position, target.width, target.height, DustID.Shadowflame);
			}
		}

		public override void SpecialTargetedMovement(Vector2 vectorToTargetPosition)
		{
			base.IdleMovement(VectorToIdle);
		}

		public override void OnStartUsingSpecial()
		{
			if(Player.whoAmI == Main.myPlayer)
			{
				Projectile p = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center, 
					Projectile.velocity, 
					ProjectileType<SoulboundSpecialBow>(), 
					5 * Projectile.damage / 4, 
					Projectile.knockBack, 
					Player.whoAmI);
				p.originalDamage = Projectile.originalDamage;
			}
		}

		public override void OnStopUsingSpecial()
		{
			if(Player.whoAmI == Main.myPlayer)
			{
				for(int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if(p.active && p.owner == Player.whoAmI && p.type == ProjectileType<SoulboundSpecialBow>())
					{
						p.Kill();
						break;
					}
				}
			}
		}

		protected override float WeaponDistanceFromCenter() => 30;

		protected override int WeaponHitboxStart() => 30;

		protected override int WeaponHitboxEnd() => 50;

		public override float ComputeIdleSpeed() => 13;

		public override float ComputeTargetedSpeed() => 13;

		public override float MaxDistanceFromPlayer() => 250;
	}
}
