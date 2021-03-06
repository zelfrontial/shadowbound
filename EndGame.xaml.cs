﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Lab
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EndGame : Page
    {
        MainPage parent;
        public EndGame(MainPage parent)
        {
            this.parent = parent;
            this.InitializeComponent();
            ScoreText.Text = parent.game.score.ToString();

            SoundEffect menuSoundEffect = new SoundEffect(@"Content\Dark_Laugh.wav", false);
            menuSoundEffect.Play();
        }
        private void RestartGame(object sender, RoutedEventArgs e)
        {
            parent.game.started = false;
            parent.game.name = playerName.Text;
            parent.game.score = Convert.ToInt32(ScoreText.Text);
            storingScore(parent.game.name, parent.game.score);
            parent.game.score = 0;
            parent.restartGame();
            parent.Children.Remove(this);  
        }
        private void goToMainMenu(object sender, RoutedEventArgs e)
        {
            /*parent.restartGame();
            parent.game.started = false;

            float finderSpeed = parent.game.finderSpeed;
            float followerSpeed = parent.game.followerSpeed;
            parent.game.name = playerName.Text;
            parent.game.score = Convert.ToInt32(ScoreText.Text);
            storingScore(parent.game.name, parent.game.score);

            parent.game.score = 0;

            parent.mainMenu = new MainMenu(parent);
            parent.mainMenu.cmdStart.Content = "Start";
            parent.mainMenu.cmdRestart.Visibility = Visibility.Collapsed;
            parent.mainMenu.enemySpeedSld.Value = finderSpeed;
            parent.mainMenu.followerSpeedSld.Value = followerSpeed;

            parent.Children.Add(parent.mainMenu);

            parent.Children.Remove(this);*/
            parent.game.name = playerName.Text;
            parent.game.score = Convert.ToInt32(ScoreText.Text);
            storingScore(parent.game.name, parent.game.score);
            parent.game.started = false;
            float finderSpeed = parent.game.finderSpeed;
            float followerSpeed = parent.game.followerSpeed;

            parent.restartGame();
            parent.game.started = false;

            parent.mainMenu = new MainMenu(parent);
            parent.mainMenu.cmdStart.Content = "Start";
            parent.mainMenu.cmdRestart.Visibility = Visibility.Collapsed;
            parent.mainMenu.enemySpeedSld.Value = finderSpeed;
            parent.mainMenu.followerSpeedSld.Value = followerSpeed;
            parent.Children.Add(parent.mainMenu);
            
            parent.game.score = 0;
            
            parent.Children.Remove(this);
           
        }

        private void storingScore(string name, int score)
        {
            if (score > 0)
            {
                var task = parent.game.WriteDataToFileAsync(parent.game.filename, name + "\t" + score + "\n");
                task.ConfigureAwait(false);
            }
            
        }
    }
}
