﻿using AmuletOfManyMinions.Core.Netcode.Packets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core
{
	/// <summary>
	/// Helper ModPlayer used to wrap around Main.MouseWorld so it's multiplayer compatible
	/// </summary>
	public class MousePlayer : ModPlayer
	{
		/*
		 * How this is used:
		 * - Whenever something wants client A's mouse position, access A's players ModPlayer, and calls {GetMousePosition()}
		 *     - It can be null! (if client B hasn't received A's mouse position yet)
		 * 
		 * How this works:
		 * - Client A makes it clear that he wants his mouse position to sync, initiates sending process (by using {SetMousePosition()})
		 *     - Sending process is send MouseWorld every {updateRate} ticks while client wants it
		 * - Server receives, resends to clients (one of them B)
		 *     - B receives {NextMousePosition}, and:
		 *         - if it's the first position received: sets calls {SetNextMousePosition()}
		 *         - else: use UpdateRule() to move {MousePosition} towards the received value
		 *     - If B holds a MousePosition and timeout is reached (no more incoming packets hopefully):
		 *         - nulls {MousePosition} and sets related fields to default
		 */

		/// <summary>
		/// Guard variable to prevent multiple packets being sent per frame
		/// </summary>
		private bool sentThisTick;

		/// <summary>
		/// Primarily used clientside to send regular updates to the server
		/// </summary>
		private int updateRate;

		/// <summary>
		/// Timeout threshold for when to stop expecting updates for mouse position
		/// </summary>
		private int timeout;

		private int timeoutTimer;

		/// <summary>
		/// "Real" mouse position
		/// </summary>
		private Vector2? MousePosition = null;

		/// <summary>
		/// Targeted mouse position
		/// </summary>
		private Vector2? NextMousePosition = null;

		/// <summary>
		/// Previous position (to reset timeout)
		/// </summary>
		private Vector2? OldNextMousePosition = null;

		public override void Initialize()
		{
			Reset();
			timeout = 30;
			updateRate = 5;
			sentThisTick = false;
		}

		public override void PostUpdate()
		{
			UpdateMousePosition();
		}

		/// <summary>
		/// Returns this player's mouse position, accurate if singleplayer/client, interpolated if other client, or null if not available
		/// </summary>
		public Vector2? GetMousePosition()
		{
			if (Player.whoAmI == Main.myPlayer)
			{
				return Main.MouseWorld;
			}
			return MousePosition;
		}

		/// <summary>
		/// Called by the local client only
		/// </summary>
		public void SetMousePosition()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
			{
				if (!sentThisTick && (NextMousePosition == null || Main.GameUpdateCount % updateRate == 0))
				{
					//If hasn't sent a mouse position recently, or when an update is required
					sentThisTick = true;

					//Send packet
					if (NextMousePosition != Main.MouseWorld)
					{
						new MousePacket(Player, Main.MouseWorld).Send();
					}
					else
					{
						//If mouse position didn't change, reset timeout and send a packet that does the same
						ResetTimeout();
						new MouseResetTimeoutPacket(Player).Send();
					}

					//Required so client also updates this variable even though its not used directly
					NextMousePosition = Main.MouseWorld;
				}
			}
		}

		/// <summary>
		/// Called on receiving latest mouse position by server or other clients
		/// </summary>
		public void SetNextMousePosition(Vector2 position)
		{
			if (Player.whoAmI != Main.myPlayer)
			{
				NextMousePosition = position;
			}
		}

		/// <summary>
		/// Called on receiving latest keepalive packet by server or other clients
		/// </summary>
		public void ResetTimeout()
		{
			timeoutTimer = 0;
		}

		/// <summary>
		/// Return a new position based on current and final
		/// </summary>
		private Vector2 UpdateRule(Vector2 current, Vector2 final)
		{
			return Vector2.Lerp(current, final, 2 * 1f / updateRate);
		}

		/// <summary>
		/// Clears all sync related fields
		/// </summary>
		private void Reset()
		{
			MousePosition = null;
			NextMousePosition = null;
			OldNextMousePosition = null;
			timeoutTimer = 0;
		}

		private void UpdateMousePosition()
		{
			sentThisTick = false;

			if (NextMousePosition == null)
			{
				//No pending position to sync
				return;
			}
			else if (MousePosition == null)
			{
				//New incoming position
				MousePosition = NextMousePosition;
				timeoutTimer++;
				return;
			}

			//If here:
			// -Non-mouse-owner client (or server)
			// -Mouse needs updating
			// -All related things aren't null

			if (timeoutTimer++ < timeout)
			{
				if (OldNextMousePosition != NextMousePosition)
				{
					//Received new position: reset timeout timer
					timeoutTimer = 0;
				}

				OldNextMousePosition = NextMousePosition;

				if (MousePosition == null || NextMousePosition == null)
				{
					//Hard failsafe, shouldn't happen
					return;
				}

				//Vector2? to Vector2 conversion
				Vector2 mousePos = MousePosition ?? Vector2.Zero;
				Vector2 nextMousePos = NextMousePosition ?? Vector2.Zero;

				MousePosition = UpdateRule(mousePos, nextMousePos);
			}
			else
			{
				Reset();
			}
		}
	}
}
