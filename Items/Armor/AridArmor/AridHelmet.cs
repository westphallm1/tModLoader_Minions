using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Armor.AridArmor
{
	[AutoloadEquip(EquipType.Head)]
	public class AridHelmet : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Arid Helmet");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 8%\n" +
				"Increases squire travel range by 3 blocks");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(silver: 3);
			item.rare = ItemRarityID.Green;
			item.defense = 5;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<AridBreastplate>() && legs.type == ItemType<AridLeggings>();
		}

		public override void UpdateEquip(Player player)
		{
			player.minionDamageMult += 0.08f;
			player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus += 48;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Increases minion damage by 15%\n"
				+ "Increases squire travel range by 1 block\n"
				+ "An Angry Tumbler will assist your squire in combat!";
			player.minionDamageMult += 0.15f;
			player.GetModPlayer<SquireModPlayer>().squireRangeFlatBonus += 16f;
			player.GetModPlayer<SquireModPlayer>().aridArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.AncientCloth, 3);
			recipe.AddIngredient(ItemID.AntlionMandible, 3);
			recipe.AddIngredient(ItemID.FossilOre, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
	public class AridTumblerProjectile : SquireBoomerangMinion
	{

		protected override int idleVelocity => 12;

		protected override int targetedVelocity => 12;

		protected override int inertia => 4;

		protected override int attackRange => 220;

		protected override int attackCooldown => 45;

		private static int AnimationFrames = 75;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 18;
			projectile.height = 18;
		}

		public override Vector2 IdleBehavior()
		{
			int angleFrame = animationFrame % AnimationFrames;
			float angle = 2 * (float)(Math.PI * angleFrame) / AnimationFrames;
			float radius = 30;
			Vector2 angleVector = radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			// offset downward vertically a bit
			// the scale messes with the positioning in some way
			return base.IdleBehavior() + angleVector + new Vector2(0, 8);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			// draw the body
			Texture2D texture = GetTexture(Texture);
			Rectangle bounds = texture.Bounds;
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, lightColor, r,
				origin, 0.75f, 0, 0);
			// draw the eyes
			Texture2D eyesTexture = GetTexture(Texture + "_Eyes");
			SpriteEffects effects = animationFrame % AnimationFrames < AnimationFrames / 2 ? 0 : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(eyesTexture, pos - Main.screenPosition,
				bounds, Color.White, 0,
				origin, 0.75f, effects, 0);
			return false;
		}

		protected override bool IsEquipped(SquireModPlayer player)
		{
			return player.aridArmorSetEquipped;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			if (squire == null)
			{
				return;
			}
			if (vectorToTarget is null || returning)
			{
				projectile.rotation += squire.spriteDirection * 0.2f;
			}
			else
			{
				projectile.rotation += squire.spriteDirection * 0.3f;
			}
		}
	}
}