using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Armor.IllusionistArmor
{
	public abstract class BaseIllusionistHood : ModItem
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Illusionist Hood");
			Tooltip.SetDefault("+1 Max Minions");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(silver: 3);
			item.rare = ItemRarityID.Orange;
			item.defense = 4;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return (body.type == ItemType<IllusionistCorruptRobe>() || body.type == ItemType<IllusionistCrimsonRobe>()) && 
				(legs.type == ItemType<IllusionistCorruptLeggings>() || legs.type == ItemType<IllusionistCrimsonLeggings>());
		}

		public override void UpdateEquip(Player player)
		{
			player.maxMinions += 1;
		}

		public override void ArmorSetShadows(Player player)
		{
			if(player.GetModPlayer<MinionSpawningItemPlayer>().illusionistArmorSetEquipped)
			{
				player.armorEffectDrawOutlines = true;
			}
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "+1 Max Minions\n"
				+ "Wisps will spawn as your minions deal damage!";
			player.maxMinions++;
			player.GetModPlayer<MinionSpawningItemPlayer>().illusionistArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}
	}


	[AutoloadEquip(EquipType.Head)]
	public class IllusionistCorruptHood: BaseIllusionistHood
	{

	}

	[AutoloadEquip(EquipType.Head)]
	public class IllusionistCrimsonHood: BaseIllusionistHood
	{

	}

	public class IllusionistWispBuff : ModBuff
	{

		public override void SetDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			DisplayName.SetDefault("Illusion Wisps");
			Description.SetDefault("Illusion Wisps are spawning around you...");
		}
	}

	public class IllusionistWisp : TransientMinion
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 10;
			projectile.width = 16;
			projectile.height = 16;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.timeLeft = 62;
			attackThroughWalls = true;
			frameSpeed = 5;
		}
		public override Vector2 IdleBehavior()
		{
			List<Projectile> others = GetMinionsOfType(projectile.type);
			int myIndex = others.FindIndex(p => p.whoAmI == projectile.whoAmI);
			if(player.GetModPlayer<MinionSpawningItemPlayer>().illusionistArmorSetEquipped)
			{
				projectile.timeLeft = Math.Max(projectile.timeLeft, 2);
			}

			Vector2 offsetVector;
			switch(myIndex)
			{
				case 0:
					offsetVector = new Vector2(24, 0);
					break;
				case 1:
					offsetVector = new Vector2(-24, 0);
					break;
				default:
					offsetVector = new Vector2(0, -40);
					break;
			}
			Lighting.AddLight(projectile.Center, Color.LimeGreen.ToVector3() * 0.25f);
			return player.Center - projectile.position +  offsetVector;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(0, 5);
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			projectile.position += vectorToIdlePosition;
			projectile.velocity = Vector2.Zero;
			projectile.friendly = false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Color translucentColor = new Color(66, 213, 0, 100);
			float r = projectile.rotation;
			Vector2 pos = projectile.Center;
			SpriteEffects effects = player.direction < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = GetTexture(Texture);
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			Rectangle bounds = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			spriteBatch.Draw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, 0.75f, effects, 0);
			return false;
		}
	}
}