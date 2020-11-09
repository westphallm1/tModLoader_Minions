using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ID;
using AmuletOfManyMinions.Core.Netcode.Packets;

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
			if (player.whoAmI == Main.myPlayer)
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
			if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
			{
				if (NextMousePosition == null || Main.GameUpdateCount % updateRate == 0)
				{
					//If hasn't sent a mouse position recently, or when an update is required

					//Required so client also updates this variable even though its not used directly
					NextMousePosition = Main.MouseWorld;

					//Send packet
					new MousePacket(player.whoAmI, Main.MouseWorld).Send();
				}
			}
		}

		/// <summary>
		/// Called on receiving latest mouse position by server or other clients
		/// </summary>
		public void SetNextMousePosition(Vector2 position)
		{
			if (player.whoAmI != Main.myPlayer)
			{
				NextMousePosition = position;
			}
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
			// All related things aren't null

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
