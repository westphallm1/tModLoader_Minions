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
			Tooltip.SetDefault("Increases your max number of minions by 1" +
							   "\nIncreases minion damage by 4%");

			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 3);
			Item.rare = ItemRarityID.Orange;
			Item.defense = 4;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return (body.type == ItemType<IllusionistCorruptRobe>() || body.type == ItemType<IllusionistCrimsonRobe>()) &&
				(legs.type == ItemType<IllusionistCorruptLeggings>() || legs.type == ItemType<IllusionistCrimsonLeggings>());
		}

		public override void UpdateEquip(Player player)
		{
			player.maxMinions += 1;
			player.GetDamage<SummonDamageClass>() += 0.04f;
		}

		public override void ArmorSetShadows(Player player)
		{
			if (player.GetModPlayer<MinionSpawningItemPlayer>().illusionistArmorSetEquipped)
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
	public class IllusionistCorruptHood : BaseIllusionistHood
	{
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.ShadowScale, 10).AddIngredient(ItemID.Bone, 20).AddTile(TileID.Anvils).Register();
		}

	}

	[AutoloadEquip(EquipType.Head)]
	public class IllusionistCrimsonHood : BaseIllusionistHood
	{
		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.TissueSample, 10).AddIngredient(ItemID.Bone, 20).AddTile(TileID.Anvils).Register();
		}

	}

	public class IllusionistWispBuff : ModBuff
	{

		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			DisplayName.SetDefault("Illusion Wisps");
			Description.SetDefault("Illusion Wisps are spawning around you...");
		}
	}

	// Uses ai[0] for corrupt vs. crimson
	// ai[1] for attack synchronization
	public class IllusionistWisp : TransientMinion
	{
		public static int SpawnFrequency = 60;
		private bool isCorrupt => Projectile.ai[0] == 0;
		internal override bool tileCollide => false;
		private int targetNPCIdx => (int)Projectile.ai[1];
		private NPC targetNPC;

		bool isAttacking = false;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[Projectile.type] = 10;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 62;
			attackThroughWalls = true;
			frameSpeed = 5;
			Projectile.penetrate = 1;
		}
		public override Vector2 IdleBehavior()
		{
			List<Projectile> others = GetMinionsOfType(Projectile.type);
			int myIndex = others.FindIndex(p => p.whoAmI == Projectile.whoAmI);
			if (player.GetModPlayer<MinionSpawningItemPlayer>().illusionistArmorSetEquipped)
			{
				Projectile.timeLeft = Math.Max(Projectile.timeLeft, 2);
			}

			if (targetNPCIdx != 0 && !isAttacking)
			{
				Projectile.netUpdate = true;
				targetNPC = Main.npc[targetNPCIdx];
				isAttacking = true;
			}
			if (isAttacking && targetNPC != null && !targetNPC.active)
			{
				Projectile.Kill();
			}

			Vector2 offsetVector;
			switch (myIndex)
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
			offsetVector.Y += 4 * (float)Math.Sin(2 * Math.PI * animationFrame / 120);
			Lighting.AddLight(Projectile.Center, Color.LimeGreen.ToVector3() * 0.25f);
			return player.Center - Projectile.Center + offsetVector;
		}

		public override void Animate(int minFrame = 0, int? maxFrame = null)
		{
			base.Animate(0, 5);
		}
		public override void IdleMovement(Vector2 vectorToIdlePosition)
		{
			Projectile.position += vectorToIdlePosition;
			Projectile.velocity = Vector2.Zero;
		}

		public override void Kill(int timeLeft)
		{
			int dustType = isCorrupt ? 89 : 87;
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(Projectile.position, 16, 16, dustType);
			}
		}

		public override void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			vectorToTargetPosition.Normalize();
			vectorToTargetPosition *= 8;
			int inertia = 8;
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
		}

		public override Vector2? FindTarget()
		{
			if (!isAttacking)
			{
				return null;
			}
			else
			{
				return Main.npc[targetNPCIdx].Center - Projectile.Center;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color translucentColor = isCorrupt ? new Color(160, 213, 137, 100) : new Color(253, 204, 129, 100);
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			SpriteEffects effects = player.direction < 0 ? 0 : SpriteEffects.FlipHorizontally;
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle bounds = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			float scale = 1f - Projectile.timeLeft / 62f;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, translucentColor, r,
				origin, scale, effects, 0);
			return false;
		}
	}
}