using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.Pathfinding
{
	internal class MinionPathfindingPlayer : ModPlayer
	{
		internal BlockAwarePathfinder pHelper;
		internal uint lastClickFrame = 0;
		internal bool lastMouseRight = false;
		internal Vector2 waypointPosition => pHelper.WaypointPos();
		public override void OnEnterWorld(Player player)
		{
			base.OnEnterWorld(player);
			pHelper = new BlockAwarePathfinder(this.player);
		}

		public override void PostUpdate()
		{
			pHelper?.Update();
			if(player.ownedProjectileCounts[MinionWaypoint.Type] > 0)
			{
				player.MinionAttackTargetNPC = -1;
			}
		}
		internal void ToggleWaypoint(bool remove = false)
		{
			int type = MinionWaypoint.Type;
			if (player.ownedProjectileCounts[type] == 0)
			{
				Projectile.NewProjectile(Main.MouseWorld, Vector2.Zero, type, 0, 0, player.whoAmI);
			}
			else
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.active && p.owner == player.whoAmI && p.type == type)
					{
						if(remove)
						{
							p.Kill();
						} else
						{
							// todo multiplayer sync
							p.position = Main.MouseWorld;
						}
					}
				}
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			// detect double clicks
			if(!triggersSet.MouseRight)
			{
				lastMouseRight = false;
			}
			if(!lastMouseRight && triggersSet.MouseRight)
			{
				if(Main.GameUpdateCount - lastClickFrame < 15)
				{
					ToggleWaypoint(true);
				}
				lastMouseRight = true;
				lastClickFrame = Main.GameUpdateCount;
			}
		}
	}
}
