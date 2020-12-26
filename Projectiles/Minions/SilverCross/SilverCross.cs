using AmuletOfManyMinions.Dusts;
using AmuletOfManyMinions.Projectiles.Minions.MinonBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Projectiles.Minions.SilverCross
{
	public class SilverCrossMinionBuff : MinionBuff
	{
		public SilverCrossMinionBuff() : base(ProjectileType<SilverCrossMinion>()) { }
		public override void SetDefaults()
		{
			base.SetDefaults();
			DisplayName.SetDefault("Silver Cross");
			Description.SetDefault("Ne-ver let you go-o.");
		}
	}

	public class SilverCrossMinionItem : MinionItem<SilverCrossMinionBuff, SilverCrossMinion>
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Silver Cross Staff");
			Tooltip.SetDefault("Summons a spinning silver cross to fight for you!");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.damage = 9;
			item.knockBack = 0.5f;
			item.mana = 10;
			item.width = 28;
			item.height = 28;
			item.value = Item.buyPrice(0, 0, 2, 0);
			item.rare = ItemRarityID.White;
		}
	}


	// Uses ai[1] to check when to stop moving
	public class SilverCrossSplitProjectile : ModProjectile, ISpinningBladeMinion
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Minions/SilverCross/SilverCrossMinion";

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = true;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 10;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 60; // only hit an enemy once
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void AI()
		{
			if(projectile.timeLeft < projectile.ai[1])
			{
				projectile.velocity = Vector2.Zero;
			}
			projectile.rotation += 0.1f;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			SpinningBladeDrawer.DrawBlade(this, spriteBatch, lightColor, projectile.rotation);
			return false;
		}
	}

	public class SilverCrossMinion : SpinningBladeMinion<SilverCrossMinionBuff>, ISpinningBladeMinion
	{
		protected override int bladeType => ProjectileType<SilverCrossSplitProjectile>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Silver Cross");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			attackFrames = 60;
			idleInertia = 8;
		}

		protected override void DoDrift(Vector2 driftVelocity)
		{
			projectile.velocity = driftVelocity;
		}

		protected override void DoSpin(Vector2 spinVelocity)
		{
			projectile.velocity = spinVelocity;
		}

		protected override void StopSpin()
		{
			projectile.velocity = Vector2.Zero;
		}

		protected override void SummonSecondBlade(Vector2 vectorToTargetPosition)
		{
			if (Main.myPlayer == player.whoAmI)
			{
				Vector2 launchVelocity = vectorToTargetPosition;
				launchVelocity.SafeNormalize();
				launchVelocity *= SpinVelocity;
				npcVelocity = Main.npc[(int)targetNPCIndex].velocity;
				launchVelocity += launchVelocity;
				spinVector = launchVelocity;
				int projId = Projectile.NewProjectile(
					projectile.Center,
					launchVelocity,
					ProjectileType<SilverCrossSplitProjectile>(),
					projectile.damage,
					projectile.knockBack,
					player.whoAmI,
					ai1: SpinAnimationLength - SpinTravelLength);
				Main.projectile[projId].timeLeft = SpinAnimationLength + 1;
			}
		}
	}
}
