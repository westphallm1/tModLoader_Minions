using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace AmuletOfManyMinions.CrossModSystem.SampleMod.Pets.SampleGroundedRangedPet
{
	// Code largely adapted from tModLoader Sample Mod
	internal class SampleGroundedRangedPetBuff : ModBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.PetTurtle;

		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			DisplayName.SetDefault("Sample Grounded Pet");
			Description.SetDefault("Sample Grounded Pet");
		}

		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
		{
			drawParams.DrawColor = Color.SkyBlue;
			return true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.buffTime[buffIndex] = 2;
			int projType = ProjectileType<SampleGroundedRangedPetProjectile>();
			if(player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] == 0)
			{
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, default, projType, 0, 0, player.whoAmI);
			}
		}
	}

}
