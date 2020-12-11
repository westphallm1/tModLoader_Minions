using AmuletOfManyMinions.Items.Accessories;
using AmuletOfManyMinions.Projectiles.NonMinionSummons;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class ForagerHelmet : NecromancerAccessory
	{
		protected override float baseDamage => 5;
		protected override int bossLifePerSpawn => 250;
		protected override int maxTransientMinions => 2;
		protected override float onKillChance => .2f;

		protected override int projType => ProjectileType<ForagerMushroom>();

		protected override float spawnVelocity => 9;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Mildew Cap");
			Tooltip.SetDefault(""
				+ "Increases minion damage by 1");
		}

		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.value = Item.sellPrice(silver: 3);
			item.rare = ItemRarityID.White;
			item.defense = 1;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<ForagerBreastplate>() && legs.type == ItemType<ForagerLeggings>();
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<MinionSpawningItemPlayer>().summonFlatDamage += 1;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Increases your max minions by 1\n"
				+ "Your minions will release damaging fungi while attacking";
			player.maxMinions++;
			player.GetModPlayer<MinionSpawningItemPlayer>().foragerArmorSetEquipped = true;
			// insert whatever variable needs to be activated so the player's minions will release homing fungi spores similar to the fungi bulb, but just recolored to look like a mushroom.
		}
		public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
		{
			drawAltHair = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Mushroom, 4);
			recipe.AddIngredient(ItemID.Wood, 12);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		internal override bool IsEquipped(MinionSpawningItemPlayer player)
		{
			return player.foragerArmorSetEquipped;
		}
	}
	public class ForagerMushroom : BumblingTransientMinion
	{
		protected override int timeToLive => 60 * 3; // 3 seconds;
		protected override float inertia => 12;
		protected override float idleSpeed => maxSpeed * 0.75f;
		protected override float searchDistance => 300f;
		protected override float distanceToBumbleBack => 8000f; // don't bumble back

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 1;
		}
		public override void SetDefaults()
		{
			base.SetDefaults();
			frameSpeed = 15;
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = true;
			projectile.penetrate = 1;
			projectile.friendly = false;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDust(projectile.Center - Vector2.One * 16, 32, 32, DustID.Copper);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return oldVelocity.Y > 0 && projectile.velocity.X == oldVelocity.X;
		}

		protected override void Move(Vector2 vector2Target, bool isIdle = false)
		{
			if (projectile.timeLeft < timeToLive - 30)
			{
				// enforce drifting left and right
				float lifeFraction = projectile.timeLeft / (float)timeToLive;
				vector2Target.X += 12 * (1 - lifeFraction) * (float)Math.Sin(10 * PI * lifeFraction);
			}
			base.Move(vector2Target, isIdle);
			if (projectile.timeLeft > timeToLive - 30)
			{
				projectile.friendly = false;
				projectile.velocity.Y = -3;
			}
			else
			{
				projectile.friendly = true;
				projectile.velocity.Y = 1.5f;
			}
		}



	}
}