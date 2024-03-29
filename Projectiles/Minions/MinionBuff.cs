﻿using AmuletOfManyMinions.Core.Minions.Tactics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions
{
	public abstract class MinionBuff : ModBuff
	{

		internal int[] projectileTypes;
		internal abstract int[] ProjectileTypes { get; }

		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			projectileTypes = ProjectileTypes;
			MinionTacticsGroupMapper.AddBuffMapping(this);
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (projectileTypes.Select(p => player.ownedProjectileCounts[p]).Sum() > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
