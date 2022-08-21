using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleFlyingRangedPet
{
	// Code largely adapted from tModLoader Example Mod
	internal class SampleFlyingRangedPetBuff : ModBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.ZephyrFish;

		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			DisplayName.SetDefault("Sample Flying Ranged Pet");
			Description.SetDefault("Sample Flying Ranged Pet");
		}

		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
		{
			drawParams.DrawColor = Color.LightSkyBlue;
			return true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.buffTime[buffIndex] = 2;
			int projType = ProjectileType<SampleFlyingRangedPetProjectile>();
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] == 0)
			{
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, default, projType, 0, 0, player.whoAmI);
			}
		}
	}

}
