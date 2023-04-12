using AmuletOfManyMinions.Projectiles.Minions.VanillaClones;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.CrossModClient.SummonersShine
{
	internal static class Hornet
	{

		internal static int ModSupport_SummonersShineHornetCystID;
		internal static void ApplyCrossModChanges()
		{
			if (ModLoader.TryGetMod("SummonersShine", out Mod summonersShine))
				ModSupport_SummonersShineHornetCystID = summonersShine.Find<ModProjectile>("HornetCyst").Type;
		}

		internal static void SetDeaults_Hornet(Projectiles.Minions.VanillaClones.HornetMinion Hornet) {

			Hornet.hsHelper.CustomFireProjectile = (i, j, k) => Hornet_CustomFireProjectile (Hornet, i, j, k);
		}
		private static void Hornet_CustomFireProjectile(Projectiles.Minions.VanillaClones.HornetMinion Hornet, Vector2 lineOfFire, int projId, float ai0)
		{
			if (!CrossMod.GetSummonersShineIsCastingSpecialAbility(Hornet.Projectile, ItemType<HornetMinionItem>()))
			{
				Hornet.hsHelper.FireProjectile(lineOfFire, projId, ai0);
				return;
			}
			Projectile.NewProjectile(
				Hornet.Projectile.GetSource_FromThis(),
				Hornet.Projectile.Center,
				Hornet.Behavior.VaryLaunchVelocity(lineOfFire),
				ModSupport_SummonersShineHornetCystID,
				Hornet.Projectile.damage,
				Hornet.Projectile.knockBack,
				Hornet.Projectile.owner,
				ai0: ai0);
		}
	}
}
