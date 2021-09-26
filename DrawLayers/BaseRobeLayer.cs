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

		protected abstract bool VisibleWithBody(int bodySlot);

		public override void Load()
		{
			//The equip tex will be assigned to the legs slot
			//slotId = Mod.AddEquipTexture(null, EquipType.Legs, TexturePath);
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
			return VisibleWithBody(drawPlayer.body);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player drawPlayer = drawInfo.drawPlayer;

			Color color = drawPlayer.GetImmuneAlphaPure(Lighting.GetColor((int)(drawPlayer.Center.X / 16), (int)(drawPlayer.Center.Y / 16)), drawInfo.shadow);

			Texture2D texture = Texture.Value;
			Vector2 drawPos = drawInfo.Position;
			drawPos.Y += 6;
			drawPos += -Main.screenPosition + new Vector2(drawPlayer.width / 2 - drawPlayer.bodyFrame.Width / 2, drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.bodyPosition;
			//drawPos += new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2);
			Vector2 legsOffset = drawInfo.legsOffset;
			DrawData drawData = new DrawData(texture, drawPos.Floor() + legsOffset, drawPlayer.legFrame, color, drawPlayer.legRotation, legsOffset, 1f, drawInfo.playerEffect, 0)
			{
				shader = drawInfo.cBody
			};
			drawInfo.DrawDataCache.Add(drawData);

			/*
			Vector2 Position = drawInfo.position;
			Position.Y += 14;
			Color color = Lighting.GetColor((int)(drawPlayer.Center.X / 16), (int)(drawPlayer.Center.Y / 16));
			Vector2 pos = new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2));
			DrawData value = new DrawData(texture, pos, new Microsoft.Xna.Framework.Rectangle?(drawPlayer.legFrame), color, drawPlayer.legRotation, drawInfo.legOrigin, 1f, drawInfo.spriteEffects, 0);
			value.shader = drawPlayer.cBody;
			Main.playerDrawData.Add(value);
			 */
		}
	}
}
