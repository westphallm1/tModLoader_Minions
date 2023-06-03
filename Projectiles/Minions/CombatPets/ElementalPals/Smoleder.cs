using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using AmuletOfManyMinions.Projectiles.Squires.PumpkinSquire;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using AmuletOfManyMinions.Core;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals
{
	public class SmolederMinionBuff : CombatPetBuff
    {
        internal override int[] ProjectileTypes => new int[] { ProjectileType<SmolederMinion>() };
	}

	public class SmolederMinionItem : CombatPetCustomMinionItem<SmolederMinionBuff, SmolederMinion>
	{
	}

	public class SmolederMinion : CombatPetGroundedRangedMinion
	{
		public override int BuffId => BuffType<SmolederMinionBuff>();

		internal override bool ShouldDoShootingMovement => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Skeletal;

		internal override int? ProjId => leveledPetPlayer.PetLevel >= (int)CombatPetTier.Spectre ? 
			ProjectileType<FlareVortexProjectile>() :
			ProjectileType<ImpFireball>();

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 2, 10)
				.WithSpriteDirection(-1)
				.WithOffset(-2, 0)
				.WhenSelected(2, 5, 5)
				.WithCode(Preview);
		}

		private static void Preview(Projectile proj, bool walking)
		{
			var smol = (SmolederMinion)proj.ModProjectile;
			smol.AnimationFrame++;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			ConfigureDrawBox(30, 24, -6, -4, -1);
			ConfigureFrames(8, (0, 1), (2, 6), (2, 2), (7, 7));
		}

		public override void LaunchProjectile(Vector2 launchVector, float? ai0 = null)
		{
			if(leveledPetPlayer.PetLevel >= (int)CombatPetTier.Spectre)
			{
				launchVector *= 0.6f; // slow down for nicer visual effect, might make it slightly worse
			}
			base.LaunchProjectile(launchVector, ai0);
		}

		public override void LoadAssets()
		{
			AddTexture(Texture + "_Flames");
		}

		public override Vector2 IdleBehavior()
		{
			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.25f);
			return base.IdleBehavior();
		}
		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			GroundAnimationState state = GHelper.GetAnimationState();
			FrameSpeed = (state == GroundAnimationState.WALKING) ? 5 : 10;
			base.Animate(minFrame, maxFrame);
			if(state == GroundAnimationState.JUMPING)
			{
				Projectile.frame = Projectile.velocity.Y > 0 ? 2 : 5;
			} else if (state == GroundAnimationState.FLYING && Main.rand.NextBool(6))
			{
				int dustId = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0f, 0f, 100, default, 2f);
				Main.dust[dustId].velocity *= 0.3f;
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].noLight = true;
			} 
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D flameTexture = ExtraTextures[0].Value;
			int flameFrame = (AnimationFrame / 5) % 8;
			int frameHeight = flameTexture.Height / 8;
			Rectangle bounds = new(0, flameFrame * frameHeight, flameTexture.Width, frameHeight);
			Vector2 baseOffset = new Vector2(-6 * Projectile.spriteDirection * forwardDir, -16).RotatedBy(Projectile.rotation);
			SpriteEffects effects = Projectile.spriteDirection == forwardDir ? 0 : SpriteEffects.FlipHorizontally;
			// regular version
			Main.EntitySpriteDraw(flameTexture, Projectile.Center + baseOffset - Main.screenPosition,
				bounds, Color.White, Projectile.rotation, bounds.GetOrigin(), 1, effects, 0);
			return true;
		}
	}
}
