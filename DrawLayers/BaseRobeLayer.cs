using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.DrawLayers
{
	public abstract class BaseRobeLayer : PlayerDrawLayer
	{
		public abstract string TexturePath { get; }

		public Asset<Texture2D> Texture { get; protected set; }

		protected abstract int GetAssociatedBodySlot();

		public override void Load()
		{
			Texture = ModContent.Request<Texture2D>(TexturePath);
		}

		public override void Unload()
		{
			Texture = null;
		}

		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.Leggings);
		}

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			//Robes by default are attached to a Body equip type
			if (drawPlayer.dead || drawPlayer.invis || drawPlayer.body == -1)
			{
				return false;
			}
			return drawPlayer.body == GetAssociatedBodySlot();
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;

			Color color = drawPlayer.GetImmuneAlphaPure(drawInfo.colorArmorBody, drawInfo.shadow);

			Texture2D texture = Texture.Value;
			Vector2 drawPos = drawInfo.Position;
			drawPos += -Main.screenPosition + new Vector2(drawPlayer.width / 2 - drawPlayer.bodyFrame.Width / 2, drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.bodyPosition;
			Vector2 legsOffset = drawInfo.legsOffset;

			DrawData drawData = new DrawData(texture, drawPos.Floor() + legsOffset, drawPlayer.legFrame, color, drawPlayer.legRotation, legsOffset, 1f, drawInfo.playerEffect, 0)
			{
				shader = drawInfo.cBody
			};
			drawInfo.DrawDataCache.Add(drawData);
		}
	}
}
