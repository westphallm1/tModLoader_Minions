using static AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses.CombatPetConvenienceMethods;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using AmuletOfManyMinions.Items.Accessories;
using System;
using Terraria.Audio;
using AmuletOfManyMinions.Core.Minions.Effects;
using AmuletOfManyMinions.Items.Armor;
using Terraria.ModLoader;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.LilEnt
{
	public class LilEntMinionBuff : MinionBuff
	{

		internal override int[] ProjectileTypes => new int[] { ProjectileType<LilEntMinion>() };
	}
	public class LilEntAccessory : ModItem
	{
		public override void SetStaticDefaults()
		{
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.Blue;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<MinionSpawningItemPlayer>().lilEntAccessoryEquipped = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.Topaz, 3).AddIngredient(ItemID.Vine, 2).AddIngredient(ItemID.Daybloom, 1).AddTile(TileID.WorkBenches).Register();
		}

	}

	public class LilEntLeafProjectile : BaseTrackingMushroom
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
			Main.projFrames[Projectile.type] = 5;
		}
		public override void Kill(int timeLeft)
		{
			// TODO dust
			for(int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 16, 16, DustID.Grass);
			}
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(minFrame, maxFrame);
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
	}

	public class LilEntMinion : SimpleGroundBasedMinion
	{
		public override int BuffId => BuffType<LilEntMinionBuff>();

		// TODO make the grounded ranged minion state generically available somehow
		internal int preferredDistanceFromTarget = 96;
		internal int lastFiredFrame = 0;

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(this, 24, 30, -5, -6);
			ConfigureFrames(7, (0, 0), (1, 6), (4, 4), (4, 4));
			xMaxSpeed = 9;
			attackFrames = 45;
			Projectile.minionSlots = 0;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GHelper.DoGroundAnimation(frameInfo, base.Animate);
			DoSimpleFlyingDust(DustID.Grass);
			if(VectorToTarget is Vector2 target && AnimationFrame - lastFiredFrame < 45)
			{
				Projectile.spriteDirection = Math.Sign(target.X);
			}
		}

		public virtual void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			lastFiredFrame = AnimationFrame;
			launchVector.SafeNormalize();
			launchVector *= 12;
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				VaryLaunchVelocity(launchVector),
				ProjectileType<LilEntLeafProjectile>(),
				Projectile.damage,
				Projectile.knockBack,
				Player.whoAmI);
		}

		protected override void DoGroundedMovement(Vector2 vector)
		{
			DoDefaultGroundedMovement(vector);
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{

			if (Math.Abs(vectorToTargetPosition.X) < 2 * preferredDistanceFromTarget &&
				Math.Abs(vectorToTargetPosition.Y) < 2 * preferredDistanceFromTarget &&
				AnimationFrame - lastFiredFrame >= attackFrames)
			{
				LaunchProjectile(vectorToTargetPosition);
			}

			// don't move if we're in range-ish of the target
			if (Math.Abs(vectorToTargetPosition.X) < 1.25f * preferredDistanceFromTarget && 
				Math.Abs(vectorToTargetPosition.X) > 0.5f * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X = 0;
			} else if (Math.Abs(vectorToTargetPosition.X) < 0.5f * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.X -= Math.Sign(vectorToTargetPosition.X) * 0.75f * preferredDistanceFromTarget;
			}

			if(Math.Abs(vectorToTargetPosition.Y) < 1.25f * preferredDistanceFromTarget && 
				Math.Abs(vectorToTargetPosition.Y) > 0.5 * preferredDistanceFromTarget)
			{
				vectorToTargetPosition.Y = 0;
			}
			base.TargetedMovement(vectorToTargetPosition);
		}

		public override bool CheckActive()
		{
			if (base.CheckActive() && !Player.GetModPlayer<MinionSpawningItemPlayer>().lilEntAccessoryEquipped)
			{
				Projectile.Kill();
				return false;
			}

			return true;
		}
	}
}
