using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BitSits_Framework
{
    /// <summary>
    /// The loading screen coordinates transitions between the menu system and the
    /// game itself. Normally one screen will transition off at the same time as
    /// the next screen is transitioning on, but for larger transitions that can
    /// take a longer time to load their data, we want the menu system to be entirely
    /// gone before we start loading the game. This is done as follows:
    /// 
    /// - Tell all the existing screens to transition off.
    /// - Activate a loading screen, which will transition on at the same time.
    /// - The loading screen watches the state of the previous screens.
    /// - When it sees they have finished transitioning off, it activates the real
    ///   next screen, which may take a long time to load its data. The loading
    ///   screen will be the only thing displayed while this load is taking place.
    /// </summary>
    class LoadingScreen : GameScreen
    {
        #region Fields


        Camera2D camera = new Camera2D();

        bool loadingIsSlow;
        bool otherScreensAreGone;

        GameScreen[] screensToLoad;

        Texture2D background;

        const string message = "Loading...";
        SpriteFont font;


        #endregion

        #region Initialization


        /// <summary>
        /// The constructor is private: loading screens should
        /// be activated via the static Load method instead.
        /// </summary>
        private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow,
                              GameScreen[] screensToLoad)
        {
            this.loadingIsSlow = loadingIsSlow;
            this.screensToLoad = screensToLoad;

            if (loadingIsSlow)
            {
                TransitionOnTime = TimeSpan.FromSeconds(.1);
                TransitionOffTime = TimeSpan.FromSeconds(2);
            }
            else TransitionOnTime = TimeSpan.FromSeconds(.1);
        }


        /// <summary>
        /// Activates the loading screen.
        /// </summary>
        public static void Load(ScreenManager screenManager, bool loadingIsSlow,
                                PlayerIndex? controllingPlayer,
                                params GameScreen[] screensToLoad)
        {
            // Tell all the current screens to transition off.
            foreach (GameScreen screen in screenManager.GetScreens())
                screen.ExitScreen();

            // Create and activate the loading screen.
            LoadingScreen loadingScreen = new LoadingScreen(screenManager,
                                                            loadingIsSlow,
                                                            screensToLoad);

            screenManager.AddScreen(loadingScreen, controllingPlayer);
        }


        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            font = content.Load<SpriteFont>("Fonts/debugFont");
            background = content.Load<Texture2D>("Graphics/BitSitsGames");
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the loading screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // If all the previous screens have finished transitioning
            // off, it is time to actually perform the load.
            if (otherScreensAreGone)
            {
                ScreenManager.LoadGameContent();

                this.ExitScreen();

                // when all screens including this screen is removed
                if (ScreenManager.GetScreens().Length == 0)
                    foreach (GameScreen screen in screensToLoad)
                    {
                        if (screen != null)
                        {
                            ScreenManager.AddScreen(screen, ControllingPlayer);
                        }
                    }
            }
        }


        /// <summary>
        /// Draws the loading screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // If we are the only active screen, that means all the previous screens
            // must have finished transitioning off. We check for this in the Draw
            // method, rather than in Update, because it isn't enough just for the
            // screens to be gone: in order for the transition to look good we must
            // have actually drawn a frame without them before we perform the load.
            if ((ScreenState == ScreenState.Active) &&
                (ScreenManager.GetScreens().Length == 1))
            {
                otherScreensAreGone = true;
            }

            // The gameplay screen takes a while to load, so we display a loading
            // message while that is going on, but the menus load very quickly, and
            // it would look silly if we flashed this up for just a fraction of a
            // second while returning from the game to the menus. This parameter
            // tells us how long the loading is going to take, so we know whether
            // to bother drawing the message.
            if (loadingIsSlow)
            {
                SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

                // Center the text in the viewport.
                Vector2 textSize = font.MeasureString(message);
                Vector2 textPosition = (Camera2D.BaseScreenSize - textSize) / 2;

                Color color = Color.White * TransitionAlpha;

                // Draw the background and text.
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.Transform);

                if (background == null)
                    spriteBatch.DrawString(font, message, textPosition, color);
                else
                    spriteBatch.Draw(background, Vector2.Zero, color);

                spriteBatch.End();
            }
        }


        #endregion
    }
}
