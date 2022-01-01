using AmuletOfManyMinions.Projectiles.Minions.CombatPets.ElementalPals;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.NPCs
{
	internal class DebuffGlobalNPC: GlobalNPC
	{
		public short cellStack;
		public short pygmySpearStack;
		public short flameVortexStack;

		public override bool InstancePerEntity => true;
		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if(cellStack > 0)
			{
				if(npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 2 * 20 * cellStack;
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
				npc.lifeRegen -= 2 * 10 * pygmySpearStack;
				if(damage < 5 * pygmySpearStack)
				{
					damage = 5 * pygmySpearStack;
				}
			}

			if(flameVortexStack > 0)
			{
				if(npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 2 * 15 * flameVortexStack;
				if(damage < 5 * flameVortexStack)
				{
					damage = 5 * flameVortexStack;
				}
			}
		}

		public override void PostAI(NPC npc)
		{
			base.PostAI(npc);
			if(flameVortexStack > 0 && !npc.HasBuff<FlareVortexDebuff>())
			{
				flameVortexStack = 0;
			} else if (flameVortexStack > 0)
			{
				AddFlareVortextDustEffect(npc);
			}
		}

		private void AddFlareVortextDustEffect(NPC npc)
		{
			int frame = (int)Main.GameUpdateCount;
			float yCycle = 30;
			float xCycle = 15;
			float yFraction = (frame % yCycle) / yCycle;
			float xFraction = (frame % xCycle) / xCycle;

			float radius = 1.25f * npc.width * yFraction;
			float offset = radius * MathF.Sin(MathHelper.TwoPi * xFraction);
			float yPos = npc.Bottom.Y - npc.Hitbox.Height * yFraction;
			int width = 16;
			int height = 16;
			for(int sign = -1; sign <= 1; sign += 2)
			{
				float xPos = npc.Center.X + sign * offset;
				int dustId = Dust.NewDust(
					new Vector2(xPos, yPos) - new Vector2(width, height) /2 , width, height, 6, 
					npc.velocity.X * 0.2f, npc.velocity.Y * 0.2f, 
					100, default, 2f);
				Main.dust[dustId].noGravity = true;
				Main.dust[dustId].velocity.X *= 0.3f;
				Main.dust[dustId].velocity.Y *= 0.3f;
				Main.dust[dustId].noLight = true;
			}
		}

	}
}
