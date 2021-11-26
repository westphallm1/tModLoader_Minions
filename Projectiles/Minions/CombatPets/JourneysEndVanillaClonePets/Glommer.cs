using AmuletOfManyMinions.Projectiles.Minions.CombatPets.CombatPetBaseClasses;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using AmuletOfManyMinions.Projectiles.Minions.CombatPets.MasterModeBossPets;
using Terraria.Audio;
using Terraria;
using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AmuletOfManyMinions.Projectiles.Minions.CombatPets.VanillaClonePets
{
	public class GlommerMinionBuff : CombatPetVanillaCloneBuff
	{
		public GlommerMinionBuff() : base(ProjectileType<GlommerMinion>()) { }
		public override string VanillaBuffName => "GlommerPet";
		public override int VanillaBuffId => BuffID.GlommerPet;
	}

	public class GlommerMinionItem : CombatPetMinionItem<GlommerMinionBuff, GlommerMinion>
	{
		internal override string VanillaItemName => "GlommerPetItem";
		internal override int VanillaItemID => ItemID.GlommerPetItem;
	}

	public class GlommerMinion : CombatPetHoverShooterMinion
	{
		internal override int BuffId => BuffType<GlommerMinionBuff>();
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GlommerPet;
		internal override int? FiredProjectileId => null;
		internal override bool DoBumblingMovement => true;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 24;
			IdleLocationSets.circlingHead.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
			int frameWidth = texture.Width / 3;
			int frameHeight = texture.Height / 12;
			int xFrame = Projectile.frame / 12;
			int yFrame = Projectile.frame % 12;
			Rectangle bounds = new Rectangle(frameWidth * xFrame, yFrame * frameHeight, frameWidth, frameHeight);
			Vector2 origin = new Vector2(bounds.Width, bounds.Height) / 2;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				bounds, lightColor, Projectile.rotation, origin, 1, effects, 0);
			return false;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			// TODO something with the unused phase in/out frames
			base.Animate(0, 3);
		}
	}
}
