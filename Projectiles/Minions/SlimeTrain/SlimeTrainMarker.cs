using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Projectiles.Minions.SlimeTrain
{
	/// <summary>
	/// Uses ai[0] for the NPC to attack, ai[1] for empower count,
	/// localAi[0] to count up animation frames
	/// </summary>
	class SlimeTrainMarkerProjectile: ModProjectile
	{
		// npc to stay on top of
		NPC clingTarget;
		// 'spawn' animation time
		public static int SetupTime = 90;
		// frames to run attack for after spawning in
		public static int BaseAttackTime = 240;

		// extra empower time for each power up
		int PerEmpowerTime = 30;
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.projFrames[projectile.type] = 6;
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			projectile.width = 16;
			projectile.height = 16;
			projectile.penetrate = -1;
			projectile.friendly = false;
			projectile.usesLocalNPCImmunity = true;
			projectile.timeLeft = SetupTime + BaseAttackTime;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}
		public override void AI()
		{
			base.AI();
			projectile.localAI[0]++;
			if (projectile.ai[0] == 0)
			{
				// failsafe in case we got a bad NPC index
				projectile.Kill();
				return; 
			}
			// "on spawn" code
			if (clingTarget == null)
			{
				clingTarget = Main.npc[(int)projectile.ai[0]];
				projectile.timeLeft += (int)projectile.ai[1] * PerEmpowerTime;
			}
			if (!clingTarget.active)
			{
				projectile.Kill();
				return;
			}
			projectile.Center = clingTarget.Center;
		}
	}
}
