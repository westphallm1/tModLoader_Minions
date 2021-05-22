using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Netcode
{
	public static class NetUtils
	{
		// Approximate size of a player's screen, in pixels
		public static Vector2 ScreenSize1080P = new Vector2(1920, 1080);

		// Number of screens away to sync position variables across,
		// also accomodate players with larger screens
		public static float ScreenSyncRange = 3f;

		// Higher order functions are always fun
		/// <summary>
		/// Return a delegate function that determinines whether to send a PlayerPacket to another player
		/// Returns true if player is within a few screens of the event, false otherwise
		/// </summary>
		/// <param name="eventPosition"></param>
		/// <returns></returns>
		public static Func<Player, bool> EventProximityDelegate(Vector2 eventPosition)
		{
			//Only send to other player if the mouse would be in visible range
			return (Player otherPlayer) =>
			{
				Rectangle otherPlayerBounds = Utils.CenteredRectangle(otherPlayer.Center, ScreenSize1080P * ScreenSyncRange);
				Point mousePoint = eventPosition.ToPoint();
				return otherPlayerBounds.Contains(mousePoint);
			};
		}

	}
}
