using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.NPCs
{
	internal class DebuffGlobalNPC: GlobalNPC
	{
		public short cellStack;
		public short pygmySpearStack;

		public override bool InstancePerEntity => true;
		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if(cellStack > 0)
			{
				if(npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 20 * cellStack;
				if(damage < 10 * cellStack)
				{
					damage = 10 * cellStack;
				}
			}

			if(pygmySpearStack > 0)
			{
				if(npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 10 * pygmySpearStack;
				if(damage < 5 * pygmySpearStack)
				{
					damage = 5 * pygmySpearStack;
				}
			}
		}
	}
}
