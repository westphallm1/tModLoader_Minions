using AmuletOfManyMinions.Core;
using AmuletOfManyMinions.Core.Netcode.Packets;
using AmuletOfManyMinions.Items.Armor.IllusionistArmor;
using AmuletOfManyMinions.Projectiles.Squires;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static AmuletOfManyMinions.AmuletOfManyMinions;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.Items.Accessories
{
	public abstract class NecromancerAccessory : ModItem
	{
		internal static List<NecromancerAccessory> accessories;
		protected virtual float spawnVelocity => 0;

		protected virtual float onKillChance => 0;
		protected virtual float onHitChance => 0;
		protected virtual int projType => 0;
		protected virtual int maxTransientMinions => 0;
		protected virtual float baseDamage => 0;

		public override void Load()
		{
			accessories = new List<NecromancerAccessory>();
		}

		public override void Unload()
		{
			accessories?.Clear();
			accessories = null;
		}

		internal virtual void ModifyPlayerWeaponDamage(MinionSpawningItemPlayer necromancerAccessoryPlayer, Item item, ref StatModifier modifier)
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
			shouldSpawnProjectile |= Main.rand.NextFloat() < onHitChance;
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
			Projectile.NewProjectile(player.GetSource_Accessory(this.Item), target.Center, spawnVelocity, projType, (int)(player.GetDamage<SummonDamageClass>().ApplyTo(baseDamage)), 2, player.whoAmI);
			return true;
		}

		internal abstract bool IsEquipped(MinionSpawningItemPlayer player);
	}

}
