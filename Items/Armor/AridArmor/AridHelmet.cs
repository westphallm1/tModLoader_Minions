using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Armor.AridArmor
{
	[AutoloadEquip(EquipType.Head)]
	public class AridHelmet : ModItem
	{
		public static readonly int SetBonusDamageIncrease = 15;
		public static readonly int SetBonusSquireTravelRangeIncrease = 1;
		public static readonly int MinionDamageIncrease = 8;
		public static readonly int SquireRangeIncrease = 3;
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionDamageIncrease, SquireRangeIncrease);
		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(SetBonusDamageIncrease, SetBonusSquireTravelRangeIncrease);
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 3);
			Item.rare = ItemRarityID.Green;
			Item.defense = 5;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<AridBreastplate>() && legs.type == ItemType<AridLeggings>();
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage<SummonDamageClass>() += MinionDamageIncrease / 100f;
			player.GetModPlayer<SquireModPlayer>().SquireRangeFlatBonus += SquireRangeIncrease * 16f;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = SetBonusText.ToString();
			player.GetDamage<SummonDamageClass>() += SetBonusDamageIncrease / 100f;
			SquireModPlayer squireModPlayer = player.GetModPlayer<SquireModPlayer>();
			squireModPlayer.SquireRangeFlatBonus += SetBonusSquireTravelRangeIncrease * 16;
			squireModPlayer.aridArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.AncientCloth, 3).AddIngredient(ItemID.AntlionMandible, 3).AddIngredient(ItemID.FossilOre, 10).AddTile(TileID.Anvils).Register();
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
			Main.projFrames[Projectile.type] = 1;
		}

		public override void LoadAssets()
		{
			AddTexture(Texture + "_Eyes");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Projectile.width = 18;
			Projectile.height = 18;
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

		public override bool PreDraw(ref Color lightColor)
		{
			float r = Projectile.rotation;
			Vector2 pos = Projectile.Center;
			// draw the body
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Rectangle bounds = texture.Bounds;
			Main.EntitySpriteDraw(texture, pos - Main.screenPosition,
				bounds, lightColor, r, bounds.GetOrigin(), 0.75f, 0, 0);
			// draw the eyes
			Texture2D eyesTexture = ExtraTextures[0].Value;
			SpriteEffects effects = animationFrame % AnimationFrames < AnimationFrames / 2 ? 0 : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(eyesTexture, pos - Main.screenPosition,
				bounds, Color.White, 0, bounds.GetOrigin(), 0.75f, effects, 0);
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
			if (VectorToTarget is null || returning)
			{
				Projectile.rotation += squire.spriteDirection * 0.2f;
			}
			else
			{
				Projectile.rotation += squire.spriteDirection * 0.3f;
			}
		}
	}
}