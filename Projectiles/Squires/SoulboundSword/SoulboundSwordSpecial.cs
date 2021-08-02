using AmuletOfManyMinions.Projectiles.Minions;
using AmuletOfManyMinions.Projectiles.Squires.SquireBaseClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;

namespace AmuletOfManyMinions.Projectiles.Squires.SoulboundSword
{
	public class SoulboundDescendingArrow : ModProjectile
	{

		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundBow/SoulboundArrow";
		protected virtual Color LightColor => new Color(1f, 0f, 0.8f, 1f);
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			SquireGlobalProjectile.isSquireShot.Add(Projectile.type);
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			// projectile.ranged = false; //Bandaid fix
			Projectile.minion = true;
			Projectile.tileCollide = false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// make the arrow a bit bigger to hit things more reliably
			projHitbox.Inflate(32, 32);
			return projHitbox.Intersects(targetHitbox);
		}

		public override void AI()
		{
			base.AI();
			// start colliding with tiles 1/3 of the way down the screen
			Vector2 position = Projectile.position;
			Vector2 myScreenPosition = Main.player[Projectile.owner].Center 
				- new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			float collideCutoff = myScreenPosition.Y + Main.screenHeight / 3f;
			if(position.Y >= collideCutoff)
			{
				Tile tile = Framing.GetTileSafely((int)position.X / 16, (int)position.Y / 16);
				if(!tile.IsActive || position.Y > Main.player[Projectile.owner].position.Y)
				{
					Projectile.tileCollide = true;
				}
			}
			Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * 0.5f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				texture.Bounds, translucentColor, Projectile.rotation,
				texture.Bounds.Center.ToVector2(), 1, 0, 0);
			return false;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10, (int)Projectile.position.X, (int)Projectile.position.Y);
			// don't spawn an arrow on kill
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				int dustCreated = Dust.NewDust(Projectile.position, 1, 1, 255, velocity.X, velocity.Y, 50, default(Color), Scale: 1.4f);
				Main.dust[dustCreated].noGravity = true;
				Main.dust[dustCreated].velocity *= 0.8f;
			}
		}
	}

	public class SoulboundSpecialBow : SquireMinion
	{
		public override string Texture => "AmuletOfManyMinions/Projectiles/Squires/SoulboundBow/SoulboundBow";
		internal override int BuffId => BuffType<SoulboundSwordMinionBuff>();

		public SoulboundSpecialBow() : base(ItemType<SoulboundSwordMinionItem>()) { }

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.minionSlots = 0;
		}
		public override void OnSpawn()
		{
			base.OnSpawn();
			SpawnDust();
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
			SpawnDust();
		}

		public override void StandardTargetedMovement(Vector2 vectorToTargetPosition)
		{
			// spawn a downward facing arrow about halfway between the player and the mouse,
			// angled towards the mouse
			float spawnAngleRange = MathHelper.Pi / 16;
			Vector2 mousePos = syncedMouseWorld;
			float hoverX = (mousePos.X + player.position.X) / 2;
			Vector2 myScreenPosition = Main.player[Projectile.owner].Center 
				- new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			float hoverY = myScreenPosition.Y + 0.05f * Main.screenHeight; // hover 5% of the way down the screen
			Vector2 hoverPos = new Vector2(hoverX, hoverY);
			Vector2 attackAngle = mousePos - hoverPos;
			Projectile.Center = hoverPos;
			Projectile.rotation = attackAngle.ToRotation();
			if(animationFrame % 6 == 0)
			{
				Vector2 launchAngle = attackAngle.RotatedBy(
					Main.rand.NextFloat(spawnAngleRange) - spawnAngleRange/2);
				launchAngle.SafeNormalize();
				Vector2 launchOffset = launchAngle;
				launchAngle *= 20;
				launchOffset *= 6 + Main.rand.Next(-2, 2);
				Vector2 launchPos = hoverPos + Vector2.One * Main.rand.NextFloat(-12, 12) + launchOffset;
				Projectile.NewProjectile(
					Projectile.GetProjectileSource_FromThis(), 
					launchPos,
					launchAngle,
					ProjectileType<SoulboundDescendingArrow>(),
					Projectile.damage,
					Projectile.knockBack,
					Main.myPlayer);
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			// need to draw sprites manually for some reason
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Color translucentColor = new Color(lightColor.R, lightColor.G, lightColor.B, 0.5f);
			// regular version
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
				texture.Bounds, translucentColor, Projectile.rotation,
				texture.Bounds.Center.ToVector2(), 1, 0, 0);
			return false;
		}

		public override void AfterMoving()
		{
			base.AfterMoving();
			Projectile.tileCollide = false;
		}

		private void SpawnDust()
		{
			for (float i = 0; i < 2 * Math.PI; i += (float)Math.PI / 12)
			{
				Vector2 velocity = 1.5f * new Vector2((float)Math.Cos(i), (float)Math.Sin(i));
				int dustCreated = Dust.NewDust(Projectile.position, 1, 1, 255, velocity.X, velocity.Y, 50, default(Color), Scale: 1.4f);
				Main.dust[dustCreated].noGravity = true;
				Main.dust[dustCreated].velocity *= 0.8f;
			}
		}

		public override float MaxDistanceFromPlayer() => 2000f;
	}

}
