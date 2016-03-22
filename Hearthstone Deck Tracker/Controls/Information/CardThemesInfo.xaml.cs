﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Themes;
using CardIds = HearthDb.CardIds;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Information
{
	/// <summary>
	/// Interaction logic for CardThemesInfo.xaml
	/// </summary>
	public partial class CardThemesInfo : UserControl
	{
		private readonly List<Hearthstone.Card> _cards = new List<Hearthstone.Card>();
		private bool _update = true;

		private readonly string[] _demoCards =
		{
			CardIds.Collectible.Neutral.GilblinStalker,
			CardIds.Collectible.Priest.NorthshireCleric,
			CardIds.Collectible.Neutral.GilblinStalker,
			CardIds.Collectible.Priest.UpgradedRepairBot,
			CardIds.Collectible.Neutral.GarrisonCommander,
			CardIds.Collectible.Neutral.YouthfulBrewmaster,
			CardIds.Collectible.Priest.UpgradedRepairBot,
			CardIds.Collectible.Priest.NorthshireCleric
		};

		public CardThemesInfo()
		{
			InitializeComponent();
			UpdateAnimatedCardListAsync();
		}

		public Hearthstone.Card Card => Database.GetCardFromId(CardIds.Collectible.Neutral.RagnarosTheFirelord);
		public ImageBrush ClassicCard => GetBarImageBuilder(ThemeManager.Themes.First(x => x.Name == "classic"), Card).Build();
		public ImageBrush MinimalCard => GetBarImageBuilder(ThemeManager.Themes.First(x => x.Name == "minimal"), Card).Build();
		public ImageBrush DarkCard => GetBarImageBuilder(ThemeManager.Themes.First(x => x.Name == "dark"), Card).Build();
		public ImageBrush LightCard => GetBarImageBuilder(ThemeManager.Themes.First(x => x.Name == "light"), Card).Build();

		private async void UpdateAnimatedCardListAsync()
		{
			foreach(var cardId in _demoCards)
			{
				var card = _cards.FirstOrDefault(x => x.Id == cardId);
				if(card == null)
					_cards.Add(Database.GetCardFromId(cardId));
				else
					card.Count++;
			}
			AnimatedCardList.Update(_cards.ToSortedCardList().Select(x => (Hearthstone.Card)x.Clone()).ToList(), true, true);
			while(_update)
			{
				foreach(var cardId in _demoCards)
				{
					if(!_update)
						break;
					await Task.Delay(2000);
					var card = _cards.FirstOrDefault(x => x.Id == cardId);
					if(card != null)
					{
						if(card.Count == 1)
							_cards.Remove(card);
						else
							card.Count--;
					}
					AnimatedCardList.Update(_cards.ToSortedCardList().Select(x => (Hearthstone.Card)x.Clone()).ToList(), false, false);
				}
				foreach(var cardId in _demoCards)
				{
					if(!_update)
						break;
					await Task.Delay(2000);
					var card = _cards.FirstOrDefault(x => x.Id == cardId);
					if(card == null)
						_cards.Add(Database.GetCardFromId(cardId));
					else
						card.Count++;
					AnimatedCardList.Update(_cards.ToSortedCardList().Select(x => (Hearthstone.Card)x.Clone()).ToList(), true, false);
				}
			}
		}

		public static CardBarImageBuilder GetBarImageBuilder(Theme theme, Hearthstone.Card card)
		{
			var buildType = theme.BuildType ?? typeof(DefaultBarImageBuilder);
			return (CardBarImageBuilder)Activator.CreateInstance(buildType, card, theme.Directory);
		}

		private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
		{
			if(AnimatedCardList == null)
				return;
			var tb = sender as ToggleButton;
			if(tb != null)
				ThemeManager.SetTheme(tb.Content.ToString().ToLower());
			foreach(var card in AnimatedCardList.Items.Cast<AnimatedCard>().Select(x => x.Card))
			{
				card.UpdateHighlight();
				card.Update();
			}
		}

		private void CardThemesInfo_OnUnloaded(object sender, RoutedEventArgs e) => _update = false;
	}
}