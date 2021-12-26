using AmuletOfManyMinions.Core.Minions.CombatPetsQuiz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static AmuletOfManyMinions.UI.CombatPetsQuizUI.CombatPetsQuizUIMain;

namespace AmuletOfManyMinions.UI.CombatPetsQuizUI
{

	/// <summary>
	/// Dialog panel that displays under the player while they're taking the
	/// Elemental Pals Personality Quiz
	/// </summary>
	class CombatPetsQuizUIMain : UIElement
	{
		public static readonly float MaxTextboxWidth = 600f;
		public static readonly float MinTextboxWidth = 120f;
		// answer panel height set dynamically
		public static readonly float MarginSize = 8f;
		public static float LineHeight { get; private set; }

		public static float QuestionPanelHeight => 3 * LineHeight + 2 * MarginSize;
		public static DynamicSpriteFont TextFont => FontAssets.MouseText.Value;

		private CombatPetsQuizUIPanel questionPanel;
		private CombatPetsQuizUIPanel answerPanel;

		public CombatPetsQuizModPlayer ModPlayer => Main.player[Main.myPlayer].GetModPlayer<CombatPetsQuizModPlayer>();
		public bool IsActive => ModPlayer.IsTakingQuiz;
		public override void OnInitialize()
		{
			base.OnInitialize();
			LineHeight = TextFont.MeasureString(" ").Y;
			questionPanel = new CombatPetsQuizUIPanel();
			answerPanel = new CombatPetsQuizUIPanel() { AllowClickText = true };
			Append(questionPanel);
			Append(answerPanel);
			answerPanel.OnMouseUp += this.AnswerPanel_OnMouseUp;

			
		}

		private void AnswerPanel_OnMouseUp(UIMouseEvent evt, UIElement listeningElement)
		{
			if(answerPanel.HighlightedLine > -1)
			{
				ModPlayer.CurrentQuiz.AnswerQuestion(answerPanel.HighlightedLine);
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if(!IsActive) { return; }
			base.DrawSelf(spriteBatch);
		}
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			if(!IsActive) { return; }
			base.DrawChildren(spriteBatch);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if(IsActive)
			{
				CombatPetsQuizQuestion currentQuestion = ModPlayer.CurrentQuiz.CurrentQuestion;
				questionPanel.TextLines = TextFont.CreateWrappedText(
					currentQuestion.QuestionText, MaxTextboxWidth - 4 * MarginSize).Split('\n');
				answerPanel.TextLines = currentQuestion.AnswerTexts;
				AnchorToScreenPosition();
			} else
			{
				// shove off of the bottom of the screen
				Top.Pixels = Main.screenHeight;
				return;
			}
		}

		private void AnchorToScreenPosition()
		{
			Vector2 answerPanelSize = answerPanel.MeasureLines();
			answerPanelSize.X = Math.Max(answerPanelSize.X + 4 * MarginSize, MinTextboxWidth);
			// No real reason to do this all in one method, but...

			// Set the size of the parent container
			float screenCenterX = Main.screenWidth / 2;
			float totalHeight = QuestionPanelHeight + answerPanelSize.Y + 5 * MarginSize;
			// fudge factor for margins
			Top.Pixels = Main.screenHeight - totalHeight;
			Height.Pixels = totalHeight;
			Left.Pixels = screenCenterX - MaxTextboxWidth / 2;
			Width.Pixels = MaxTextboxWidth;

			// Set the size of the narration text box (fixed)
			questionPanel.Top.Pixels = Height.Pixels - QuestionPanelHeight - 2 * MarginSize;
			questionPanel.Height.Pixels = QuestionPanelHeight;
			questionPanel.Left.Pixels = 0;
			questionPanel.Width.Pixels = MaxTextboxWidth;

			// Set the size of the answer options text box dynamic based on text
			answerPanel.Top.Pixels = 0;
			answerPanel.Height.Pixels = answerPanelSize.Y + 2 * MarginSize;
			answerPanel.Left.Pixels = MaxTextboxWidth - answerPanelSize.X - 2 * MarginSize;
			answerPanel.Width.Pixels = answerPanelSize.X + 2 * MarginSize;
		}


	}

	class CombatPetsQuizUIPanel : UIPanel
	{

		// todo change this dynamically
		internal string[] TextLines;

		internal bool AllowClickText;

		internal int HighlightedLine { get; private set; } = -1;

		public override void OnInitialize()
		{
			base.OnInitialize();
			IgnoresMouseInteraction = false;
		}

		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			base.DrawChildren(spriteBatch);
			for(int i = 0; i < TextLines.Length; i++)
			{
				DrawText(spriteBatch, TextLines[i], i);
			}
		}

		public Vector2 MeasureLines()
		{
			float ySum = LineHeight * TextLines.Length;
			float xMax = TextLines.Select(tl => TextFont.MeasureString(tl).X).Max();
			return new(xMax, ySum);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if(AllowClickText && TextLines != default && ContainsPoint(Main.MouseScreen))
			{
				float top = Parent.Top.Pixels + Top.Pixels + MarginSize;
				HighlightedLine = (int)((Main.MouseScreen.Y - top) / LineHeight);
			} else
			{
				HighlightedLine = -1;
			}
		}

		private void DrawText(SpriteBatch spriteBatch, string text, int line)
		{
			// TODO line wrap? Maybe?
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			float xPos = Parent.Left.Pixels + Left.Pixels + 2 * MarginSize;
			float yPos = Parent.Top.Pixels + Top.Pixels + line * LineHeight + MarginSize;
			// budget text outline
			Vector2 pos = new Vector2(xPos, yPos);
			for(int i = -1; i < 2; i += 2)
			{
				for(int j = -1; j < 2; j += 2)
				{
					spriteBatch.DrawString(font, text, pos + new Vector2(i, j), Color.Black);
				}
			}
			Color textColor = line == HighlightedLine ? Color.Yellow : Color.White;
			spriteBatch.DrawString(font, text, pos, textColor);
		}
	}

	public class CombatQuizClickSuppressor: GlobalItem
	{
		// TODO figure out why this needs to be manually suppressed
		public override bool CanUseItem(Item item, Player player)
		{
			if(player.whoAmI == Main.myPlayer && UserInterfaces.quizUI.IsMouseHovering)
			{
				return false;
			}
			return base.CanUseItem(item, player);
		}

	}
}
