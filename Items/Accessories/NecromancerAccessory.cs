using AmuletOfManyMinions.Items.Armor.IllusionistArmor;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories
{
	public abstract class NecromancerAccessory : ModItem
	{
		internal static List<NecromancerAccessory> accessories;
		protected virtual float spawnVelocity => 0;

		protected virtual float onKillChance => 0;
		protected virtual int bossLifePerSpawn => 0;
		protected virtual int projType => 0;
		protected virtual int maxTransientMinions => 0;
		protected virtual float baseDamage => 0;

		public static void Load()
		{
			accessories = new List<NecromancerAccessory>();
		}

		public static void Unload()
		{
			accessories?.Clear();
			accessories = null;
		}

		internal virtual void ModifyPlayerWeaponDamage(MinionSpawningItemPlayer necromancerAccessoryPlayer, Item item, ref float add, ref float mult, ref float flat)
		{
			// no op
		}

		public override void SetStaticDefaults()
		{
			accessories.Add(this);
		}
		internal virtual bool SpawnProjectileOnChance(Projectile projectile, NPC target, int damage)
		{
			Player player = Main.player[projectile.owner];
			bool shouldSpawnProjectile = player.whoAmI == Main.myPlayer && !target.boss && target.life <= 0 && Main.rand.NextFloat() < onKillChance;
			shouldSpawnProjectile |= Main.rand.NextFloat() < damage / (float)bossLifePerSpawn;
			if (!shouldSpawnProjectile)
			{
				return false;
			}
			Vector2 spawnVelocity = projectile.velocity;
			spawnVelocity.SafeNormalize();
			spawnVelocity *= this.spawnVelocity;
			spawnVelocity.Y = -Math.Abs(spawnVelocity.Y);
			var currentProjectiles = new List<Projectile>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.type == projType && p.owner == player.whoAmI)
				{
					currentProjectiles.Add(p);
				}
			}
			if (currentProjectiles.Count >= maxTransientMinions)
			{
				Projectile oldest = currentProjectiles.OrderBy(p => p.timeLeft).FirstOrDefault();
				if (oldest != default)
				{
					oldest.Kill();
				}
			}
			Projectile.NewProjectile(target.Center, spawnVelocity, projType, (int)(baseDamage * player.minionDamageMult), 2, player.whoAmI);
			return true;
		}

		internal abstract bool IsEquipped(MinionSpawningItemPlayer player);
	}

	internal class MinionSpawningItemPlayer : ModPlayer
	{
		public bool wormOnAStringEquipped = false;
		public bool spiritCallerCharmEquipped = false;
		public bool techromancerAccessoryEquipped = false;
		internal bool foragerArmorSetEquipped;
		internal int summonFlatDamage;
		internal bool illusionistArmorSetEquipped;

		public override void ResetEffects()
		{
			wormOnAStringEquipped = false;
			spiritCallerCharmEquipped = false;
			techromancerAccessoryEquipped = false;
			foragerArmorSetEquipped = false;
			illusionistArmorSetEquipped = false;
			summonFlatDamage = 0;
		}

		public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat)
		{
			if (!item.summon)
			{
				return;
			}
			foreach (NecromancerAccessory accessory in NecromancerAccessory.accessories)
			{
				if (accessory.IsEquipped(this))
				{
					accessory.ModifyPlayerWeaponDamage(this, item, ref add, ref mult, ref flat);
				}
			}
			// a bit hacky, will wanna make this nicer eventually
			flat += summonFlatDamage;
		}

		public static readonly PlayerLayer IllusionistRobeLegs = new PlayerLayer("AmuletOfManyMinions", "IllusionistRobeLegs", PlayerLayer.Legs, delegate (PlayerDrawInfo drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			Mod mod = ModLoader.GetMod("AmuletOfManyMinions");
			Texture2D texture;
			// this may not be the most efficient
			if(drawPlayer.armor.Any(item=>item.type == ItemType<IllusionistCorruptRobe>())) 
			{
				texture = mod.GetTexture("Items/Armor/IllusionistArmor/IllusionistCorruptRobe_Legs");
			} else if (drawPlayer.armor.Any(item=>item.type == ItemType<IllusionistCrimsonRobe>()))
			{
				texture = mod.GetTexture("Items/Armor/IllusionistArmor/IllusionistCrimsonRobe_Legs");
			} else
			{
				return;
			}
			Vector2 Position = drawInfo.position;
			Position.Y += 14;
			Color color = Lighting.GetColor((int)(drawPlayer.Center.X / 16), (int)(drawPlayer.Center.Y / 16));
			Vector2 pos = new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2));
			DrawData value = new DrawData(texture, pos, new Microsoft.Xna.Framework.Rectangle?(drawPlayer.legFrame), color, drawPlayer.legRotation, drawInfo.legOrigin, 1f, drawInfo.spriteEffects, 0);
			Main.playerDrawData.Add(value);
		});

		public override void ModifyDrawLayers(List<PlayerLayer> layers)
		{
			int legLayer = layers.FindIndex(layer => layer.Name.Equals("Legs"));
			if(legLayer != -1)
			{
				IllusionistRobeLegs.visible = true;
				layers.Insert(legLayer + 1, IllusionistRobeLegs);
			}
		}
		public List<Projectile> GetMinionsOfType(int projectileType)
		{
			var otherMinions = new List<Projectile>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (other.active && other.owner == player.whoAmI && other.type == projectileType)
				{
					otherMinions.Add(other);
				}
			}
			otherMinions.Sort((x, y) => x.minionPos - y.minionPos);
			return otherMinions;
		}

		public override void PostUpdate()
		{
			if(player.whoAmI != Main.myPlayer)
			{
				return;
			}
			int projectileType = ProjectileType<IllusionistWisp>();
			if(illusionistArmorSetEquipped && GetMinionsOfType(projectileType).Count < 3)
			{
				int buffType = BuffType<IllusionistWispBuff>();
				// this is a hacky check
				bool isCorrupt = player.armor.Any(i => i.type == ItemType<IllusionistCorruptHood>());
				if (!player.HasBuff(buffType))
				{
					player.AddBuff(buffType, IllusionistWisp.SpawnFrequency);
				} else if (player.buffTime[player.FindBuffIndex(buffType)] == 1)
				{
					Projectile.NewProjectile(player.Center, Vector2.Zero, projectileType, (int)(20 * player.minionDamageMult), 0.1f, player.whoAmI, ai0: isCorrupt? 0: 1);
				}
			}
		}
	}

	class MinionSpawningItemGlobalProjectile : GlobalProjectile
	{
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			if (!projectile.minion && !ProjectileID.Sets.MinionShot[projectile.type])
			{
				return;
			}
			Player player = Main.player[projectile.owner];
			MinionSpawningItemPlayer modPlayer = player.GetModPlayer<MinionSpawningItemPlayer>();
			foreach (NecromancerAccessory accessory in NecromancerAccessory.accessories)
			{
				if (accessory.IsEquipped(modPlayer))
				{
					accessory.SpawnProjectileOnChance(projectile, target, damage);
				}
			}
			int wispType = ProjectileType<IllusionistWisp>();
			List<Projectile> wisps = modPlayer.GetMinionsOfType(wispType);
			if (wisps.Count == 3 && wisps.All(p => p.timeLeft <= 10))
			{
				foreach(Projectile wisp in wisps) {
					wisp.ai[1] = target.whoAmI;
				}
			}
		}
	}
}
